using UnityEngine;
using System;
using System.Net.Http;
using System.Collections;
using UnityEngine.Networking;

public class GetData_S2 : MonoBehaviour
{
    // NOTE: this was hardcoded to a specific device's LAN IP ("http://192.168.85.111/s1") in the
    // source project. Changed to follow the same mDNS hostname convention as GetData_M/GetData_S1
    // (sonia_m.local / sonia_s1.local) - update this if the real right-hand sensor uses a different host.
    string url = "http://sonia_s2.local";

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
    private Vector3 initialAcceleration;


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
                if (newParcalar.Length >= 3)
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
        if (float.TryParse(parcalar[0].Trim(), out float PX))
        {
            float px = PX;
            AssignValue(px, "px");
        }
        if (float.TryParse(parcalar[1].Trim(), out float PY))
        {
            float py = PY;
            AssignValue(py, "py");
        }
        if (float.TryParse(parcalar[2].Trim(), out float PZ))
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
            // NOTE: ParseAndAssignData only ever calls AssignValue with "px"/"py"/"pz" here, so the
            // "rx"/"ry"/"rz"/"g" cases above (and therefore Masax/Masay/Masaz/Grip) are never actually
            // set from live sensor data in this class - unlike GetData_S1, where the equivalent
            // rotation/grip parsing is live. These fields are always their default (0 / false), so the
            // values written below are placeholders, not tracked right-hand orientation, until S2's
            // parsing is restored to match S1's. What this fix does change: it previously wrote those
            // placeholders into vrInputSender.HS_rotation* (the headset fields GetData_M actively
            // drives), which is still wrong regardless - GetData_S2 is the right-hand sensor (mirrors
            // GetData_S1's LH_* wiring), so this now targets RH_rotation* instead.
            vrInputSender.RH_rotationx = Masax;
            vrInputSender.RH_rotationy = Masay;
            vrInputSender.RH_rotationz = Masaz;
            vrInputSender.RightGrip = Grip;

            //// Güncel ivme değerini hesapla
            //Vector3 currentAcceleration = new Vector3(Elx, Ely, Elz) - initialAcceleration;

            //// Yeni hızı hesapla
            //velocity += currentAcceleration * Time.deltaTime;

            //// Hızı belirli bir aralıkta tut
            //velocity.x = Mathf.Clamp(velocity.x, -1f, 1f); // Örnek olarak -1f ve 1f arasında sınırlama yapıldı
            //velocity.y = Mathf.Clamp(velocity.y, -1f, 1f);
            //velocity.z = Mathf.Clamp(velocity.z, -1f, 1f);

            //// Pozisyon değişikliğini hızla çarparak güncelle
            //transform.position += velocity * Time.deltaTime;

            //// Başlangıç ivmesini güncelle
            //initialAcceleration = new Vector3(Elx, Ely, Elz);
        }
        else
        {
            Hs.SetActive(false);
        }
    }


}
