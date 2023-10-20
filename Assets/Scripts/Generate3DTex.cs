using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Generate3DTex : MonoBehaviour
{
    [SerializeField]
    public Texture3D voxelTexture;
    [SerializeField]
    public int textureSize = 128;
    void Start()
    {
        
    }

    public void GenerateVoxelTexture()
    {
        voxelTexture = new Texture3D(textureSize, textureSize, textureSize, TextureFormat.ARGB32, false);

        Color[] colors = new Color[textureSize * textureSize * textureSize];

        Vector3 center = new Vector3(textureSize / 2f, textureSize / 2f, textureSize / 2f);

        for (int z = 0; z < textureSize; z++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    Vector3 position = new Vector3(x, y, z);
                    float distance = Vector3.Distance(position, center) / ((float)textureSize); // Normalize distance
                    int index = x + y * textureSize + z * textureSize * textureSize;
                    colors[index] = new Color(1 - distance, 1 - distance, 1 - distance, 1f);
                }
            }
        }

        voxelTexture.SetPixels(colors);
        voxelTexture.Apply();
    }
}
[CustomEditor(typeof(Generate3DTex))]
public class GenerateTexScript : Editor 
{
    public override void OnInspectorGUI()
    {
        var script = target as Generate3DTex;

        script.voxelTexture = (Texture3D)EditorGUILayout.ObjectField("Texture", script.voxelTexture, typeof(Texture3D), false);
        script.textureSize = EditorGUILayout.IntField("Texture Size", script.textureSize);

        if (GUILayout.Button("Generate 3D Texture"))
        {
            script.GenerateVoxelTexture();
        }
    }
}
