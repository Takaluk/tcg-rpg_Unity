using System.Collections;
using UnityEngine;

public enum SkillType
{
    PscDamage,
    MgcDamage,
    Dot,
    Heal,
    AttackBuff,
    DefenseBuff,
    AttackDebuff,
    DefenseDebuff
};

public class SkillController : MonoBehaviour
{
    public void UseSkill(SkillCard skill, Entity target, Entity user, int equipNum)
    {
        if (skill == null)
            return;

        if (!Utils.CalculateProbability(skill.acc))
        {
            target.EntityPopUp("<color=#606060>MISS");
            return;
        }

        StartCoroutine(ProcessSkillEffects(skill, target, user, equipNum));
    }

    void DamageSkill(SkillEffect skillEffect, Entity target, Entity user, int equipNum)
    {
        int damage = 1;
        damage += skillEffect.pow / 100 * user.entityStat[skillEffect.skillStatType];

        target.TakeDamage(skillEffect.skillType, damage, equipNum);
    }

    void HealSkill(SkillEffect skillEffect, Entity user)
    {
        int heal = 1;
        if (skillEffect.skillStatType == EntityStat.MaxHP)
        {
            heal += user.entityStat[skillEffect.skillStatType] * skillEffect.pow / 100;
        }

        user.TakeHeal(heal);
    }

    float SkillVfxTiming(Entity target, GameObject skillPrefeb)
    {
        var vfx = Instantiate(skillPrefeb, target.entityCard.transform);
        VfxData vData = vfx.GetComponent<VfxData>();
        return vData.effectTiming;
    }

    IEnumerator ProcessSkillEffects(SkillCard skill, Entity target, Entity user, int equipNum)
    {
        GameManager.instance.AddControlBlock();
        yield return new WaitForSeconds(0.2f);

        foreach (SkillEffect skillEffect in skill.skillEffects)
        {
            switch (skillEffect.skillType)
            {
                case SkillType.PscDamage:
                    if (target.entityStat[EntityStat.CurrentHP] == 0)
                    {
                        break;
                    }

                    yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                    DamageSkill(skillEffect, target, user, equipNum);

                    yield return new WaitForSeconds(0.3f);
                    continue;

                case SkillType.MgcDamage:
                    if (target.entityStat[EntityStat.CurrentHP] == 0)
                    {
                        break;
                    }

                    yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                    DamageSkill(skillEffect, target, user, equipNum);

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
