using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Android;

public class TakePhoto : MonoBehaviour
{
    public WebCamTexture webCamTexture;
    public RawImage display;
    public AspectRatioFitter fit;

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

        // Save the image to Gallery/Photos
        string fileName = "Image.png";
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(texture, "GalleryTest", fileName, (success, path) =>
        {
            if (success)
            {
                Debug.Log("Image saved to gallery: " + path);

                // Save the image path to PlayerPrefs
                PlayerPrefs.SetString("LastImagePath", path);
            }
            else
            {
                Debug.LogError("Failed to save image to gallery");
            }
        });

        Debug.Log("Permission result: " + permission);

        // Destroy the texture to avoid memory leaks
        Destroy(texture);
    }
}

