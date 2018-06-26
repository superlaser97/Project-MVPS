﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SealTeam4;

public class Explosive : MonoBehaviour, IActions
{
    List<string> action = new List<string>();
    [SerializeField] ParticleSystem explosionParticles;

    void Start()
    {
        action.Add("Explode");
    }
    public List<string> GetActions()
    {
        foreach (string a in action)
        {
            Debug.Log(a);
        }
        return action;
    }

    public void SetAction(string action)
    {
        Debug.Log("SetAction");
        Explode();
    }

    private void Explode()
    {
        ParticleSystem explosion;
        explosion = Instantiate(explosionParticles, gameObject.transform.position, Quaternion.identity) as ParticleSystem;
        explosion.Play();
        //GameManagerAssistant.instance.CmdSyncHaps(, ControllerHapticsManager.HapticType.GUNFIRE, VRTK.VRTK_DeviceFinder.Devices.LeftController);
        //GameManagerAssistant.instance.CmdSyncHaps(, ControllerHapticsManager.HapticType.GUNFIRE, VRTK.VRTK_DeviceFinder.Devices.RightController);
        Destroy(gameObject);
        
    }

    public string GetName()
    {
        return gameObject.name;
    }
}