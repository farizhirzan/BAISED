using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using TMPro;

public class ImageToTextConverter : MonoBehaviour
{
    public RawImage imageDisplay;
    public string apiKey = "r8_0D3MzXy3oqHGCIYkO0VjdBCz8VFFPMN29wcjq";
    public TextMeshProUGUI generatedTextDisplay;

    [System.Serializable]
    public class ResponseData
    {
        public ChoiceData[] choices;
    }

    [System.Serializable]
    public class ChoiceData
    {
        public string text;
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(LoadImageAndGenerateText());
        }
    }

    public IEnumerator LoadImageAndGenerateText()
    {
        // Load the image file
        string imagePath = PlayerPrefs.GetString("LastImagePath");
        Texture2D texture = new Texture2D(2, 2);
        byte[] imageData = File.ReadAllBytes(imagePath);
        texture.LoadImage(imageData);

        // Display the loaded image
        imageDisplay.texture = texture;

        // Encode the image to base64
        string base64Image = System.Convert.ToBase64String(imageData);

        // Create the request data
        string requestData = "{\"prompt\": \"[IMAGE] " + base64Image + "\", \"max_tokens\": 100}";

        // Create the request
        UnityWebRequest request = UnityWebRequest.Post("https://api.replicate.com/v1/predictions", requestData);

        // Set the request headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        // Send the request
        yield return request.SendWebRequest();

        // Check for errors
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to send request: " + request.error);
            yield break;
        }

        // Get the response text
        string responseText = request.downloadHandler.text;

        // Parse the response to get the generated text
        ResponseData responseData = JsonUtility.FromJson<ResponseData>(responseText);
        string generatedText = responseData.choices[0].text;

        // Display the generated text
        generatedTextDisplay.text = generatedText;
    }
}
