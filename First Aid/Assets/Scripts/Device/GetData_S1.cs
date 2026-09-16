using UnityEngine;
using System;
using System.Net.Http;
using System.Collections;
using UnityEngine.Networking;

public class GetData_S1 : MonoBehaviour
{
    string url = "http://sonia_s1.local";

    public float interval = 0;
    public float timeoutDuration = 5f;
    private float lastDataReceivedTime = 0;

    public bool HS_Gorunsun = true;

    [SerializeField] GameObject Hs;

    public VRInputSender vrInputSender;

    bool startagain = false;

    private float initialMasax = 0;
    private float initialMasay = 0;
    private float initialMasaz = 0;
    private float initialElx = 0;
    private float initialEly = 0;
    private float initialElz = 0;
    private Vector3 velocity;


    public float Masax;
    public float Masay;
    public float Masaz;
    public bool Grip;
    public float Elx;
    public float Ely;
    public float Elz;

    void Start()
    {
        velocity = Vector3.zero;
        StartCoroutine(GetDataRepeatedly());
    }

    IEnumerator GetDataRepeatedly()
    {
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                startagain = true;
            }
            else
            {
                string responseData = www.downloadHandler.text;
                string[] newParcalar = responseData.Split("/");
                if (newParcalar.Length >= 7)
                {
                    ParseAndAssignData(newParcalar);
                    lastDataReceivedTime = Time.time;
                }
                else
                {
                    Debug.Log("Veriler eksik, yeniden denenecek.");
                    startagain = true;
                }
            }

            if (Time.time - lastDataReceivedTime > timeoutDuration)
            {
                Debug.Log("Bağlantı koptu");
                initialMasax = Masax;
                initialMasay = Masay;
                initialMasaz = Masaz;
                initialElx = Elx;
                initialEly = Ely;
                initialElz = Elz;
                startagain = true;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    void ParseAndAssignData(string[] parcalar)
    {
        if (float.TryParse(parcalar[0].Trim(), out float xDegeri))
        {
            float x = xDegeri + initialMasax;
            x /= 100;
            AssignValue(x, "rx");
        }

        if (float.TryParse(parcalar[1].Trim(), out float yDegeri))
        {
            float y = yDegeri + initialMasay;
            y /= 100;
            AssignValue(y, "ry");
        }

        if (float.TryParse(parcalar[2].Trim(), out float zDegeri))
        {
            float z = zDegeri + initialMasaz;
            z /= 100;
            AssignValue(z, "rz");
        }
        if (float.TryParse(parcalar[3].Trim(), out float grip))
        {
            float g = grip;
            AssignValue(g, "g");
        }
        if (float.TryParse(parcalar[4].Trim(), out float PX))
        {
            float px = PX;
            AssignValue(px, "px");
        }
        if (float.TryParse(parcalar[6].Trim(), out float PY))
        {
            float py = PY;
            AssignValue(py, "py");
        }
        if (float.TryParse(parcalar[5].Trim(), out float PZ))
        {
            float pz = PZ;
            AssignValue(pz, "pz");
        }
    }

    void AssignValue(float value, string axis)
    {
        if (axis == "rx") Masax = value;
        else if (axis == "ry") Masay = value;
        else if (axis == "rz") Masaz = value;
        else if (axis == "g")
        {
            if (value == 1) Grip = true; else Grip = false;
        }
        else if (axis == "px") Elx = value;
        else if (axis == "py") Ely = value;
        else if (axis == "pz") Elz = value;
        if (HS_Gorunsun)
        {
            Hs.SetActive(true);
            vrInputSender.LH_rotationx = Masax;
            vrInputSender.LH_rotationy = Masay;
            vrInputSender.LH_rotationz = Masaz;
            vrInputSender.LeftGrip = Grip;

            //Vector3 acceleration = new Vector3(Elx, Ely, Elz);
            //velocity += acceleration * Time.deltaTime;
            //// Yeni hızı kullanarak pozisyonu güncelle

            //vrInputSender.LH_positionx = velocity.x * Time.deltaTime;
            //vrInputSender.LH_positiony = velocity.y * Time.deltaTime;
            //vrInputSender.LH_positionz = velocity.z * Time.deltaTime;

        }
        else
        {
            Hs.SetActive(false);
        }
    }
}
