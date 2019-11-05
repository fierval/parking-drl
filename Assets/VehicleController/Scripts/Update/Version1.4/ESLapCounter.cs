using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ESLapCounter : MonoBehaviour {
    public int currentlap = 0;
    public int maxlap;
    public bool EndRace;
    

	// Use this for initialization
	void Start () {
		
	}
	
	
	void Update () {
        //
       
        if (currentlap == maxlap)
            EndRace = true;
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "LapTrigger")
        {
            if (currentlap < maxlap)
                currentlap++;
        }
    }
}
