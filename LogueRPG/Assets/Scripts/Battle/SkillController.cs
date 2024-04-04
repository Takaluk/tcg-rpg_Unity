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
        float statBonusDamage = skillEffect.pow / 100f * user.entityStat[skillEffect.skillStatType];
        int damage = 1 + (int)statBonusDamage;

        target.TakeDamage(damage);
    }

    void HealSkill(SkillEffect skillEffect, Entity user)
    {
        int heal = 1 + skillEffect.pow;

        user.TakeHeal(heal);
    }

    float SkillVfxTiming(Entity target, GameObject skillPrefeb)
    {
        var vfx = Instantiate(skillPrefeb, target.entityCard.transform);
        VfxData vData = vfx.GetComponent<VfxData>();
        return vData.effectTiming;
    }

    IEnumerator ProcessSkillEffects(SkillCard skill, Entity target, Entity user)
    {
        GameManager.instance.AddControlBlock();
        yield return new WaitForSeconds(0.1f);

        foreach (SkillEffect skillEffect in skill.skillEffects)
        {
            switch (skillEffect.skillType)
            {
                case SkillType.Deal:
                    if (target.entityStat[EntityStat.CurrentHP] == 0)
                    {
                        break;
                    }

                    yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                    DamageSkill(skillEffect, target, user);

                    yield return new WaitForSeconds(0.3f);
                    continue;

                case SkillType.Heal:

                    yield return new WaitForSeconds(SkillVfxTiming(user, skillEffect.vfx));

                    HealSkill(skillEffect, user);

                    yield return new WaitForSeconds(0.3f);
                    continue;
            }
        }

        GameManager.instance.RemoveControlBlock();
    }
}
