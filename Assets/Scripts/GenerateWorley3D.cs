using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;
public class SubGrid3D
{
    int pixelsX = 0;
    int pixelsY = 0;
    int pixelsZ = 0;
    public int x;
    public int y;
    public int z;
    public SubGrid3D(int pixelsX, int pixelsY, int pixelsZ)
    {
        this.pixelsX = pixelsX;
        this.pixelsY = pixelsY;
        this.pixelsZ = pixelsZ;
    }
    public void GeneratePoint()
    {
        x = Random.Range(0, pixelsX);
        y = Random.Range(0, pixelsY);
        z = Random.Range(0, pixelsZ);
    }
}

public class GenerateWorley3D : EditorWindow
{
    static private GenerateWorley3D mWindow;

    public static Texture3D texture;

    public static int numGrids;
    public static int gridSize;
    public static SubGrid3D[,,] grids;
    public static SubGrid3D[,,] copiedGrids;

    string path = "Assets/Textures/WorleyNoises/Separate.asset";

    [MenuItem("Generate/Worley Noise")]
    private static void Initialize()
    {
        mWindow = GetWindow<GenerateWorley3D>("Worley Texture Generator");
        mWindow.Show();
    }
    public void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        path = EditorGUILayout.TextField("path", path);
        numGrids = EditorGUILayout.IntField("numGrids", numGrids);
        gridSize = EditorGUILayout.IntField("gridSize", gridSize);
        EditorGUILayout.LabelField("Resolution: " + (numGrids * gridSize));
        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }
    public void Generate()
    {
        // Generate grids
        //Random.InitState(1);

        grids = new SubGrid3D[numGrids, numGrids, numGrids];
        for (int i = 0; i < numGrids; i++)
        {
            for (int j = 0; j < numGrids; j++)
            {
                for (int k = 0;  k < numGrids; k++)
                {
                    grids[i, j, k] = new SubGrid3D(gridSize, gridSize, gridSize);
                    grids[i, j, k].GeneratePoint();
                }
            }
        }
        // Copy the grids around the central grid
        copiedGrids = new SubGrid3D[numGrids * 3, numGrids * 3, numGrids * 3];

        int mainGridIndexX = 0;
        int mainGridIndexY = 0;
        int mainGridIndexZ = 0;

        for (int mainGridX = 0; mainGridX < 3; mainGridX++)
        {
            for (int mainGridY = 0; mainGridY < 3; mainGridY++)
            {
                for (int mainGridZ = 0; mainGridZ  < 3; mainGridZ++)
                {
                    // Copy data
                    mainGridIndexX = mainGridX * numGrids;
                    mainGridIndexY = mainGridY * numGrids;
                    mainGridIndexZ = mainGridZ * numGrids;
                    for (int i = 0; i < numGrids; i++)
                    {
                        for (int j = 0; j < numGrids; j++)
                        {
                            for (int k = 0; k < numGrids; k++)
                            {
                                copiedGrids[mainGridIndexX + i, mainGridIndexY + j, mainGridIndexZ + k] = grids[i, j, k];
                            }
                        }
                    }
                }
            }
        }
        ConstructImage();
    }
    public void ConstructImage()
    {
        int imageWidth = numGrids * gridSize;
        int imageHeight = numGrids * gridSize;
        int imageDepth = numGrids * gridSize;

        int centralGridStartX = numGrids;
        int centralGridStartY = numGrids;
        int centralGridStartZ = numGrids;

        texture = new Texture3D(imageWidth, imageHeight, imageDepth, TextureFormat.RFloat, false);

        float maxPossibleDist = Mathf.Sqrt(gridSize * gridSize + gridSize * gridSize + gridSize * gridSize);

        for (int i = 0; i < imageWidth; i++)
        {
            for (int j = 0; j < imageHeight; j++)
            {
                for (int k = 0; k < imageDepth; k++)
                {
                    int gridIndexX = i / gridSize + centralGridStartX;
                    int gridIndexY = j / gridSize + centralGridStartY;
                    int gridIndexZ = k / gridSize + centralGridStartZ;

                    SubGrid3D grid = copiedGrids[gridIndexX, gridIndexY, gridIndexZ];

                    float dist = GetDistToClosest(gridIndexX, gridIndexY, gridIndexZ, i + centralGridStartX * gridSize, j + centralGridStartY * gridSize, k + centralGridStartZ * gridSize);

                    texture.SetPixel(i, j, k, Color.white - Color.white * dist / (maxPossibleDist / 1.5f));
                }
            }
        }

        texture.Apply();
        Texture2D slice = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0;j < texture.height; j++)
            {
                float col = texture.GetPixel(i, j, texture.depth / 2).r;
                slice.SetPixel(i, j, Color.white  * col);
            }
        }
        slice.Apply();
        GameObject.Find("WorleyPlane").GetComponent<MeshRenderer>().sharedMaterial.mainTexture = slice;
        AssetDatabase.CreateAsset(texture, path);
    }
    public float GetDistToClosest(int gridX, int gridY, int gridZ, int pixelX, int pixelY, int pixelZ)
    {
        float dist = 10000000;
        Vector3 coord1 = new Vector3(pixelX, pixelY, pixelZ);
        Vector3 coord2 = Vector3.zero;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                for (int k = -1; k < 2; k++)
                {
                    SubGrid3D adjacentGrid = copiedGrids[gridX + i, gridY + j, gridZ + k];
                    int realPosX = (gridX + i) * gridSize + adjacentGrid.x;
                    int realPosY = (gridY + j) * gridSize + adjacentGrid.y;
                    int realPosZ = (gridZ + k) * gridSize + adjacentGrid.z;
                    coord2.x = realPosX;
                    coord2.y = realPosY;
                    coord2.z = realPosZ;
                    float distance = Vector3.Distance(coord1, coord2);
                    if (distance < dist)
                    {
                        dist = distance;
                    }
                }
            }
        }
        return dist;
    }
}