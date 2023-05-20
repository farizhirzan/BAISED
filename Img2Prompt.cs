using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class Img2Prompt : MonoBehaviour
{
    public RawImage displayImage;
    public TMP_Text promptText;

    private string lastImagePath;
    private const string REPLICATE_API_TOKEN = "r8_0D3MzXy3oqHGCIYkO0VjdBCz8VFFPMN29wcjq";
    private static readonly HttpClient client = new HttpClient();

    public async void Start()
    {
        // Set the last captured image path
        lastImagePath = "/storage/emulated/0/DCIM/GalleryTest/Image (8).png";

        // Load the image from file path
        Texture2D loadedImage = LoadImageFromFile(lastImagePath);

        // Display the loaded image
        displayImage.texture = loadedImage;

        // Convert the image to base64
        string imageBase64 = ConvertImageToBase64(loadedImage);

        // Send the image to the Img2Prompt API
        await SendImageToImg2Prompt(imageBase64);
    }

    private async Task GetLastImageFromGallery()
    {
        // Access the device's gallery
        AndroidJavaClass environment = new AndroidJavaClass("android.os.Environment");
        AndroidJavaObject internalStorageDirectory = environment.CallStatic<AndroidJavaObject>("getInternalStorageDirectory");
        string galleryPath = internalStorageDirectory.Call<string>("getAbsolutePath") + "/DCIM/GalleryTest";

        // Get the paths of all images in the gallery
        string[] imagePaths = Directory.GetFiles(galleryPath, "*.jpg");

        if (imagePaths.Length > 0)
        {
            // Get the path of the last image in the gallery
            lastImagePath = imagePaths[imagePaths.Length - 1];
        }
        else
        {
            Debug.Log("No images found in the gallery.");
        }
    }


    public Texture2D LoadImageFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private string ConvertImageToBase64(Texture2D image)
    {
        byte[] imageBytes = image.EncodeToPNG();
        return Convert.ToBase64String(imageBytes);
    }

    private async Task SendImageToImg2Prompt(string imageBase64)
    {
        var url = "https://api.replicate.com/v1/predictions";

        var data = new
        {
            version = "50adaf2d3ad20a6f911a8a9e3ccf777b263b8596fbd2c8fc26e8888f8a0edbb5",
            input = new { image = imageBase64 }
        };

        var json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Token {REPLICATE_API_TOKEN}");

        await PostRequest(url, content);
    }

    private async Task PostRequest(string url, StringContent content)
    {
        using (var request = new HttpRequestMessage(HttpMethod.Post, url))
        {
            request.Content = content;

            using (var response = await client.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    Debug.Log(responseString);

                    var parsedJson = JsonConvert.DeserializeObject<Img2PromptResponse>(responseString);

                    StartCoroutine(PollPrediction(parsedJson.id));
                }
                else
                {
                    Debug.Log($"Error: {response.StatusCode}");
                }
            }
        }
    }

    private IEnumerator PollPrediction(string predictionId)
    {
        var url = $"https://api.replicate.com/v1/predictions/{predictionId}";

        while (true)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    string responseString = www.downloadHandler.text;
                    Debug.Log(responseString);

                    var parsedJson = JsonConvert.DeserializeObject<Img2PromptResponse>(responseString);

                    if (parsedJson.status == "completed" && parsedJson.output != null)
                    {
                        promptText.text = parsedJson.output[0].prompt;
                        yield break;
                    }
                    else if (parsedJson.status == "failed")
                    {
                        Debug.Log($"Prediction failed: {parsedJson.error}");
                        yield break;
                    }
                }
                else
                {
                    Debug.Log($"Error: {www.error}");
                    yield break;
                }
            }

            yield return new WaitForSeconds(1f); // Poll every 1 second
        }
    }

    [Serializable]
    public class Img2PromptResponse
    {
        public string id;
        public string version;
        public Input input;
        public Output[] output;
        public string status;
        public string error;
    }

    [Serializable]
    public class Input
    {
        public string image;
    }

    [Serializable]
    public class Output
    {
        public string prompt;
    }
}