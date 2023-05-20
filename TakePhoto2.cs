using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Android;

public class TakePhoto2 : MonoBehaviour
{
    public WebCamTexture webCamTexture;
    public RawImage display;
    public AspectRatioFitter fit;
    public ImageToTextConverter imageToTextConverter;

    public void Start()
    {
        // Start the camera feed

        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }

        WebCamDevice[] devices = WebCamTexture.devices;

        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing)
            {
                webCamTexture = new WebCamTexture(devices[i].name);
                break;
            }
        }

        webCamTexture.requestedWidth = 966;
        webCamTexture.requestedHeight = 966;

        display.texture = webCamTexture;
        webCamTexture.Play();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(CaptureImageAndSave());
        }
    }

    public IEnumerator CaptureImageAndSave()
    {
        yield return new WaitForEndOfFrame();

        // Create a new Texture2D using the camera feed
        Texture2D texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGB24, false);
        texture.SetPixels(webCamTexture.GetPixels());
        texture.Apply();

        // Save the image to PlayerPrefs
        string fileName = "Image.png";
        string imagePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(imagePath, texture.EncodeToPNG());
        PlayerPrefs.SetString("LastImagePath", imagePath);

        // Destroy the texture to avoid memory leaks
        Destroy(texture);

        // Call the LoadImageAndGenerateText method from the ImageToTextConverter script
        if (imageToTextConverter != null)
        {
            yield return StartCoroutine(imageToTextConverter.LoadImageAndGenerateText());
        }
        else
        {
            Debug.LogError("ImageToTextConverter script not found.");
        }
    }
}
