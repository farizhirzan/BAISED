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

public class PredictionResponse
{
    public string id { get; set; }
    public string version { get; set; }
    public InputData input { get; set; }
    public string output { get; set; }
    public string logs { get; set; }
    public string error { get; set; }
    public string status { get; set; }
    public DateTime created_at { get; set; }
    public Urls urls { get; set; }
}

public class InputData
{
    public string image { get; set; }
}

public class Urls
{
    public string cancel { get; set; }
    public string get { get; set; }
}

public class Generate : MonoBehaviour
{
    string idrequest = "";
    string base64Image = "";
    public TMP_Text generatedprompt;

    // Send the image to replicate
    public async void Start()
    {
        string imagePath = PlayerPrefs.GetString("LastImagePath");
        Debug.Log("Send Image: " + imagePath);
        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            base64Image = Convert.ToBase64String(imageBytes);
            Debug.Log("Base64 Image: " + base64Image);
        }
        else
        {
            Debug.LogError("Image file not found.");
            return;
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "your token here");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.replicate.com/v1/predictions");
        var content = new StringContent(JsonConvert.SerializeObject(new
        {
            version = "50adaf2d3ad20a6f911a8a9e3ccf777b263b8596fbd2c8fc26e8888f8a0edbb5",
            input = new { image = "data:image/jpeg;base64," + base64Image }
        }), Encoding.UTF8, "application/json");

        request.Content = content;

        try
        {
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.Log("Response Status Code: " + response.StatusCode);
            Debug.Log("Response Content: " + responseContent);

            PredictionResponse obj;
            try
            {
                obj = JsonConvert.DeserializeObject<PredictionResponse>(responseContent);
                idrequest = obj.id;
                Debug.Log("Prediction Response: " + obj.id);
            }
            catch (JsonException ex)
            {
                Debug.LogError("JSON Deserialization Exception: " + ex.Message);
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError("HTTP Request Exception: " + ex.Message);
        }

        await PollPrediction();
    }

    // Poll the prediction status
    public async Task PollPrediction()
    {
        while (true)
        {
            await Task.Delay(1000); // Poll every 1 second

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", "");

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.replicate.com/v1/predictions/" + idrequest);

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();

                PredictionResponse promptResult;
                try
                {
                    promptResult = JsonConvert.DeserializeObject<PredictionResponse>(responseContent);
                    Debug.Log("Prediction Response: " + promptResult.output);
                    if (promptResult.status == "succeeded" && !string.IsNullOrEmpty(promptResult.output))
                        {
                            generatedprompt.text = promptResult.output;
                            break;
                        }
                    else if (promptResult.status == "failed")
                        {
                            Debug.Log("Prediction failed: " + promptResult.error);
                            break;
                        }
                }
                catch (JsonException ex)
                {
                    Debug.LogError("JSON Deserialization Exception: " + ex.Message);
                }

                Debug.Log("Response Status Code: " + response.StatusCode);
                Debug.Log("Response Content: " + responseContent);
            }
            catch (HttpRequestException ex)
            {
            Debug.LogError("HTTP Request Exception: " + ex.Message);
            }
        }
    }
}

