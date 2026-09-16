using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.OpenXR.Input;
using Vector2Control = UnityEngine.InputSystem.Controls.Vector2Control;
using Vector3Control = UnityEngine.InputSystem.Controls.Vector3Control;


[InputControlLayout(stateType = typeof(SoniaDeviceState))]
public class SoniaDevice : InputDevice, IInputUpdateCallbackReceiver
{
    public Vector3Control HeadsetPosition { get; private set; }
    public QuaternionControl HeadsetRotation { get; private set; }
    public Vector3Control LeftHandPosition { get; private set; }

    public QuaternionControl LeftHandRotation { get; private set; }

    public ButtonControl LeftGrip { get; private set; }
    public ButtonControl RightGrip { get; private set; }

    public Vector3Control RightHandPosition { get; private set; }

    public QuaternionControl RightHandRotation { get; private set; }
    public Vector2Control RightHandJoystickControl { get; private set; } // Joystick kontrol�n� ekliyoruz.

    private static VRInputSender vrInputSender;

    public void OnUpdate()
    {
        var newState = new SoniaDeviceState();

        if (vrInputSender != null)
        {
            newState.hs_positionx = vrInputSender.HS_positionx;
            newState.hs_positiony = vrInputSender.HS_positiony;
            newState.hs_positionz = vrInputSender.HS_positionz;
            newState.hs_rotation = UnityEngine.Quaternion.Euler(vrInputSender.HS_rotationx, vrInputSender.HS_rotationy, vrInputSender.HS_rotationz);
            newState.lh_positionx = vrInputSender.LH_positionx;
            newState.lh_positiony = vrInputSender.LH_positiony;
            newState.lh_positionz = vrInputSender.LH_positionz;
            newState.lh_rotation = UnityEngine.Quaternion.Euler(vrInputSender.LH_rotationx, vrInputSender.LH_rotationy, vrInputSender.LH_rotationz);

            newState.rh_positionx = vrInputSender.RH_positionx;
            newState.rh_positiony = vrInputSender.RH_positiony;
            newState.rh_positionz = vrInputSender.RH_positionz;
            newState.rh_rotation = UnityEngine.Quaternion.Euler(vrInputSender.RH_rotationx, vrInputSender.RH_rotationy, vrInputSender.RH_rotationz);
            newState.rh_joystickx = vrInputSender.RH_joystickx;
            newState.rh_joysticky = vrInputSender.RH_joysticky;
            newState.leftGrip = vrInputSender.LeftGrip;
            newState.rightGrip = vrInputSender.RightGrip;
        }

        InputSystem.QueueStateEvent(this, newState);
    }

    public static void SetVRInputSender(VRInputSender vrSender)
    {
        vrInputSender = vrSender;
    }

    protected override void FinishSetup()
    {
        base.FinishSetup();


        HeadsetPosition = GetChildControl<Vector3Control>("Headset Position");
        HeadsetRotation = GetChildControl<QuaternionControl>("Headset Rotation");

        LeftHandPosition = GetChildControl<Vector3Control>("LeftHand Position");
        LeftHandRotation = GetChildControl<QuaternionControl>("LeftHand Rotation");

        LeftGrip = GetChildControl<ButtonControl>("LeftGrip");
        RightGrip = GetChildControl<ButtonControl>("RightGrip");

        RightHandPosition = GetChildControl<Vector3Control>("RightHand Position");
        RightHandRotation = GetChildControl<QuaternionControl>("RightHand Rotation");
        RightHandJoystickControl = GetChildControl<Vector2Control>("RightHand Joystick");

    }

    static SoniaDevice()
    {
        InputSystem.RegisterLayout<SoniaDevice>(
            matches: new InputDeviceMatcher().WithInterface("SoniaDevice"));

        if (!InputSystem.devices.Any(x => x is SoniaDevice))
            InputSystem.AddDevice(new InputDeviceDescription { interfaceName = "SoniaDevice", product = "VRDevice" });
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Custom Device Sample/Create Sonia Device")]
    private static void CreateDevice()
    {
        InputSystem.AddDevice(new InputDeviceDescription
        {
            interfaceName = "SoniaDevice",
            product = "VRDevice"
        });
    }

    [MenuItem("Tools/Custom Device Sample/Remove Sonia Device")]
    private static void RemoveDevice()
    {
        var customDevice = InputSystem.devices.FirstOrDefault(x => x is SoniaDevice);
        if (customDevice != null)
            InputSystem.RemoveDevice(customDevice);
    }
#endif
}

public struct SoniaDeviceState : IInputStateTypeInfo
{
    public FourCC format => new FourCC('M', 'B', 'A', 'V');

    [InputControl(name = "Headset Position", layout = "Vector3", format = "VC3S", sizeInBits = 96)]
    [InputControl(name = "Headset Position/x", format = "FLT")]
    public float hs_positionx;
    [InputControl(name = "Headset Position/y", format = "FLT", offset = 4)]
    public float hs_positiony;
    [InputControl(name = "Headset Position/z", format = "FLT", offset = 8)]
    public float hs_positionz;
    [InputControl(name = "Headset Rotation", layout = "Quaternion", format = "QUAT", sizeInBits = 128)]
    public UnityEngine.Quaternion hs_rotation;

    [InputControl(name = "LeftHand Position", layout = "Vector3", format = "VC3S", sizeInBits = 96)]
    [InputControl(name = "LeftHand Position/x", format = "FLT")]
    public float lh_positionx;
    [InputControl(name = "LeftHand Position/y", format = "FLT", offset = 4)]
    public float lh_positiony;
    [InputControl(name = "LeftHand Position/z", format = "FLT", offset = 8)]
    public float lh_positionz;

    [InputControl(name = "LeftHand Rotation", layout = "Quaternion", format = "QUAT", sizeInBits = 128)]
    public UnityEngine.Quaternion lh_rotation;

    [InputControl(name = "RightGrip", layout = "Button", format = "BIT")]
    public bool rightGrip;

    [InputControl(name = "RightHand Position", layout = "Vector3", format = "VC3S", sizeInBits = 96)]
    [InputControl(name = "RightHand Position/x", format = "FLT")]
    public float rh_positionx;
    [InputControl(name = "RightHand Position/y", format = "FLT", offset = 4)]
    public float rh_positiony;
    [InputControl(name = "RightHand Position/z", format = "FLT", offset = 8)]
    public float rh_positionz;

    [InputControl(name = "RightHand Rotation", layout = "Quaternion", format = "QUAT", sizeInBits = 128)]
    public UnityEngine.Quaternion rh_rotation;

    [InputControl(name = "RightHand Joystick", layout = "Vector2", format = "VC2S", sizeInBits = 96)]
    [InputControl(name = "RightHand Joystick/x", format = "FLT")]
    public float rh_joystickx;
    [InputControl(name = "RightHand Joystick/y", format = "FLT")]
    public float rh_joysticky;


    [InputControl(name = "LeftGrip", layout = "Button", format = "BIT")]
    public bool leftGrip;
}


