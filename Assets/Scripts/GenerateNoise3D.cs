using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GenerateNoise3D : MonoBehaviour
{
    public Texture3D texture;
    public int size = 64;
    [Range(0.01f, 40f)]
    public float scale = 1.0f;
    [Range(1, 8)]
    public int octaves = 1;
    [Range(0.01f, 1f)]
    public float persistence = 0.5f;
    [Range(1f, 4f)]
    public float lacunarity = 0.5f;
    public void Generate()
    {
        texture = new Texture3D(size, size, size, TextureFormat.RFloat, false);
        Vector3 pos = Vector3.zero;
        float noise = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    pos.x = (float)i / (float)size;
                    pos.y = (float)j / (float)size;
                    pos.z = (float)k / (float)size;
                    pos *= scale;
                    noise = GetNoise(pos, octaves, persistence, lacunarity);
                    texture.SetPixel(i, j, k, Color.white * noise);
                }
            }
        }
        Normalize();
        texture.Apply();
        gameObject.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
        AssetDatabase.CreateAsset(texture, "Assets/Textures/Perlin3D.asset");
    }
    public float GetNoise(Vector3 pos, int octaves, float persistence, float lacunarity)
    {
        return Perlin.Noise(pos);
        float noise = 0;
        for (int i = 0; i < octaves; i++)
        {
            noise += (Perlin.Noise(pos * lacunarity * (i + 1)) * 0.5f + 0.5f) * Mathf.Pow(persistence, (float)i);
        }
        noise /= octaves;
        return noise;
    }
    public void Normalize()
    {
        float largest = 0;
        float smallest = 100;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    Color pixel = texture.GetPixel(i, j, k);
                    if (pixel.r > largest)
                    {
                        largest = pixel.r;
                    }
                    if (pixel.r < smallest)
                    {
                        smallest = pixel.r;
                    }
                }
            }
        }
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                for (int k = 0; k < size; k++)
                {
                    Color pixel = texture.GetPixel(i, j, k);
                    texture.SetPixel(i, j, k, Color.white * ((pixel.r - smallest) / (largest - smallest)));
                }
            }
        }
    }
}

[CustomEditor(typeof(GenerateNoise3D))]
public class GenerateNoise3DButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GenerateNoise3D generateNoise = target as GenerateNoise3D;
        generateNoise.texture = (Texture3D)EditorGUILayout.ObjectField("Texture", generateNoise.texture, typeof(Texture3D), false);
        //generateWorley.dim = (int)EditorGUILayout.Slider(generateWorley.dim, 1, 1024);
        if (GUILayout.Button("Generate"))
        {

            generateNoise.Generate();
        }
    }
}
