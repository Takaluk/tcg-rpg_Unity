using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Deal, //물리공격, 마법공격, 보조공격, 펫(크리쳐) 공격 등등
    Dot,
    Heal,
    AttackBuff,
    DefenseBuff,
    AttackDebuff,
    DefenseDebuff
};

[System.Serializable]
public class SkillEffect
{
    public SkillType skillType;
    public EntityStat skillStatType;
    public int pow;
}

public class SkillController : MonoBehaviour
{
    //스킬의 파워 타입, 계수, 명중률, 종류 등을 분석

    public void UseSkill(SkillCard skill, Entity target, Entity user)
    {
        if (skill == null)
            return;

        if (!Utils.CalculateProbability(skill.acc))
        {
            return;
        }

        foreach (SkillEffect skillEffect in skill.skillEffects)
        {
            if (target.stat[EntityStat.currentHP] == 0)
            {
                break;
            }

            if (skillEffect.skillType == SkillType.Deal)
            {
                DamageSkill(skillEffect, target, user);
            }
        }
    }

    void DamageSkill(SkillEffect skillEffect, Entity target, Entity user)
    {
        int damage = skillEffect.pow * user.stat[skillEffect.skillStatType];

        target.TakeDamage(damage);
    }
}
