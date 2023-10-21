using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class TexCombiner : EditorWindow
{
    static private TexCombiner window;
    public static string path = "Assets/Textures/Combined3DTex.asset";
    public static Texture3D tex1;
    public static Texture3D tex2;
    public static Texture3D tex3;
    public static Texture3D tex4;
    public static Texture3D resultTex;
    [MenuItem("Generate/Combine 3D Textures")]
    private static void Initialize()
    {
        window = GetWindow<TexCombiner>("3D Texture Combiner");
    }
    private void OnEnable()
    {
        
    }
    public void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        path = EditorGUILayout.TextField("path", path);
        tex1 = (Texture3D)EditorGUILayout.ObjectField("Red Channel (Norm. Input)", tex1, typeof(Texture3D), false);
        tex2 = (Texture3D)EditorGUILayout.ObjectField("Green Channel", tex2, typeof(Texture3D), false);
        tex3 = (Texture3D)EditorGUILayout.ObjectField("Blue Channel", tex3, typeof(Texture3D), false);
        tex4 = (Texture3D)EditorGUILayout.ObjectField("Alpha Channel", tex4, typeof(Texture3D), false);

        if (GUILayout.Button("Combine", GUILayout.MaxWidth(200f)))
        {
            Combine();
        }
        if (GUILayout.Button("Multiply Red", GUILayout.MaxWidth(200f)))
        {
            CombineMultiply();
        }
        if (GUILayout.Button("Normalize", GUILayout.MaxWidth(200f)))
        {
            Normalize();
        }
    }
    public void Combine()
    {
        if (tex1 ==  null || tex2 == null || tex3 == null || tex4 == null) { return; }
        if ((tex1.width + tex2.width + tex3.width + tex4.width) / 4 != tex1.width) { return; }
        int width = tex1.width;

        resultTex = new Texture3D(width, width, width, TextureFormat.ARGB32, false);

        Color col1 = Color.black;
        Color col2 = Color.black;
        Color col3 = Color.black;
        Color col4 = Color.black;

        Color result = Color.black;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    col1 = tex1.GetPixel(i, j, k);
                    col2 = tex2.GetPixel(i, j, k);
                    col3 = tex3.GetPixel(i, j, k);
                    col4 = tex4.GetPixel(i, j, k);

                    result.r = col1.r;
                    result.g = col2.r;
                    result.b = col3.r;
                    result.a = col4.r;

                    resultTex.SetPixel(i, j, k, result);
                }
            }
        }

        resultTex.Apply();
        AssetDatabase.CreateAsset(resultTex, path);
    }
    public void CombineMultiply()
    {
        if (tex1 == null || tex2 == null || tex3 == null || tex4 == null) { return; }
        if ((tex1.width + tex2.width + tex3.width + tex4.width) / 4 != tex1.width) { return; }
        int width = tex1.width;

        resultTex = new Texture3D(width, width, width, TextureFormat.ARGB32, false);

        Color col1 = Color.black;
        Color col2 = Color.black;
        Color col3 = Color.black;
        Color col4 = Color.black;

        Color result = Color.black;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                for (int k = 0; k < width; k++)
                {
                    col1 = tex1.GetPixel(i, j, k);
                    col2 = tex2.GetPixel(i, j, k);
                    col3 = tex3.GetPixel(i, j, k);
                    col4 = tex4.GetPixel(i, j, k);

                    result.r = (col1.r + col2.r / 2f + col3.r / 4f + col4.r / 8f) / 1.875f;

                    resultTex.SetPixel(i, j, k, result);
                }
            }
        }

        resultTex.Apply();
        AssetDatabase.CreateAsset(resultTex, path);
    }
    public void Normalize()
    {
        int width = tex1.width;
        resultTex = new Texture3D(width, width, width, TextureFormat.ARGB32, false);

        float largestPixel = 0;
        float smallestPixel = 1000;
        for (int i = 0; i < tex1.width; i++)
        {
            for (int j = 0; j < tex1.height; j++)
            {
                for (int k = 0; k < tex1.depth; k++)
                {
                    float pixel = tex1.GetPixel(i, j, k).r;
                    if (pixel > largestPixel)
                    {
                        largestPixel = pixel;
                    }
                    if (pixel < smallestPixel)
                    {
                        smallestPixel = pixel;
                    }
                }
            }
        }
        for (int i = 0; i < tex1.width; i++)
        {
            for (int j = 0; j < tex1.height; j++)
            {
                for (int k = 0; k < tex1.depth; k++)
                {
                    float pixel = tex1.GetPixel(i, j, k).r;
                    float newR = (pixel - smallestPixel) / (largestPixel - smallestPixel);
                    resultTex.SetPixel(i, j, k, newR * Color.white);
                }
            }
        }
        resultTex.Apply();
        AssetDatabase.CreateAsset(resultTex, path);
    }
}
