using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PRS
{
    public Vector3 pos;
    public Quaternion rot; 
    public Vector3 scale;

    public PRS(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        this.pos = pos;
        this.rot = rot;
        this.scale = scale;
    }
}

public class Utils : MonoBehaviour
{
    public static string color_miss = "<color=#727272>";
    public static string color_pcsDamage = "<color=#FB4000>";
    public static string color_pcsCriticalDamage = "<color=red>";
    public static string color_mgcDamage = "<color=#5864E0>";
    public static string color_mgcCriticalDamage = "<color=#5026FF>";
    public static string color_broken = "<color=#727272>";
    public static string color_heal = "<color=green>";
    public static string color_mana = "<color=#52B1F9>";
    public static string color_cool = "<color=#727272>";

    public static string color_player = "<color=white>";
    public static string color_enemy = "<color=#870000>";
    public static string color_skill = "<color=#52B1F9>";
    public static string color_equip = "<color=#BFBFBF>";
    public static string color_location = "<color=#1E90FF>";
    public static string color_event = "<color=#FFE273>";

    public static string color_Str = "<color=#FB4000>";
    public static string color_Int = "<color=#5864E0>";
    public static string color_HP = "<color=green>";

    public static Quaternion QI => Quaternion.identity;

    public static Vector3 MousePos
    {
        get
        {
            Vector3 result = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            result.z = -10;
            return result;
        }
    }

    public static bool CalculateProbability(float percent)
    {
        float probability = percent / 100f;
        float randomValue = Random.value;
        return randomValue <= probability;
    }
}