using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;


public class CaptureImage : MonoBehaviour
{
    WebCamTexture webCam;
    public RawImage display;
    public AspectRatioFitter fit;

    [SerializeField]
    public UnityEngine.UI.RawImage _rawImage;

    public void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        for (int i = 0; i < devices.Length; i++)
        {
            print("Webcam available: " + devices[i].name);
        }

        WebCamTexture tex = new WebCamTexture(devices[0].name);

        tex.requestedWidth = 1080;
        tex.requestedHeight = 1080;

        RawImage m_RawImage;
        m_RawImage = GetComponent<RawImage>();
        m_RawImage.texture = tex;
        tex.Play();
    }
}
