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

        user.apChargeBlock = true;
        if (!Utils.CalculateProbability(skill.acc))
        {
            user.apChargeBlock = false;
            target.EntityPopUp("<color=#606060>MISS");
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
                            user.apChargeBlock = false;
                            break;
                        }

                        yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                        DamageSkill(skillEffect, target, user, equipNum);

                        yield return new WaitForSeconds(0.3f);
                        continue;

                    case SkillType.MgcDamage:
                        if (target.entityStat[EntityStat.CurrentHP] == 0)
                        {
                            user.apChargeBlock = false;
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

            user.apChargeBlock = false;
        }

        GameManager.instance.RemoveControlBlock();
    }
}
