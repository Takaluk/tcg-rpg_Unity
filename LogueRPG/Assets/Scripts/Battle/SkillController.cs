using System.Collections;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public enum SkillType
{
    PscDamage,
    MgcDamage,
    TrueDamage,
    Dot,
    Heal,
    Buff,
    Debuff,
    DurabilityDamage,
    SelfDamage,
    PermanentBuff
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
        float damage = 0;
        bool criCheck = user.CriticalCheck();

        damage += (user.entityStat[EntityStat.CurrentCharge] + 100f) / 100f * skillEffect.pow / 100f * user.entityStat[skillEffect.skillStatType];
        user.entityStat[EntityStat.CurrentCharge] = 0;
        if (criCheck)
        {
            switch (skillEffect.skillType)
            {
                case SkillType.PscDamage:
                    damage *= 1.5f + user.entityStat[EntityStat.CriticalDamage] / 100f;
                    break;
                default:
                    break;
            }
        }
        float actualDamage = target.TakeDamage(skillEffect.skillType, (int)damage, equipNum, criCheck);

        if (target.entityStat[EntityStat.Reflect] > 0)
        {
            float reflectDamage = actualDamage * target.entityStat[EntityStat.Reflect] / 100f;
            user.TakeDamage(SkillType.TrueDamage, (int)reflectDamage);
            Debug.Log(reflectDamage);
        }

        if (user.entityStat[EntityStat.Leech] > 0 && actualDamage > 0)
        {
            float healAmount = actualDamage * user.entityStat[EntityStat.Leech] / 100f;
            user.TakeHeal((int)healAmount);
        }
    }

    void SelfDamageSkill(SkillEffect skillEffect, Entity user)
    {
        float damage = skillEffect.pow / 100f * user.entityStat[skillEffect.skillStatType];

        user.TakeDamage(skillEffect.skillType, (int)damage);
    }

    void HealSkill(SkillEffect skillEffect, Entity user)
    {
        float heal = 0;
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

    void AttackDurabilitySkill()
    {

    }

    void RecoverDurabilitySkill()
    {

    }

    void PermanentBuffSkill(SkillEffect buffEffect, Entity user)
    {
        string buffPopup = Utils.color_buff + Utils.GetStatName(buffEffect.skillStatType, false);
        user.EntityPopUp(buffPopup);
        user.IncreaseStat(buffEffect.skillStatType, buffEffect.pow);
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

        float accuracyCheck = 100;
        if (TurnManager.instance.currentState == GameState.Battle)
        {
            int targetDodge = target.entityStat[EntityStat.Dodge];
            if (targetDodge < 0)
                targetDodge = 0;
            if (targetDodge > 90)
                targetDodge = 90;

            accuracyCheck = (skill.acc + user.entityStat[EntityStat.Acc]) * (1f - targetDodge / 100f);
            Debug.Log(accuracyCheck);
        }

        if (!Utils.CalculateProbability(accuracyCheck))
        {
            user.manaChargeBlock = false;
            target.EntityPopUp(Utils.color_miss + GameManager.instance.GetLocaleString("Battle-Miss"));
        }
        else
        {
            foreach (SkillEffect skillEffect in skill.skillEffects)
            {
                if (TurnManager.instance.currentState == GameState.Battle)
                    if (user.entityStat[EntityStat.CurrentHP] == 0 || target.entityStat[EntityStat.CurrentHP] == 0)
                    {
                        user.manaChargeBlock = false;
                        break;
                    }

                switch (skillEffect.skillType)
                {
                    case SkillType.PscDamage:
                        yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                        DamageSkill(skillEffect, target, user, equipNum);
                        continue;

                    case SkillType.MgcDamage:
                        yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                        DamageSkill(skillEffect, target, user, equipNum);
                        continue;

                    case SkillType.Heal:
                        yield return new WaitForSeconds(SkillVfxTiming(user, skillEffect.vfx));

                        HealSkill(skillEffect, user);
                        continue;

                    case SkillType.Buff:
                        yield return new WaitForSeconds(SkillVfxTiming(user, skillEffect.vfx));

                        BuffSkill(skillEffect, user);
                        continue;

                    case SkillType.Debuff:
                        yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                        BuffSkill(skillEffect, target);
                        continue;

                    case SkillType.DurabilityDamage:
                        yield return new WaitForSeconds(SkillVfxTiming(target, skillEffect.vfx));

                        //AttackDurabilitySkill(skillEffect, target);
                        continue;

                    case SkillType.SelfDamage:
                        yield return new WaitForSeconds(SkillVfxTiming(user, skillEffect.vfx));

                        SelfDamageSkill(skillEffect, user);
                        continue;

                    case SkillType.PermanentBuff:
                        yield return new WaitForSeconds(SkillVfxTiming(user, skillEffect.vfx));

                        PermanentBuffSkill(skillEffect, user);
                        continue;

                }
            }

            user.manaChargeBlock = false;
        }
        yield return new WaitForSeconds(0.2f);
        GameManager.instance.RemoveControlBlock();
    }
}
