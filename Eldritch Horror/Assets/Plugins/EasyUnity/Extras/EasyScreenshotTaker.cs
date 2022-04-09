using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;

public class EasyScreenshotTaker : MonoBehaviour
{
    [SerializeField] protected Camera myCamera;

    protected static List<EasyScreenshotTaker> instances = new List<EasyScreenshotTaker>();

    public static IEnumerable<EasyScreenshotTaker> GetInstances => instances.AsEnumerable();

    protected void Awake() => instances.Add(this);
    protected void OnDestroy() => instances.Remove(this);

    protected Action<Texture2D> Callback { get; set; }

    public void TakeScreenshot(Action<Texture2D> callback)
    {
        if (Callback != null) throw new InvalidOperationException($"ScreenshotTaker {name} is busy waiting for another screenshot");
        if (myCamera == null) throw new ArgumentNullException(nameof(myCamera));
        if (myCamera.targetTexture != null) throw new InvalidOperationException($"Camera {myCamera.name} already has an assigned Target Texture.");

        Callback = callback ?? throw new ArgumentNullException(nameof(callback));

        myCamera.targetTexture = RenderTexture.GetTemporary(Screen.width, Screen.height);
        RenderPipelineManager.endCameraRendering += RenderPipelineManager_endCameraRendering;
    }

    private void RenderPipelineManager_endCameraRendering(ScriptableRenderContext ctx, Camera camera)
    {
        RenderPipelineManager.endCameraRendering -= RenderPipelineManager_endCameraRendering;
        if (Callback != null)
        {
            RenderTexture renderTexture = camera.targetTexture;
            Texture2D resultTexture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            resultTexture.ReadPixels(rect, 0, 0);
            RenderTexture.ReleaseTemporary(renderTexture);
            camera.targetTexture = null;
            Callback.Invoke(resultTexture);
            Callback = null;
        }
    }
}
