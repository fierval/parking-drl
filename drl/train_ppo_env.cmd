REM rd models /q/s
REM rd summaries /q/s
mlagents-learn config\ppo_trainer_config.yaml --env ..\env\cars.exe --run-id ppo-3 --keep-checkpoints 10 --train --width 800 --height 800 --quality-level 0 --time-scale 20  --curriculum=config\curricula\Parking