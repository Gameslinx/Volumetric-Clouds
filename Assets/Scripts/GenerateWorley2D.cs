using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class SubGrid
{
    int pixelsX = 0;
    int pixelsY = 0;
    public int x;
    public int y;
    public SubGrid(int pixelsX, int pixelsY) 
    { 
        this.pixelsX = pixelsX;
        this.pixelsY = pixelsY;
    }
    public void GeneratePoint()
    {
        x = Random.Range(0, pixelsX);
        y = Random.Range(0, pixelsY);
    }
}

public class GenerateWorley2D : MonoBehaviour
{
    [SerializeField]
    public Texture2D texture;

    public int numGrids;
    public int gridSize;
    public SubGrid[,] grids;
    public SubGrid[,] copiedGrids;

    public void Generate()
    {
        // Generate grids
        Random.InitState(1);

        grids = new SubGrid[numGrids, numGrids];
        for (int i = 0; i < numGrids; i++)
        {
            for (int j = 0; j < numGrids; j++)
            {
                grids[i, j] = new SubGrid(gridSize, gridSize);
                grids[i, j].GeneratePoint();
            }
        }
        // Copy the grids around the central grid
        copiedGrids = new SubGrid[numGrids * 3, numGrids * 3];

        int mainGridIndexX = 0;
        int mainGridIndexY = 0;

        for (int mainGridX = 0; mainGridX < 3; mainGridX++)
        {
            for (int mainGridY = 0; mainGridY < 3; mainGridY++)
            {
                // Copy data
                mainGridIndexX = mainGridX * numGrids;
                mainGridIndexY = mainGridY * numGrids;  
                for (int i = 0; i < numGrids; i++)
                {
                    for (int j = 0; j < numGrids; j++)
                    {
                        copiedGrids[mainGridIndexX + i, mainGridIndexY + j] = grids[i, j];
                    }
                }
            }
        }
        ConstructImage();
    }
    public void ConstructFullGridImage()
    {
        int imageWidth = numGrids * gridSize * 3;
        int imageHeight = numGrids * gridSize * 3;

        texture = new Texture2D(imageWidth, imageHeight);

        for (int i = 0; i < imageWidth; i++)
        {
            for (int j = 0; j < imageHeight; j++)
            {
                int gridIndexX = i / gridSize;
                int gridIndexY = j / gridSize;

                SubGrid grid = copiedGrids[gridIndexX, gridIndexY];

                int pixelCoordX = gridIndexX * gridSize + grid.x;
                int pixelCoordY = gridIndexY * gridSize + grid.y;

                texture.SetPixel(i, j, Color.green);
                texture.SetPixel(pixelCoordX, pixelCoordY, Color.red);
                if (i % gridSize == 0 || j % gridSize == 0)
                {
                    texture.SetPixel(i, j, Color.black);
                }
            }
        }

        texture.Apply();
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
    }
    public void ConstructImage()
    {
        int imageWidth = numGrids * gridSize;
        int imageHeight = numGrids * gridSize;

        int centralGridStartX = numGrids;
        int centralGridStartY = numGrids;

        texture = new Texture2D(imageWidth, imageHeight);

        float maxPossibleDist = Mathf.Sqrt(gridSize * gridSize + gridSize * gridSize);

        for (int i = 0; i < imageWidth; i++)
        {
            for (int j = 0; j < imageHeight; j++)
            {
                int gridIndexX = i / gridSize + centralGridStartX;
                int gridIndexY = j / gridSize + centralGridStartY;

                SubGrid grid = copiedGrids[gridIndexX, gridIndexY];

                int pixelCoordX = gridIndexX * gridSize + grid.x;
                int pixelCoordY = gridIndexY * gridSize + grid.y;

                float dist = GetDistToClosest(gridIndexX, gridIndexY, i + centralGridStartX * gridSize, j + centralGridStartY * gridSize);

                texture.SetPixel(i, j, Color.white * dist / maxPossibleDist); 
                //texture.SetPixel(pixelCoordX, pixelCoordY, Color.red);
                //if (i % gridSize == 0 || j % gridSize == 0)
                //{
                //    texture.SetPixel(i, j, Color.black);
                //}
            }
        }

        texture.Apply();
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
    }
    public float GetDistToClosest(int gridX, int gridY, int pixelX, int pixelY)
    {
        float dist = 10000000;
        SubGrid grid = copiedGrids[gridX, gridY];
        Vector2 coord1 = new Vector2(pixelX, pixelY);
        Vector2 coord2 = Vector2.zero;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                //if (i == 0 || j == 0)
                //{
                //    continue;
                //}
                SubGrid adjacentGrid = copiedGrids[gridX + i, gridY + j];
                int realPosX = (gridX + i) * gridSize + adjacentGrid.x;
                int realPosY = (gridY + j) * gridSize + adjacentGrid.y;
                coord2.x = realPosX;
                coord2.y = realPosY;
                float distance = Vector2.Distance(coord1, coord2);
                if (distance < dist)
                {
                    dist = distance;
                }
            }
        }
        return dist;
    }
}

[CustomEditor(typeof(GenerateWorley2D))]
public class GenerateWorleyButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GenerateWorley2D generateWorley = target as GenerateWorley2D;
        generateWorley.texture = (Texture2D)EditorGUILayout.ObjectField("Texture", generateWorley.texture, typeof(Texture2D), false);
        //generateWorley.dim = (int)EditorGUILayout.Slider(generateWorley.dim, 1, 1024);
        if (GUILayout.Button("Generate"))
        {
            
            generateWorley.Generate();
        }
    }
}
