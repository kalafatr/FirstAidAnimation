using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit.Inputs;

public class VRInputSender : MonoBehaviour
{
    
    public bool HareketWithKeyboard;
    private bool isWheelButtonDown = false;

    //HeadSet
    public float HS_positionx = 0;
    public float HS_positiony = 0;
    public float HS_positionz = 0;
    public float HS_rotationx = 0;
    public float HS_rotationy = 0;
    public float HS_rotationz = 0;
    public float HS_rotationw = 1;

    //LeftHand
    public float LH_positionx = 0;
    public float LH_positiony = 0;
    public float LH_positionz = 0;

    public float LH_rotationx = 0;
    public float LH_rotationy = 0;
    public float LH_rotationz = 0;
    public float LH_rotationw = 1;

    public float LH_joystcikx = 0;
    public float LH_joysticky = 0;

    //RightHand
    public float RH_positionx = 0;
    public float RH_positiony = 0;
    public float RH_positionz = 0;

    public float RH_rotationx = 0;
    public float RH_rotationy = 0;
    public float RH_rotationz = 0;
    public float RH_rotationw = 1;

    public float RH_joystickx = 0;
    public float RH_joysticky = 0;

    //Buttons
    public bool LeftGrip;
    public bool XButton;
    public bool YButton;
    public bool RightGrip;
    public bool AButton;
    public bool BButton;

    // Keyboarddan Y�netmek ��in
    Quaternion headRotation = Quaternion.identity;
    private void Start()
    {
            LH_positionx = -.4f;
            LH_positiony = -.2f;
            LH_positionz = .6f;

            RH_positionx = .4f;
            RH_positiony = -.2f;
            RH_positionz = .6f;
            
        SoniaDevice.SetVRInputSender(this);
    }

    void Update()
    {
          
        if (!HareketWithKeyboard) return; 
        // LH_positionx = -.4f;
        // LH_positiony = -.2f;
        // LH_positionz = .6f;
        //
        // RH_positionx = .4f;
        // RH_positiony = -.2f;
        // RH_positionz = .6f;

        RotateHead();

        ChangeValue_RH_Joystick();

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.E))
                return;
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            bool isLeftHand;
            bool isRightHand;
            if (Input.GetKey(KeyCode.Q))
            {
                isLeftHand = true;
            }
            else
            {
                isLeftHand = false;
            }
            if (Input.GetKey(KeyCode.E))
            {
                isRightHand= true;
            }
            else
            {
                isRightHand = false;
            }
            RotateHand(mouseX, mouseY, isLeftHand, isRightHand);
            if (Input.GetKey(KeyCode.G))
            {
                if (isLeftHand)
                {
                    LeftGrip = true;
                }
                if (isRightHand)
                {
                    RightGrip = true;
                }
            }
            else
            {
                LeftGrip = false;
                RightGrip = false;
            }
            HandMoveByMouse(isRightHand,isLeftHand);
        }

        if (Input.GetKeyUp(KeyCode.G))
        {
            LeftGrip = false;
            RightGrip = false;
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            RH_rotationx = 0;
            RH_rotationy = 0;
            RH_rotationz = 0;
            LH_rotationx = 0;
            LH_rotationy = 0;
            LH_rotationz = 0;

            LH_positionx = -.4f;
            LH_positiony = -.2f;
            LH_positionz = .6f;
            RH_positionx = .4f;
            RH_positiony = -.2f;
            RH_positionz = .6f;
        }
    }

    private void HandMoveByMouse(bool isRight,bool isLeft)
    {
        if (Input.GetMouseButtonDown(2))
        {
            isWheelButtonDown = true;
        }
        if (Input.GetMouseButtonUp(2))
        {
            isWheelButtonDown = false;
        }
        if (isWheelButtonDown)
        {
            float wheelInput = Input.GetAxis("Mouse ScrollWheel");
            float mouseX = Input.GetAxis("Mouse X")*0.05f;
            float mouseY = Input.GetAxis("Mouse Y")*0.05f;
            if (isRight)
            {
                RH_positionx += mouseX;
                RH_positiony += mouseY;
                RH_positionz += wheelInput;
            }if (isLeft)
            {
                LH_positionx += mouseX;
                LH_positiony += mouseY;
                LH_positionz += wheelInput;
            }
        }
    }

    private void ChangeValue_RH_Joystick()
    {
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D)) { }
        else
        {
            if (Input.GetKey(KeyCode.A))
            {
                RH_joystickx = -0.5f;
            }
            else if(Input.GetKeyUp(KeyCode.A))
            {
                RH_joystickx = 0;
            }

            if (Input.GetKey(KeyCode.D))
            {
                RH_joystickx = 0.5f;
            }
            else if(Input.GetKeyUp(KeyCode.D))
            {
                RH_joystickx = 0;
            }
        }


        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S)) { }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                RH_joysticky = 0.5f;
            }
            else if(Input.GetKeyUp(KeyCode.W))
            {
                RH_joysticky = 0;
            }

            if (Input.GetKey(KeyCode.S))
            {
                RH_joysticky = -0.5f;
            }
            else if(Input.GetKeyUp(KeyCode.S))
            {
                RH_joysticky = 0;
            }
        }
    }

    private void RotateHand(float mouseX, float mouseY, bool isLeftHand, bool isRightHand)
    {
        if (isWheelButtonDown) return;
        
        float sensitivity = 20; // D�n�� hassasiyeti
        float rotationSpeed = 6f; // D�n�� h�z�

        // D�nd�rme miktar�n� hesapla
        float rotationAmountX = -mouseY * sensitivity * rotationSpeed * Time.deltaTime;
        float rotationAmountY = mouseX * sensitivity * rotationSpeed * Time.deltaTime;

        // Sol el i�in d�nd�rme
        if (isLeftHand)
        {
            LH_rotationx += rotationAmountX;
            LH_rotationy += rotationAmountY;
        }
        // Sa� el i�in d�nd�rme
        else if (isRightHand)
        {
            RH_rotationx += rotationAmountX;
            RH_rotationy += rotationAmountY;
        }
    }

    private void RotateHead()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            float sensitivity = 1f; // Duyarl�l�k fakt�r�

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            float mouseScroll = Input.GetAxis("Mouse ScrollWheel");

            Quaternion rotationDelta =
                Quaternion.Euler(-mouseY * sensitivity, mouseX * sensitivity, -mouseScroll * sensitivity);
            headRotation *= rotationDelta;

            // G�ncellenmi� rotasyonu HS_rotation'e atama
            HS_rotationx = Mathf.Clamp(HS_rotationx + (-mouseY * sensitivity), -360, 360);
            HS_rotationy = Mathf.Clamp(HS_rotationy + (mouseX * sensitivity), -360, 360);
            HS_rotationz = Mathf.Clamp(HS_rotationz + (-mouseScroll * sensitivity), -360, 360);
        }
    }

}