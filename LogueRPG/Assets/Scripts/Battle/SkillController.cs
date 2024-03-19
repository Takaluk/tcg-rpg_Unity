using System.Collections;
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
    public void UseSkill(SkillCard skill, Entity target, Entity user)
    {
        if (skill == null)
            return;

        if (!Utils.CalculateProbability(skill.acc))
        {
            target.EntityPopUp("<color=#606060>MISS");
            return;
        }

        StartCoroutine(ProcessSkillEffects(skill, target, user));
    }

    void DamageSkill(SkillEffect skillEffect, Entity target, Entity user)
    {
        int damage = skillEffect.pow * user.stat[skillEffect.skillStatType];

        target.TakeDamage(damage);
    }

    void HealSkill(SkillEffect skillEffect, Entity user)
    {
        int heal = skillEffect.pow * user.stat[skillEffect.skillStatType] - 352;

        user.TakeHeal(heal);
    }

    IEnumerator ProcessSkillEffects(SkillCard skill, Entity target, Entity user)
    {
        foreach (SkillEffect skillEffect in skill.skillEffects)
        {
            if (skillEffect.skillType == SkillType.Deal)
            {
                if (target.stat[EntityStat.currentHP] == 0)
                {
                    break;
                }
                DamageSkill(skillEffect, target, user);
            }
            else if (skillEffect.skillType == SkillType.Heal)
                HealSkill(skillEffect, user);


            yield return new WaitForSeconds(0.3f);
        }
    }
}
