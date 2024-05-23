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
    public static string color_miss = "<color=#909090>";
    public static string color_pcsDamage = "<color=#FB4000>";
    public static string color_pcsCriticalDamage = "<color=red>";
    public static string color_mgcDamage = "<color=#5864E0>";
    public static string color_mgcCriticalDamage = "<color=#5026FF>";
    public static string color_broken = "<color=#B0B0B0>";
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

    public static string color_buff = "<color=#7ed1ff>";
    public static string color_debuff = "<color=#c90223>";

    public static string color_normalEnemyName = "<color=#FFFFFF>";
    public static string color_middleBossEnemyName = "<color=#C02E2E>";
    public static string color_bossEnemyName = "<color=#FF0000>";

    public static Quaternion QI => Quaternion.identity;

    public static string GetStatColor(EntityStat stat)
    {
        switch (stat)
        {
            case EntityStat.Str:
                return color_Str + "Str</color>";
            case EntityStat.Int:
                return color_Int + "Int</color>";
            case EntityStat.MPChargeSpeed:
                return color_mana + "MPCharge</color>";
            case EntityStat.MaxHP:
                return color_HP + "MaxHP</color>";
            case EntityStat.CurrentHP:
                return color_HP + "CurrentHP</color>";
            case EntityStat.PDef:
                return color_Str + "PscDef</color>";
            case EntityStat.MDef:
                return color_Int + "MgcDef</color>";
            case EntityStat.Dodge:
                return color_miss + "Dodge</color>";
            case EntityStat.Acc:
                return color_miss + "Acc</color>";
            default:
                return "";
        }
    }

    public static string GetSkillTypeColor(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.PscDamage:
                return "Deal " + color_pcsDamage + "PscDamge</color>";
            case SkillType.MgcDamage:
                return "Deal " + color_mgcDamage + "MgcDamge</color>";
            case SkillType.Heal:
                return color_HP + "Heal</color>";
            case SkillType.Buff:
                return color_buff + "Gain</color>";
            case SkillType.Debuff:
                return color_debuff + "Inflict</color>";
            default:
                return "";
        }
    }
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