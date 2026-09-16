// Bu kod, Unity'de kemik pozisyonlarýný yönetmek ve benzerlikleri kontrol etmek için kullanýlýr.
// Bir objenin kemiklerinin rotasyonunu kaydeder, bu rotasyonlarý diðer objelerle karþýlaþtýrarak benzerlikleri tespit eder.

#if UNITY_EDITOR
// Unity Editor özelliðini kullanmak için UnityEditor kütüphanesini kullanýr.
using UnityEditor;
#endif

// UnityEngine kütüphanesini kullanýr.
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using TMPro;

// Serializable sýnýflarý, Unity'nin JsonUtility sýnýfýyla dönüþtürülebilir hale getirir.
[Serializable]
public class SaveDataSerialization
{
    // Her bir kemik için rotasyon ve isim bilgisini tutan liste.
    public List<BoneDataSerialization> boneData = new List<BoneDataSerialization>();

    // Kaydedilen dosyanýn numarasýný tutan deðiþken.
    public int fileNumber = 1; // Dosya numarasýný tutacak deðiþken

    // SaveDataSerialization sýnýfýnýn kurucu methodu. Listeleri sýfýrlar ve dosya numarasýný 1 olarak ayarlar.
    public SaveDataSerialization()
    {
        boneData = new List<BoneDataSerialization>();
        fileNumber = 1;
    }
}

// Her bir kemik için rotasyon ve isim bilgisini tutan sýnýf.
[Serializable]
public class BoneDataSerialization
{
    // Quaternion türünde rotasyon bilgisini tutan deðiþken.
    public Quaternion rotation;
    // Kemik ismini tutan deðiþken.
    public string name;

    // BoneDataSerialization sýnýfýnýn kurucu methodu. Rotasyon ve isim bilgisini alarak atama yapar.
    public BoneDataSerialization(Quaternion rotation, string name)
    {
        this.rotation = rotation;
        this.name = name;
    }
}

// Kemik pozisyonlarýný yöneten sýnýf.
public class BonePositionManager : MonoBehaviour
{
    // Tüm kemiklerin rotasyon ve isim bilgilerini tutan liste.
    public List<BoneDataSerialization> boneDataList = new List<BoneDataSerialization>();
    // Yüklenen kayýtlarýn listesini tutan liste.
    public List<SaveDataSerialization> loadedDataList = new List<SaveDataSerialization>();
    // Kayýt iþlemlerini yöneten sýnýf.
    public SaveDataSerialization saveData = new SaveDataSerialization();
    // Yüklenen kayýtlarý yöneten sýnýf.
    public LoadBoneData loadBoneData = new LoadBoneData();
    // Kemikler arasýndaki benzerlik eþiði.
    public int threshold = 10;
    // Seçilen yüklenen kaydýn index numarasý.
    public int index = 0;

    // Tüm kemikleri listeleyen ve listeyi dolduran method.
    void PopulateBoneDataList()
    {
        boneDataList.Clear();
        Transform[] bones = GetComponentsInChildren<Transform>();
        foreach (Transform bone in bones)
        {
            if (bone.CompareTag("Bone"))
            {
                // Her bir kemik için rotasyon ve isim bilgilerini listeye ekler.
                boneDataList.Add(new BoneDataSerialization(
                    bone.localRotation, // Kemik rotasyonu
                    bone.name // Kemik ismi
                ));
            }
        }
    }

    // Oyun baþladýðýnda yüklenen kayýtlarý yükleyen method.
    private void Start()
    {
        loadBoneData.LoadData();
        loadedDataList = loadBoneData.loadedDataList;
    }

    // Her güncelleme döngüsünde kemik pozisyonlarýný güncelleyen ve benzerlik kontrolü yapan method.
    private void Update()
    {
        PopulateBoneDataList();
        CheckForSimilarMoves();
    }

    // Yüklenen kayýtlarý kontrol ederek benzerlikleri tespit eden method.
    private void CheckForSimilarMoves()
    {
        if (loadedDataList.Count > 0 && index >= 0 && index < loadedDataList.Count)
        {
            var loadedData = loadedDataList[index];
            bool isSimilar = true;

            foreach (var boneData in loadedData.boneData)
            {
                bool found = false;

                foreach (var newData in boneDataList)
                {
                    if (boneData.name == newData.name)
                    {
                        // Kemikler arasýndaki farkýn açýsýný hesaplar.
                        float angle = Quaternion.Angle(boneData.rotation, newData.rotation);
                        // Eðer fark belirli bir eþik deðerinin altýndaysa, benzer kabul edilir.
                        if (angle < threshold)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    isSimilar = false;
                    break;
                }
            }

            // Eðer tüm kemikler benzerse, bu durumu loglar.
            if (isSimilar)
            {
                Debug.Log($"Loaded data {index} is similar to newly populated data.");
            }
        }
    }

#if UNITY_EDITOR
    // Kaydedilecek dosyanýn yolunu döndüren method.
    public static string GetSaveName()
    {
        return SceneManager.GetActiveScene().name;
    }

    public static string GetSaveFolderPath()
    {
        return Application.streamingAssetsPath + "/Saves/";
    }

    // Unity Editor üzerinde "Tools" menüsü altýnda "SaveBoneData" seçeneði ekleyen method.
    [MenuItem("Tools/SaveBoneData")]
    public static void SaveBoneData()
    {
        BonePositionManager editor = FindObjectOfType<BonePositionManager>();
        if (editor != null)
        {
            SaveDataSerialization saveData = new SaveDataSerialization();

            foreach (var boneData in editor.boneDataList)
            {
                saveData.boneData.Add(boneData);
            }
            string json = JsonUtility.ToJson(saveData);
            // Dosyaya yazma iþlemini gerçekleþtirir.
            if (WriteToFile(GetSaveName() + "_" + editor.saveData.fileNumber, json))
            {
                Debug.Log("Successfully saved data");
                editor.saveData.fileNumber++; // Dosya numarasýný artýr
            }
        }
    }

    // Dosyaya yazma iþlemini gerçekleþtiren method.
    private static bool WriteToFile(string name, string content)
    {
        var fullPath = Path.Combine(GetSaveFolderPath(), name + ".json");

        try
        {
            File.WriteAllText(fullPath, content);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving to a file " + e.Message);
        }

        return false;
    }
#endif
}

// Kayýtlý verileri yükleyen sýnýf.
public class LoadBoneData
{
    // Yüklenen kayýtlarýn listesini tutan liste.
    public List<SaveDataSerialization> loadedDataList = new List<SaveDataSerialization>();

    public static string GetSaveFolderPath()
    {
        return Application.streamingAssetsPath + "/Saves/";
    }

    // Kayýtlý verileri yükleyen method.
    public void LoadData()
    {
        string[] fileNames = Directory.GetFiles(GetSaveFolderPath(), "*.json");

        foreach (string fileName in fileNames)
        {
            string sceneName = Path.GetFileNameWithoutExtension(fileName).Split('_')[0];
            int fileNumber = int.Parse(Path.GetFileNameWithoutExtension(fileName).Split('_')[1]);

            if (sceneName == SceneManager.GetActiveScene().name)
            {
                string json = File.ReadAllText(fileName);
                SaveDataSerialization saveData = JsonUtility.FromJson<SaveDataSerialization>(json);
                saveData.fileNumber = fileNumber;
                loadedDataList.Add(saveData);
            }
        }
        Debug.Log("Loaded " + loadedDataList.Count + " files.");
    }
}
