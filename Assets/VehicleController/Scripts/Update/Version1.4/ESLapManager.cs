using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESLapManager : MonoBehaviour 
{
    public bool BeginRace;
    public GameObject _racectrl;

    public float CountDown =  5;
    public float CeilCountDown;
   
    public float ang;
    public ESVehicleAI ai;
    public ESVehicleController vehiclectrl;

    void Start()
    {
        _racectrl = GameObject.Find("RaceController");
        
        if (transform.GetComponent<ESVehicleController>() != null)
        {
            vehiclectrl = GetComponent<ESVehicleController>();
        }
        if (transform.GetComponent<ESVehicleAI>() != null)
        {
            ai = GetComponent<ESVehicleAI>();
        }
     
    }
	// Update is called once per frame
	void Update ()
    {
       
        if (CountDown >= 1)
        {
            CountDown -= Time.deltaTime;
        }
        if (CountDown <= 0)
        {
            CountDown = 1;
        }
        CeilCountDown = Mathf.Ceil(CountDown);
       
        //
        if (CeilCountDown == 1)
        {
            BeginRace = true;
        }
        //
        if (!BeginRace)
        {    
            if (ai != null)
            {
                ai.mul = 0;
            }
            if (vehiclectrl != null)
            {
                vehiclectrl.mul = 0;
            }
        }
        if (BeginRace)
        {
           
            if (ai != null)
            {
                ai.mul = 1;
            }
            if (vehiclectrl != null)
            {
                vehiclectrl.mul = 1;
            }
        }

        for (int i = 0; i < _racectrl.GetComponent<ESRaceController>().nodes.Count; i++)
        {
           
            if (Vector3.Distance(transform.position, _racectrl.GetComponent<ESRaceController>().nodes[i].transform.position) < 50)
            {

                ang  = Vector3.Dot(transform.forward, _racectrl.GetComponent<ESRaceController>().nodes[i].transform.forward);
            }
        }

          
	}

   
}
