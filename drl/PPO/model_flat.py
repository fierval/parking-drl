import torch
import torch.nn as nn
import numpy as np

def xavier(layer):
    if isinstance(layer, nn.Conv2d) or isinstance(layer, nn.Linear):
        nn.init.xavier_uniform_(layer.weight)
device = torch.device("cuda:0" if torch.cuda.is_available() else "cpu")

class ActorCritic(nn.Module):

    LAST_LAYER_DIM = 128

    def __init__(self, obs_size, act_size, model_path=None):
        '''
        obs_size - 1D visual observation
        act_size - action space size
        '''
        super().__init__()

        self.action_dim = act_size
        self.state_dim = obs_size
        
        self.fc_hidden = self.hidden_layers()

        fc_critic = self.fc_hidden \
            + [ nn.Linear(self.LAST_LAYER_DIM, self.LAST_LAYER_DIM // 2),
                nn.Tanh(),
                nn.Linear(self.LAST_LAYER_DIM // 2, 1)]

        fc_actor = self.fc_hidden \
            + [ nn.Linear(self.LAST_LAYER_DIM, self.LAST_LAYER_DIM // 2),
                nn.Tanh(),
                nn.Linear(self.LAST_LAYER_DIM // 2, self.action_dim), 
                nn.Tanh()]
        
        self.actor = nn.Sequential(*fc_actor).to(device)
        self.critic = nn.Sequential(*fc_critic).to(device)
        self.log_std = nn.Parameter(torch.zeros(1, act_size)).to(device)

        print(f"Actor: {self.actor}")
        print(f"Critic: {self.critic}")
        
        if model_path is None:
            self.init_weights()
        else:
            self.load_state_dict(torch.load(model_path))


    def init_weights(self):
        self.actor.apply(xavier)
        self.critic.apply(xavier)

    def hidden_layers(self):
        return [
            nn.Linear(self.state_dim, 512),
            nn.Tanh(),
            nn.Linear(512, 256),
            nn.Tanh(),
            nn.Linear(256, self.LAST_LAYER_DIM)
        ]

    def forward(self, x, actions=None):
        value = self.critic(x).squeeze(-1)
        mu = self.actor(x)

        std = self.log_std.exp().expand_as(mu)
        dist = torch.distributions.Normal(mu, std)

        # actions are [-1, 1]
        if actions is None:
            actions = dist.sample()

        log_prob = dist.log_prob(actions)
        entropy = dist.entropy()

        log_prob = torch.sum(log_prob, dim=-1)
        entropy = torch.sum(dist.entropy(), dim=-1)

        return actions, log_prob, entropy, value

    def state_values(self, states):
        return self.critic(states)