using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[AddComponentMenu("EasyVehicleSystem/ESAudioSystem")]
[RequireComponent(typeof(AudioSource))]
public class ESAudioSystem : MonoBehaviour {

    [HideInInspector]public AudioSource audiosource;
    [HideInInspector]public ESVehicleController vehiclecontroller;
    [HideInInspector]public ESGearShift gearshift;
    [Header("PitchSettings")]
    public float PitchModifier =0.5f;
    [Tooltip("To Get Best sound make value same as MaxEngineRpm")]
    public float PitchMultiplier =100f;
    [Header("VolumeSettings")]
    [Tooltip("To Get Best sound make value same as MaxEngineRpm")]
    public float VolumeMultiplier =100f;
    public float StartVolume = 0.6f;
    // Use this for initialization
    private void Start()
    {
        gearshift = GetComponent<ESGearShift>();
        vehiclecontroller = GetComponent<ESVehicleController>();
        audiosource = GetComponent<AudioSource>();
        audiosource.loop = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (vehiclecontroller.usefuel)
        {
            if (vehiclecontroller.fuelmanager.Empty && audiosource.isPlaying)
            {
                audiosource.Stop();
            }
            else if (!vehiclecontroller.fuelmanager.Empty && !audiosource.isPlaying)
            {
                audiosource.Play();
            }
        }
        audiosource.pitch = Mathf.Abs(vehiclecontroller.Rpm) > 0 && Mathf.Abs(gearshift.forwardSlip) > (gearshift.sliplimit +0.1f) && vehiclecontroller.CurrentSpeed < 0.3f && Mathf.Abs(Input.GetAxis("Vertical")) > 0?
            gearshift.forwardSlip  :(gearshift.EngineRpm / PitchMultiplier) + PitchModifier;
        audiosource.volume = (gearshift.EngineRpm / VolumeMultiplier) + StartVolume;
    }
}
