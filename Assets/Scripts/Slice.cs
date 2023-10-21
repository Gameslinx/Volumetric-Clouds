using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Slice : MonoBehaviour
{
    public Texture3D texture;
    public void SliceTexture()
    {
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false);
        for (int i = 0; i < tex.width; i++)
        {
            for (int j = 0; j < tex.height; j++)
            {
                float col = texture.GetPixel(i, j, texture.depth / 2).r;
                tex.SetPixel(i, j, Color.white * col);
            }
        }
        tex.Apply();
        gameObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;
    }
}
[CustomEditor(typeof(Slice))]
public class SliceButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        Slice slice = target as Slice;
        slice.texture = (Texture3D)EditorGUILayout.ObjectField("Texture", slice.texture, typeof(Texture3D), false);
        //generateWorley.dim = (int)EditorGUILayout.Slider(generateWorley.dim, 1, 1024);
        if (GUILayout.Button("Slice"))
        {
            slice.SliceTexture();
        }
    }
}