using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GetData_M : MonoBehaviour
{
    string url1 = "http://sonia_m.local";
    string url2 = "http://sonia_m.local";
    public string currentUrl;

    public float interval = 0.1f; // Örnek bir interval süresi
    public float timeoutDuration = 5f;
    private float lastDataReceivedTime = 0;

    public bool HS_Gorunsun = true;

    [SerializeField] GameObject Hs;
    public VRInputSender vrInputSender;

    bool startagain = false;

    private float initialMasax = 0;
    private float initialMasay = 0;
    private float initialMasaz = 0;

    public float Masax;
    public float Masay;
    public float Masaz;

    void Start()
    {
        currentUrl = url1; // Başlangıçta url1 kullanılıyor
        StartCoroutine(ChangeUrlAfterDelay());
        StartCoroutine(GetDataRepeatedly());
    }

    IEnumerator ChangeUrlAfterDelay()
    {
        yield return new WaitForSeconds(30f); // 30 saniye bekleyin

        if (currentUrl == url1)
        {
            currentUrl = url2;
        }
        else
        {
            currentUrl = url1;
        }

        Debug.Log("URL değiştirildi: " + currentUrl);
    }

    IEnumerator GetDataRepeatedly()
    {
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(currentUrl);
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
                startagain = true;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    void ParseAndAssignData(string[] parcalar)
    {
        if (float.TryParse(parcalar[1].Trim(), out float xDegeri))
        {
            float x = xDegeri + initialMasax;
            x /= 100;
            AssignValue(x, 'x');
        }

        if (float.TryParse(parcalar[0].Trim(), out float yDegeri))
        {
            float y = yDegeri + initialMasay;
            y /= 100;
            AssignValue(y, 'y');
        }

        if (float.TryParse(parcalar[2].Trim(), out float zDegeri))
        {
            float z = zDegeri + initialMasaz;
            z /= 100;
            AssignValue(z, 'z');
        }
    }

    void AssignValue(float value, char axis)
    {
        if (axis == 'x') Masax = value;
        else if (axis == 'y') Masay = value;
        else if (axis == 'z') Masaz = value;

        if (HS_Gorunsun)
        {
            Hs.SetActive(true);
            vrInputSender.HS_rotationx = Masax;
            vrInputSender.HS_rotationy = Masay;
            vrInputSender.HS_rotationz = Masaz;
        }
        else
        {
            Hs.SetActive(false);
        }
    }
}
