using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadImage : MonoBehaviour
{
    public RawImage display;

    private void Start()
    {
        // Load and display the last captured image, if available
        string lastImagePath = PlayerPrefs.GetString("LastImagePath");
        if (!string.IsNullOrEmpty(lastImagePath))
        {
            StartCoroutine(LoadImageFromGallery(lastImagePath));
        }
    }

    IEnumerator LoadImageFromGallery(string path)
    {

        // Load the image from the given path using NativeGallery
        Texture2D texture = NativeGallery.LoadImageAtPath(path, -1, false);

        if (texture == null)
        {
            Debug.LogError("Failed to load image from gallery");
            yield break;
        }

        // Set the loaded image as the texture of the RawImage component
        display.texture = texture;
        display.SetNativeSize();
    }
}
