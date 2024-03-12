using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Deal, //��������, ��������, ��������, ��(ũ����) ���� ���
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
    //��ų�� �Ŀ� Ÿ��, ���, ���߷�, ���� ���� �м�

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
