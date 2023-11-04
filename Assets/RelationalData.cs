using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class RelationalData : MonoBehaviour
{   
    // singleton static reference
    private static RelationalData assignAsset;
    public static RelationalData data {
        get {
            if (assignAsset == null) {
                assignAsset = FindObjectOfType<RelationalData>();
                if (assignAsset == null)
                    Debug.LogError("There is no RelationalData in the scene!");
            }
            return assignAsset;
        }
    }

    public string filePath = "/saves/gamesave.mds";

    public enum StatType {
        Health = 0,
        Armor = 1,
        Damage = 2,
        AbilityStrength = 3,
        Accuracy = 4,
        CritcalChance = 5,
        Threat = 6,
        FireResistance = 7,
        LightResistance = 8,
        DarkResistance = 9
    }
    public Dictionary<int, string> statColor = new Dictionary<int, string>()
    {
        {0, "#12FF14"},    
        {1, "#BEBEBE"},
        {2, "#FF003D"},
        {3, "#2058B9"},
        {4, "#FFF900"},
        {5, "#FF6528"},
        {6, "#FF007D"}
    };

    public enum DamageType {
        Physical = 0,
        Fire = 1,
        Frost = 2,
        Poison = 3,
        Light = 4,
        Dark = 5,
    }
    public Dictionary<int, string> damageColor = new Dictionary<int, string>()
    {
        {0, "#FF003D"},    
        {1, "#FFA845"},
        {2, ""},
        {3, ""},
        {4, "#F3E7E2"},
        {5, "#483A41"},
    };

    public List<ItemBase> itemObjects = new List<ItemBase>();
    public List<UnitBase> unitObjects = new List<UnitBase>();

    public RandomName randName;

    public bool gameState = false;

    void Awake() {
        DontDestroyOnLoad(this.gameObject);

        itemObjects = Resources.LoadAll<ItemBase>("Items/").ToList();
        unitObjects = Resources.LoadAll<UnitBase>("Unit/").ToList();
        randName = Resources.Load("RandomName") as RandomName;
    }

    public void UpdateText(TextMeshProUGUI text, string strText, Color color) {
        text.text = strText;
        text.color = color;
    }
}
