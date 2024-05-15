using System.Collections;
using UnityEngine;

public enum SkillType
{
    PscDamage,
    MgcDamage,
    TrueDamage,
    Dot,
    Heal,
    Buff,
    Debuff
};

public class SkillController : MonoBehaviour
{
    public void UseSkill(SkillCard skill, Entity target, Entity user, int equipNum)
    {
        if (skill == null)
            return;

        StartCoroutine(ProcessSkillEffects(skill, target, user, equipNum));
    }

    void DamageSkill(SkillEffect skillEffect, Entity target, Entity user, int equipNum)
    {
        float damage = 1;
        damage += skillEffect.pow / 100f * user.entityStat[skillEffect.skillStatType];

        target.TakeDamage(skillEffect.skillType, (int)damage, equipNum);
    }

    void HealSkill(SkillEffect skillEffect, Entity user)
    {
        float heal = 1;
        if (skillEffect.skillStatType == EntityStat.MaxHP)
        {
            heal += skillEffect.pow / 100f * user.entityStat[skillEffect.skillStatType];
        }

        user.TakeHeal((int)heal);
    }

    void BuffSkill(SkillEffect skillEffect, Entity target)
    {
        EntityController.instance.AddBuff(skillEffect, target);
    }

    float SkillVfxTiming(Entity target, GameObject skillPrefeb)
    {
        if (skillPrefeb == null)
            return 0;
        var vfx = Instantiate(skillPrefeb, target.entityCard.transform);
        VfxData vData = vfx.GetComponent<VfxData>();
        return vData.effectTiming;
    }

    IEnumerator ProcessSkillEffects(SkillCard skill, Entity target, Entity user, int equipNum)
    {
        GameManager.instance.AddControlBlock();
        yield return new WaitForSeconds(0.2f);

        user.manaChargeBlock = true;
        float accuracyCheck = (skill.acc + user.entityStat[EntityStat.Acc]) * (1 - user.entityStat[EntityStat.Dodge] / 100);
        Debug.Log(accuracyCheck);
        if (!Utils.CalculateProbability(accuracyCheck))
        {
            user.manaChargeBlock = false;
            target.EntityPopUp(Utils.color_miss + "MISS");
        }
        else
        {
            foreach (SkillEffect skillEffect in skill.skillEffects)
            {
                switch (skillEffect.skillType)
                {
                    case SkillType.PscDamage:
                        if (target.entityStat[EntityStat.CurrentHP] == 0)
                        {
                            user.manaChargeBlock = false;
                            break;
                        }

                        yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                        DamageSkill(skillEffect, target, user, equipNum);

                        yield return new WaitForSeconds(0.3f);
                        continue;

                    case SkillType.MgcDamage:
                        if (target.entityStat[EntityStat.CurrentHP] == 0)
                        {
                            user.manaChargeBlock = false;
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

                    case SkillType.Buff:
                        yield return new WaitForSeconds(SkillVfxTiming(user, skillEffect.vfx));

                        BuffSkill(skillEffect, user);

                        yield return new WaitForSeconds(0.3f);
                        continue;

                    case SkillType.Debuff:
                        yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                        BuffSkill(skillEffect, target);

                        yield return new WaitForSeconds(0.3f);
                        continue;
                }
            }

            user.manaChargeBlock = false;
        }

        GameManager.instance.RemoveControlBlock();
    }
}
