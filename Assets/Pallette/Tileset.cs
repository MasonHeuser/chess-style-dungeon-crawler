using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Tileset", menuName = "ScriptableObjects/Tileset", order = 1)]
public class Tileset : ScriptableObject
{   
    public Dictionary<string, Tile> tileDefinition = new Dictionary<string, Tile>();

    public int randomFillPercent;
    
    public Tile wall;
    public Tile pillar;
    public Tile floor;

    public Tile topBorder;  
    public Tile rightBorder;
    public Tile leftBorder;

    public Tile leftStraightCorner;
    public Tile rightStraightCorner;

    public Tile leftAngleCorner;
    public Tile rightAngleCorner;

    public Tile leftCorner;
    public Tile rightCorner;

    public void DefineTiles() {

        tileDefinition.Add("North_False_South_True-0_East_True_West_True", wall);
        tileDefinition.Add("North_True_South_True-0_East_True0_West_True", wall); //Left
        tileDefinition.Add("North_True_South_True-0_East_True_West_True-0", wall); //Right
        tileDefinition.Add("North_True_South_True-0_East_True-0_West_True", wall);

        tileDefinition.Add("North_False_South_True_East_True_West_True", pillar);
        tileDefinition.Add("North_True-0_South_True-0_East_True-0_West_True-0", pillar);

        tileDefinition.Add("North_True-0_South_False_East_True_West_True", topBorder);
        tileDefinition.Add("North_True_South_False_East_True_West_True", topBorder);
        tileDefinition.Add("North_True_South_True_East_False_West_True-0", leftBorder);
        tileDefinition.Add("North_True_South_True_East_True-0_West_False", rightBorder);

        tileDefinition.Add("North_True-0_South_True_East_True_West_True-0", leftAngleCorner);
        tileDefinition.Add("North_True-0_South_True_East_True-0_West_True", rightAngleCorner);

        tileDefinition.Add("North_True_South_False_East_False_West_True", leftCorner);
        tileDefinition.Add("North_True_South_False_East_True_West_False", rightCorner);

        tileDefinition.Add("North_False_South_True_East_False_West_True", leftStraightCorner);
        tileDefinition.Add("North_True_South_True_East_False_West_True", leftStraightCorner);
        tileDefinition.Add("North_False_South_True_East_True_West_False", rightStraightCorner);
        tileDefinition.Add("North_True_South_True_East_True_West_False", rightStraightCorner);
    }
}
