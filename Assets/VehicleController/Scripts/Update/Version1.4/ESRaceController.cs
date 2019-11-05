using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ESRaceController : MonoBehaviour {
    [System.Serializable]
    public class VehicleSettings
    {
        public string VehicleName = "Name";
        public Transform _VehiclePrefab;
        public bool UseCutsomRotation;
        public Vector3 _EulerAngles;
        public Transform StartPoint;
        public Vector3 OffsetPostion;
        public Vector3 EulerStartRot;
        public Vector3 pos;
        public Quaternion rot;
        public int currentnode;
    }

    public List<VehicleSettings> m_vehiclesettings = new List<VehicleSettings>(1);
    

    public Transform[] vehicle;
    public Transform nodeparent;
    public List<Transform> nodes;
    public float DistanceApartFromNode = 200;

   
  
    public int classindex = 0;
    public bool[] m_vehiclesettingsfoldout = new bool[1];

    void Start()
    {
        Transform[] pathtrans = nodeparent.GetComponentsInChildren<Transform>();
        nodes = new List<Transform>();
        for (int i = 0; i < pathtrans.Length; i++)
        {
            if (pathtrans[i] != nodeparent.transform)
            {
                nodes.Add(pathtrans[i]);
            }
        }
        //
        System.Array.Resize(ref vehicle, m_vehiclesettings.Count);
        for (int i = 0; i < m_vehiclesettings.Count; i++)
        {
            vehicle[i] = Instantiate(m_vehiclesettings[i]._VehiclePrefab, m_vehiclesettings[i].StartPoint.position , Quaternion.identity);
            vehicle[i].Rotate(m_vehiclesettings[i].EulerStartRot);    
        }
    }
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < m_vehiclesettings.Count; i++)
        {
            if (vehicle[i] != null)
            {
                if (vehicle[i].GetComponent<ESSpawnManager>()._destroy)
                {
                    if (vehicle[i].GetComponent<ESVehicleAI>() != null)
                    {

                        m_vehiclesettings[i].currentnode = vehicle[i].GetComponent<ESVehicleAI>().currentnode;

                    }
                    m_vehiclesettings[i].pos = vehicle[i].GetComponent<ESSpawnManager>().pos;
                    m_vehiclesettings[i].rot = vehicle[i].GetComponent<ESSpawnManager>().rot;
                    Destroy(vehicle[i].gameObject);
                    vehicle[i] = Instantiate(m_vehiclesettings[i]._VehiclePrefab,m_vehiclesettings[i].pos + m_vehiclesettings[i].OffsetPostion,m_vehiclesettings[i].rot);
                    if (vehicle[i].GetComponent<ESVehicleAI>() != null)
                    {
                       
                            vehicle[i].GetComponent<ESVehicleAI>().currentnode =m_vehiclesettings[i].currentnode;
             
                    }
                    
                    if (m_vehiclesettings[i].UseCutsomRotation)
                    {
                        vehicle[i].Rotate(m_vehiclesettings[i]._EulerAngles);
                    }
                }
            }
        }
    }
}
