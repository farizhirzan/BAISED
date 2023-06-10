using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SendToGoogleForm : MonoBehaviour
{
    public TMPro.TMP_Text generatedprompt;

    [SerializeField]
    private string BASE_URL = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSd_CAkDeHcAeIM5imCr9cTssodBURGbZ1_Kh0g4WkeoGPShkw/formResponse";

    IEnumerator Post(string prompt)
    {
        WWWForm form = new WWWForm();
        form.AddField("entry.1763471470", prompt);
        byte[] rawData = form.data;

        UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            Debug.Log("Form submission successful!");
        }
    }

    public void Send()
    {
        StartCoroutine(Post(generatedprompt.text));
    }
}

