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
using System.Collections.Generic;

public class Response
{
    public string id { get; set; }
    public string version { get; set; }
    public PredictionInput input { get; set; }
    public string logs { get; set; }
    public List<string> output { get; set; }
    public object error { get; set; }
    public string status { get; set; }
    public DateTime created_at { get; set; }
    public DateTime started_at { get; set; }
    public DateTime completed_at { get; set; }
    public Metrics metrics { get; set; }
    public ImageUrls urls { get; set; }
}

public class PredictionInput
{
    public string prompt { get; set; }
}

public class Metrics
{
    public double predict_time { get; set; }
}

public class ImageUrls
{
    public string cancel { get; set; }
    public string get { get; set; }
}

public class prompt2image : MonoBehaviour
{
    string idrequest = "";
    public TMPro.TMP_Text generatedprompt;
    public RawImage display;

    // Send the prompt to replicate
    public async void Start()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Token r8_0D3MzXy3oqHGCIYkO0VjdBCz8VFFPMN29wcjq");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.replicate.com/v1/predictions");
        var content = new StringContent(JsonConvert.SerializeObject(new
        {
            version = "db21e45d3f7023abc2a46ee38a23973f6dce16bb082a930b0c49861f96d1e5bf",
            input = new PredictionInput { prompt = generatedprompt.text }
        }), Encoding.UTF8, "application/json");

        request.Content = content;

        try
        {
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.Log("Response Status Code: " + response.StatusCode);
            Debug.Log("Response Content: " + responseContent);

            Response obj;
            try
            {
                obj = JsonConvert.DeserializeObject<Response>(responseContent);
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
            client.DefaultRequestHeaders.Add("Authorization", "Token r8_0D3MzXy3oqHGCIYkO0VjdBCz8VFFPMN29wcjq");

            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.replicate.com/v1/predictions/" + idrequest);

            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.Log("Polling Response Status Code: " + response.StatusCode);
                Debug.Log("Polling Response Content: " + responseContent);

                Response obj;
                try
                {
                    obj = JsonConvert.DeserializeObject<Response>(responseContent);
                    if (obj.status == "succeeded" && obj.output.Count > 0)
                    {
                        Debug.Log("Prediction Succeeded");
                        await GetTextureFromURL(obj.output[0]);
                        break;
                    }
                    else if (obj.status == "failed")
                    {
                        Debug.LogError("Prediction Failed");
                        break;
                    }
                }
                catch (JsonException ex)
                {
                    Debug.LogError("JSON Deserialization Exception: " + ex.Message);
                    break;
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.LogError("HTTP Request Exception: " + ex.Message);
                break;
            }
        }
    }

    public async Task GetTextureFromURL(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("URL is null or empty");
            return;
        }

        if (display == null)
        {
            Debug.LogError("Display object is null");
            return;
        }

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            var asyncOperation = www.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await Task.Delay(100);
            }

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                display.texture = texture;
            }
            else
            {
                Debug.LogError("Image Download Error: " + www.error);
            }
        }
    }

}