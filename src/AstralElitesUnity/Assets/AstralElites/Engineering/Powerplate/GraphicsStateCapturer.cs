#if DEBUG
using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class GraphicsStateCapturer : MonoBehaviour
{
    private GraphicsStateCollection collection;
    private bool isCapturing;

#if UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void GSC_DownloadBytes(string filename, byte[] data, int length);
#endif

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame && !isCapturing)
            BeginCapture();
        else if (Keyboard.current.digit2Key.wasPressedThisFrame && isCapturing)
            EndCapture();
    }

    private void BeginCapture()
    {
        collection = new GraphicsStateCollection();
        collection.BeginTrace();
        isCapturing = true;
        Debug.Log("[GSC] Capture started.");
    }

    private void EndCapture()
    {
        collection.EndTrace();
        isCapturing = false;
        Debug.Log("[GSC] Capture ended. Exporting.");
        StartCoroutine(ExportCoroutine());
    }

    private IEnumerator ExportCoroutine()
    {
        // Yield two frames so the EndCapture flush can complete before we read.
        yield return null;
        yield return null;

        string path = Path.Combine(Application.persistentDataPath, "AstralElites.gsc");

        try
        {
            collection.SaveToFile(path);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GSC] Save failed: {e.Message}");
            yield break;
        }

        // Yield another frame so the WebGL file system can flush to IndexedDB.
        yield return null;

        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            Debug.LogError($"[GSC] Read-back failed: {e.Message}");
            yield break;
        }

        Debug.Log($"[GSC] Captured {bytes.Length} bytes.");

#if UNITY_WEBGL
        GSC_DownloadBytes("AstralElites.gsc", bytes, bytes.Length);
#else
        Debug.Log($"[GSC] Saved to: {path}");
#endif

        // Base64 console fallback — paste into a decoder if the download fails.
        Debug.Log($"[GSC:base64] {Convert.ToBase64String(bytes)}");
    }
}
#endif
