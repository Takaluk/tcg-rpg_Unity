using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResolution : MonoBehaviour
{
    public float widhtRes = 9.0f;
    public float heightRes = 16.0f;
    private void Awake()
    {
        Camera cam = GetComponent<Camera>();

        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float currentAspectRatio = screenWidth / screenHeight;

        float targetAspectRatio = widhtRes / heightRes;


        if (currentAspectRatio < targetAspectRatio)
        {
            float widthDifference = targetAspectRatio * screenHeight - screenWidth;

            float sizeDifference = widthDifference / 2f / cam.pixelWidth * cam.orthographicSize * 2f;

            float newSize = cam.orthographicSize + sizeDifference;
            cam.orthographicSize = newSize;
        }
    }
}
