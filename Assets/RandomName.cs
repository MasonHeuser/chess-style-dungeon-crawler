using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomName", menuName = "ScriptableObjects/RandomName")]
public class RandomName : ScriptableObject
{
    public List<string> maleName = new List<string>();
    public List<string> femaleName = new List<string>();

    public string RandomHandle(UnitBase.Gender gender) {

        if (gender == UnitBase.Gender.Male) {
            return maleName[Random.Range(0, maleName.Count)];
        } else if (gender == UnitBase.Gender.Female) {
            return femaleName[Random.Range(0, femaleName.Count)];
        }

        List<string> allName = new List<string>();
        allName.AddRange(maleName);
        allName.AddRange(femaleName);
        return allName[Random.Range(0, allName.Count)];       
    }
}
