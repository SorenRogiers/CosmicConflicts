using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* ImageToVoxelGenerator
 * *********************
 * Class designed to create a world based off an black and white texture
 */ 
public class ImageToVoxelGenerator : MonoBehaviour {

    public QuadtreeComponent QuadtreeComponent;

    private static string MapNameKey = "MAP_NAME";

    private string _mapName;
    private string _tutorialMapName;

    private float _thresHold = 0.1f;
    private Texture2D _image;

    void Start()
    {
        //Load the chosen map from the player preferences and create a level
        if(PlayerPrefs.HasKey(MapNameKey))
            _mapName = PlayerPrefs.GetString(MapNameKey);

        _image = Resources.Load<Texture2D>("Textures/Maps/" + _mapName);
        
        Generate();
    }

    //Use an image texture to create the world.
    //Only inserting data into the quadtree where a pixel value is white.
    private void Generate()
    {
        int cells = (int)Mathf.Pow(2, QuadtreeComponent.Depth);

        for(int x = 0; x <= cells;++x)
        {
            for (int y = 0; y <= cells; ++y)
            {
                Vector2 position = QuadtreeComponent.transform.position;
                position.x += ((x - cells / 2) / (float)cells) * QuadtreeComponent.Size;
                position.y += ((y - cells / 2) / (float)cells) * QuadtreeComponent.Size;

                Color pixel = _image.GetPixelBilinear(x / (float)cells, y / (float)cells);

                if(pixel.r > _thresHold && pixel.g > _thresHold && pixel.b > _thresHold)
                {
                    QuadtreeComponent.Quadtree.InsertExplosion(position, 0.0001f, 1);
                }
            }
        }
    }
}
