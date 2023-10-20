using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcess : MonoBehaviour
{
    public Material postProcessingMat;
    public Camera cam;
    public void Start()
    {
        
    }
    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, postProcessingMat);
    }
    private void Update()
    {
        Vector3[] frustumCorners = new Vector3[4];
        cam = gameObject.GetComponent<Camera>();
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), cam.farClipPlane, cam.stereoActiveEye, frustumCorners);
        var bl = transform.TransformVector(frustumCorners[0]);
        var tl = transform.TransformVector(frustumCorners[1]);
        var tr = transform.TransformVector(frustumCorners[2]);
        var br = transform.TransformVector(frustumCorners[3]);
        //
        Matrix4x4 frustumCornersArray = Matrix4x4.identity;
        frustumCornersArray.SetRow(0, bl);
        frustumCornersArray.SetRow(1, br);
        frustumCornersArray.SetRow(2, tl);
        frustumCornersArray.SetRow(3, tr);

        postProcessingMat.SetMatrix("_FrustumCorners", frustumCornersArray);
        postProcessingMat.SetInt("_FrameCount", Time.frameCount);
    }
}
