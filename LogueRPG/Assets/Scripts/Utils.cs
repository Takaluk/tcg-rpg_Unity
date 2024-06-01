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

    public static string GetStatName(EntityStat stat, bool withColor = true)
    {
        switch (stat)
        {
            case EntityStat.Str:
                if (withColor)
                    return color_Str + GameManager.instance.GetLocaleString("Battle-Stat-Str") + "</color>";
                else
                    return GameManager.instance.GetLocaleString("Battle-Stat-Str");
            case EntityStat.Int:
                if (withColor)
                    return color_Int + GameManager.instance.GetLocaleString("Battle-Stat-Int") + "</color>";
                else
                    return GameManager.instance.GetLocaleString("Battle-Stat-Int");
            case EntityStat.MPChargeSpeed:
                if (withColor)
                    return color_mana + GameManager.instance.GetLocaleString("Battle-Stat-MPChargeSpeed") + "</color>";
                else
                    return GameManager.instance.GetLocaleString("Battle-Stat-MPChargeSpeed");
            case EntityStat.MaxHP:
                if (withColor)
                    return color_HP + GameManager.instance.GetLocaleString("Battle-Stat-MaxHP") + "</color>";
                else
                    return GameManager.instance.GetLocaleString("Battle-Stat-MaxHP");
            case EntityStat.CurrentHP:
                if (withColor)
                    return color_HP + GameManager.instance.GetLocaleString("Battle-Stat-CurrentHP") + "</color>";
                else
                    return GameManager.instance.GetLocaleString("Battle-Stat-CurrentHP");
            case EntityStat.PDef:
                if (withColor)
                    return color_Str + GameManager.instance.GetLocaleString("Battle-Stat-PDef") + "</color>";
                else
                    return GameManager.instance.GetLocaleString("Battle-Stat-PDef");
            case EntityStat.MDef:
                return color_Int + GameManager.instance.GetLocaleString("Battle-Stat-MDef") + "</color>";
            case EntityStat.Dodge:
                if (withColor)
                    return color_miss + GameManager.instance.GetLocaleString("Battle-Stat-Dodge") + "</color>";
                else
                    return GameManager.instance.GetLocaleString("Battle-Stat-Dodge");
            case EntityStat.Acc:
                return GameManager.instance.GetLocaleString("Battle-Stat-Acc");
            case EntityStat.AdditionalHit:
                return GameManager.instance.GetLocaleString("Battle-Stat-AdditionalHit");
            case EntityStat.Critical:
                return GameManager.instance.GetLocaleString("Battle-Stat-Critical");
            case EntityStat.CriticalDamage:
                return GameManager.instance.GetLocaleString("Battle-Stat-CriticalDamage");
            case EntityStat.Charge:
                return GameManager.instance.GetLocaleString("Battle-Stat-Charge");
            case EntityStat.Leech:
                return GameManager.instance.GetLocaleString("Battle-Stat-Leech");
            case EntityStat.Reflect:
                return GameManager.instance.GetLocaleString("Battle-Stat-Reflect");
            case EntityStat.Resist:
                return GameManager.instance.GetLocaleString("Battle-Stat-Resist");
            case EntityStat.Shild:
                return GameManager.instance.GetLocaleString("Battle-Stat-Shild");
            default:
                return "";
        }
    }

    public static string GetSkillTypeDescription(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.PscDamage:
                return GameManager.instance.GetLocaleString("Battle-SkillDescription-PcsDamage");
            case SkillType.MgcDamage:
                return GameManager.instance.GetLocaleString("Battle-SkillDescription-MgcDamage");
            case SkillType.Heal:
                return GameManager.instance.GetLocaleString("Battle-SkillDescription-Heal");
            case SkillType.Buff:
                return GameManager.instance.GetLocaleString("Battle-SkillDescription-Buff");
            case SkillType.Debuff:
                return GameManager.instance.GetLocaleString("Battle-SkillDescription-Debuff");
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