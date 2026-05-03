using UnityEngine;

// Adjusts the camera's orthographic size so each source pixel maps to exactly
// `targetScale` screen pixels — no fractional scaling, no blurring.
// Formula: orthoSize = screenHeight / (2 * pixelsPerUnit * targetScale)
[RequireComponent(typeof(Camera))]
public class PixelPerfectCam : MonoBehaviour
{
    [SerializeField] int pixelsPerUnit = 32;
    [SerializeField] int targetScale   = 4; // screen pixels per source pixel

    void Awake()
    {
        var cam = GetComponent<Camera>();
        cam.orthographicSize = Screen.height / (2f * pixelsPerUnit * targetScale);
    }
}
