﻿using UnityEngine;
using UnityEngine.Networking;
using VRTK;

public class PlayerInput : MonoBehaviour
{
    private VRTK_ControllerEvents lHandEvents;
    private VRTK_ControllerEvents rHandEvents;
    private VRTK_ControllerReference lHandRef;
    private VRTK_ControllerReference rHandRef;
    private ServerSync ss;

    private Transform headset, lHandCont, rHandCont;
    private Vector3AndQuaternion head, lHand, rHand;

    [SerializeField] private bool serverCanPlay;
    [SerializeField] private float grabRadius;
    [SerializeField] private float holdTouchPadTimer;

    private float leftTimer;
    private float rightTimer;

    private void Start()
    {
        gameObject.name = "Player " + GetComponent<NetworkIdentity>().netId;

        ss = GetComponent<ServerSync>();

        if (!ss.isLocalPlayer || (!serverCanPlay && ss.isServer))
        {
            Destroy(this);
            return;
        }

        head = new Vector3AndQuaternion();
        lHand = new Vector3AndQuaternion();
        rHand = new Vector3AndQuaternion();
        headset = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.Headset);
        lHandCont = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController);
        rHandCont = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController);

        lHandEvents = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.LeftController).GetComponent<VRTK_ControllerEvents>();
        rHandEvents = VRTK_DeviceFinder.DeviceTransform(VRTK_DeviceFinder.Devices.RightController).GetComponent<VRTK_ControllerEvents>();

        lHandRef = VRTK_DeviceFinder.GetControllerReferenceLeftHand();
        rHandRef = VRTK_DeviceFinder.GetControllerReferenceRightHand();

        lHandEvents.GripClicked += LHandEvents_GripClicked;
        rHandEvents.GripClicked += RHandEvents_GripClicked;

        lHandEvents.GripUnclicked += LHandEvents_GripUnclicked;
        rHandEvents.GripUnclicked += RHandEvents_GripUnclicked;

        lHandEvents.TriggerClicked += LHandEvents_TriggerClicked;
        rHandEvents.TriggerClicked += RHandEvents_TriggerClicked;

        lHandEvents.TouchpadPressed += LHandEvents_TouchpadPressed;
        rHandEvents.TouchpadPressed += RHandEvents_TouchpadPressed;

        lHandEvents.TouchpadReleased += LHandEvents_TouchpadReleased;
        rHandEvents.TouchpadReleased += RHandEvents_TouchpadReleased;
    }

    private void Update()
    {
        head.SetPosAndRot(headset);
        lHand.SetPosAndRot(lHandCont);
        rHand.SetPosAndRot(rHandCont);
        ss.CmdSyncVRTransform(head, lHand, rHand);
    }

    private void RHandEvents_TriggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        ss.CmdTriggerClick(VRTK_DeviceFinder.Devices.RightController);
    }

    private void LHandEvents_TriggerClicked(object sender, ControllerInteractionEventArgs e)
    {
        ss.CmdTriggerClick(VRTK_DeviceFinder.Devices.LeftController);
    }

    private void RHandEvents_GripUnclicked(object sender, ControllerInteractionEventArgs e)
    {
        ss.CmdCallUngrab(VRTK_DeviceFinder.Devices.RightController, VRTK_DeviceFinder.GetControllerVelocity(rHandRef), VRTK_DeviceFinder.GetControllerAngularVelocity(rHandRef));
    }

    private void LHandEvents_GripUnclicked(object sender, ControllerInteractionEventArgs e)
    {
        ss.CmdCallUngrab(VRTK_DeviceFinder.Devices.LeftController, VRTK_DeviceFinder.GetControllerVelocity(lHandRef), VRTK_DeviceFinder.GetControllerAngularVelocity(lHandRef));
    }

    private void RHandEvents_GripClicked(object sender, ControllerInteractionEventArgs e)
    {
        ss.CmdCallGrab(VRTK_DeviceFinder.Devices.RightController, grabRadius);
    }

    private void LHandEvents_GripClicked(object sender, ControllerInteractionEventArgs e)
    {
        ss.CmdCallGrab(VRTK_DeviceFinder.Devices.LeftController, grabRadius);
    }

    private void RHandEvents_TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        ss.CmdCallTouchpadButton(VRTK_DeviceFinder.Devices.RightController, rHandEvents.GetTouchpadAxis());
        TouchPadDown(VRTK_DeviceFinder.Devices.RightController);
    }

    private void LHandEvents_TouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        ss.CmdCallTouchpadButton(VRTK_DeviceFinder.Devices.LeftController, lHandEvents.GetTouchpadAxis());
        TouchPadDown(VRTK_DeviceFinder.Devices.LeftController);
    }

    private void RHandEvents_TouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        TouchPadUp(VRTK_DeviceFinder.Devices.RightController);
    }

    private void LHandEvents_TouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        TouchPadUp(VRTK_DeviceFinder.Devices.LeftController);
    }

    private void TouchPadDown(VRTK_DeviceFinder.Devices control)
    {
        switch (control)
        {
            case VRTK_DeviceFinder.Devices.LeftController:
                leftTimer = Time.time + holdTouchPadTimer;
                break;
            case VRTK_DeviceFinder.Devices.RightController:
                rightTimer = Time.time + holdTouchPadTimer;
                break;
        }
    }

    private void TouchPadUp(VRTK_DeviceFinder.Devices control)
    {
        switch (control)
        {
            case VRTK_DeviceFinder.Devices.LeftController:
                // Checks if user held down button for long enough
                if (Time.time > leftTimer)
                {
                    Debug.Log("ZH DIED");
                }
                break;
            case VRTK_DeviceFinder.Devices.RightController:
                // Checks if user held down button for long enough
                if (Time.time > rightTimer)
                {
                    Debug.Log("ZH MUM DIED");
                }
                break;
        }
    }
}
