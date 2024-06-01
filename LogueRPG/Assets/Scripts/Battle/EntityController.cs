using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;


public enum EntityStat
{
    None = 0,
    MaxHP,//800~500 / 130~60
    Shild,
    Resist,
    CurrentHP,
    MPChargeSpeed,//20 ~ 40

    Str,//70~40 / 5~2
    Int,//70~40 / 5~2
    PDef,//47~18 / 6~3
    MDef,//47~18 / 6~3
    Acc,//~1000
    Dodge,//~90
    Critical,//~100
    CriticalDamage,
    Leech,
    Reflect,
    Charge,
    AdditionalHit
};

[System.Serializable]
public class Entity
{
    public CardOnFeild entityCard = null;
    public bool isPlayer;
    public bool isOnReaction;
    public Transform entityPopUpPosition;
    public Dictionary<EntityStat, int> entityStat = new Dictionary<EntityStat, int>();
    public List<SkillCard> skillCards;
    public List<EquipmentCard> equipCards;

    public int[] entityEquipLevels = { 1, 1, 1 };
    public int[] currentEquipDurabilities = { 0, 0, 0 };
    public int[] maxEquipDurabilities = { 0, 0, 0 };
    public bool[] isEnchanted = { false, false, false };

    public bool manaChargeBlock = false;
    public float mana = 0;

    public List<Buff> buffList = new List<Buff>();
    public List<Buff> debuffList = new List<Buff>();
    public Transform buffTransform;
    public Transform debuffTransform;

    public void UpdateEntity(CardOnFeild cof)
    {
        entityCard = cof;
        buffTransform = cof.buffTrnasform;
        debuffTransform = cof.debuffTransform;
        EnemyCard entityData = (EnemyCard)cof.card;

        skillCards.Clear();
        equipCards.Clear();

        foreach (SkillCard skill in entityData.enemySkills)
        {
            skillCards.Add(skill);
        }

        foreach (EquipmentCard equip in entityData.equipmentCards)
        {
            equipCards.Add(equip);
        }

        SetEntityStat(GameManager.instance.GetEnemyLevel());
    }

    private void SetEntityStat(int level)
    {
        for (int i = 0; i < 3; i++)
        {
            maxEquipDurabilities[i] = 0;
        }

        foreach (EntityStat stat in Enum.GetValues(typeof(EntityStat)))
        {
            entityStat[stat] = 0;
        }

        IncreaseStat(EntityStat.MaxHP, 100);

        for (int i = 0; i < 3; i++)
        {
            entityEquipLevels[i] = level;
            foreach (EquipmentStats eStat in equipCards[i].equipStats)
            {
                float baseStat = eStat.basePow;
                IncreaseStat(eStat.equipStat, (int)baseStat);
                float powPLStat = eStat.PowPL * level;
                IncreaseStat(eStat.equipStat, (int)powPLStat);

                if (!isPlayer)
                {
                    if (entityCard.card.weight <= 30)
                        IncreaseStat(eStat.equipStat, (int)(powPLStat / 10f));
                    if (entityCard.card.weight <= 10)
                        IncreaseStat(eStat.equipStat, (int)(powPLStat / 10f));
                    if (entityCard.card.weight <= 5)
                        IncreaseStat(eStat.equipStat, (int)(powPLStat / 10f));
                }
            }
        }

        if (!isPlayer)
        {
            int paneltyHP = 0;
            if (entityCard.card.weight > 10)
                paneltyHP += 10;
            if (entityCard.card.weight > 20)
                paneltyHP += 10;
            if (entityCard.card.weight > 30)
                paneltyHP += 30;
            float paneltyStat = entityStat[EntityStat.MaxHP] * (paneltyHP / 100f);
            DecreaseStat(EntityStat.MaxHP, (int)paneltyStat);
            Debug.Log(paneltyStat);
        }

        for (int i = 0; i < 3; i++)
        {
            float durability = 2 * entityStat[EntityStat.MaxHP] * equipCards[i].Durability / 100f;
            maxEquipDurabilities[i] += (int)durability;
            currentEquipDurabilities[i] = maxEquipDurabilities[i];
        }

        mana = 0;
        entityStat[EntityStat.CurrentHP] = entityStat[EntityStat.MaxHP];
        entityCard.UpdateHealthbar(entityStat[EntityStat.MaxHP], entityStat[EntityStat.CurrentHP]);
    }


    public void TakeDamage(SkillType skillType, int damage, int equipNum = 3, bool isCritical = false)
    {
        EntityController.instance.DamageReaction(this);

        string damageAmount = "";
        float actualDamage = damage;

        if (damage < 0)
        {
            actualDamage = 0;
            damageAmount = "0";
        }
        else
            switch (skillType)
            {
                case (SkillType.PscDamage):
                    int pDef = entityStat[EntityStat.PDef];

                    if (pDef > 0)
                    {
                        actualDamage = actualDamage / (1f + pDef / 100f);
                    }
                    else if (pDef < 0)
                    {
                        actualDamage = damage * (2f - 1f / (1f - pDef / 100f));
                    }

                    if (isCritical)
                    {
                        damageAmount += Utils.color_pcsCriticalDamage + "◈" + ((int)actualDamage).ToString();
                    }
                    else
                        damageAmount += Utils.color_pcsDamage + ((int)actualDamage).ToString();

                    if (equipNum < 3)
                    {
                        TakeDurabilityDamage(equipNum, (int)actualDamage);
                    }
                    break;

                case (SkillType.MgcDamage):
                    int mDef = entityStat[EntityStat.MDef];

                    if (mDef > 0)
                    {
                        actualDamage = damage / (1f + mDef / 100f);
                    }
                    else if (mDef < 0)
                    {
                        actualDamage = damage * (2f - 1f / (1f - mDef / 100f));
                    }
                    damageAmount += Utils.color_mgcDamage + ((int)actualDamage).ToString();
                    break;
            }


        EntityPopUp(damageAmount);

        int afterHp = entityStat[EntityStat.CurrentHP] - (int)actualDamage;

        if (afterHp <= 0)
        {
            afterHp = 0;
            Die();
        }
        entityStat[EntityStat.CurrentHP] = afterHp;

        entityCard.UpdateHealthbar(entityStat[EntityStat.MaxHP], entityStat[EntityStat.CurrentHP]);
    }

    public void TakeDurabilityDamage(int equipNum, int damage)
    {
        if (currentEquipDurabilities[equipNum] == 0)
            return;
        damage += GameManager.instance.GetEnemyLevel() * 50;
        currentEquipDurabilities[equipNum] -= damage;

        if (currentEquipDurabilities[equipNum] <= 0)
        {
            currentEquipDurabilities[equipNum] = 0;
            if (isPlayer)
            {
                CardManager.instance.playerEquip[equipNum].CardBreakEffect(true);
                CardManager.instance.playerEquip[equipNum].CardPopUp(Utils.color_broken + GameManager.instance.GetLocaleString("Battle-EquipBroken"));
            }
            else
            {
                CardManager.instance.enemyEquip[equipNum].CardBreakEffect(true);
                CardManager.instance.enemyEquip[equipNum].CardPopUp(Utils.color_broken + GameManager.instance.GetLocaleString("Battle-EquipBroken"));
                EntityController.instance.rewardEquips.Add(equipCards[equipNum]);
            }
            EntityController.instance.EquipPanelty(this, equipNum);
        }

        if (isPlayer)
            CardManager.instance.playerEquip[equipNum].UpdateEquipDurability(maxEquipDurabilities[equipNum], currentEquipDurabilities[equipNum]);
        else
            CardManager.instance.enemyEquip[equipNum].UpdateEquipDurability(maxEquipDurabilities[equipNum], currentEquipDurabilities[equipNum]);
    }

    public bool CriticalCheck()
    {
        return Utils.CalculateProbability(entityStat[EntityStat.Critical]);
    }

    public void TakeHeal(int heal)
    {
        if (heal < 0)
            EntityPopUp(Utils.color_heal + "0");
        else
        {
            int afterHp = entityStat[EntityStat.CurrentHP] + heal;
            if (afterHp > entityStat[EntityStat.MaxHP])
                entityStat[EntityStat.CurrentHP] = entityStat[EntityStat.MaxHP];
            else
                entityStat[EntityStat.CurrentHP] = afterHp;

            string healAmount = Utils.color_heal + heal.ToString();
            EntityPopUp(healAmount);
            entityCard.UpdateHealthbar(entityStat[EntityStat.MaxHP], entityStat[EntityStat.CurrentHP]);
        }
    }

    public void GetStunned(int seconds)
    {

    }

    public void Die()
    {
        buffList.Clear();
        debuffList.Clear();

        EntityController.instance.EntityDied(this);
    }

    public void EntityPopUp(string line)
    {
        entityCard.CardPopUp(line);
    }

    public void EntityManaCharge(float manaCharge)
    {
        if (manaChargeBlock || entityStat[EntityStat.MPChargeSpeed] < 0)
            return;

        if (mana < 4f)
            mana += (entityStat[EntityStat.MPChargeSpeed] / 60f) * manaCharge;
        else
            mana = 4f;

        entityCard.UpdateManaGage(mana);
    }

    public void ChangeEquip(EquipmentCard newEquip, int level)
    {
        int equipNum = (int)newEquip.equipType;
        foreach (EquipmentStats eStat in equipCards[equipNum].equipStats)
        {
            DecreaseStat(eStat.equipStat, eStat.basePow);
            DecreaseStat(eStat.equipStat, eStat.PowPL * entityEquipLevels[equipNum]);
        }

        equipCards[equipNum] = newEquip;
        entityEquipLevels[equipNum] = level;

        foreach (EquipmentStats eStat in newEquip.equipStats)
        {
            IncreaseStat(eStat.equipStat, eStat.basePow);
            IncreaseStat(eStat.equipStat, eStat.PowPL * level);
        }

        if (entityStat[EntityStat.CurrentHP] > entityStat[EntityStat.MaxHP])
            entityStat[EntityStat.CurrentHP] = entityStat[EntityStat.MaxHP];
        entityCard.UpdateHealthbar(entityStat[EntityStat.MaxHP], entityStat[EntityStat.CurrentHP]);
    }

    public void IncreaseStat(EntityStat statType, int pow)
    {
        entityStat[statType] += pow;

        switch (statType)
        {
            case EntityStat.Critical:
                if (entityStat[EntityStat.Critical] > 100)
                    entityStat[EntityStat.Critical] = 100;
                return;
            case EntityStat.Dodge:
                if (entityStat[EntityStat.Dodge] > 90)
                    entityStat[EntityStat.Dodge] = 90;
                return;
        }
    }

    public void DecreaseStat(EntityStat statType, int pow)
    {
        entityStat[statType] -= pow;
    }

    public void ChangeSkill(SkillCard skill, int skillNum)
    {
        skillCards[skillNum] = skill;
    }

    public void AddBuff(Buff buff)
    {
        buffList.Add(buff);
    }

    public void RemoveBuff(Buff buff)
    {
        buffList.Remove(buff);
    }

    public void AddDebuff(Buff buff)
    {
        debuffList.Add(buff);
    }

    public void RemoveDebuff(Buff buff)
    {
        debuffList.Remove(buff);
    }

    public void StartAllBuffTimer()
    {
        foreach (Buff buff in buffList)
        {
            buff.ActivateBuff();
        }

        foreach (Buff buff in debuffList)
        {
            buff.ActivateBuff();
        }
    }

    public void StopAllBuffTimer()
    {
        foreach (Buff buff in buffList)
        {
            buff.PauseBuffTimer();
        }

        foreach (Buff buff in debuffList)
        {
            buff.PauseBuffTimer();
        }
    }
}

public class EntityController : MonoBehaviour
{
    #region Instance
    private static EntityController m_instance;
    public static EntityController instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<EntityController>();
            }

            return m_instance;
        }
    }
    #endregion
    SkillController skillController = null;
    public GameObject handBlockSprite;

    public Entity player;
    public Entity enemy;

    public GameObject buffPrefab;

    public Material hitFlashMaterial;

    public List<EquipmentCard> rewardEquips = new List<EquipmentCard>();

    float manaChargeTimer = 0f;
    float manaChargeInterval = 0.1f;
    float enemyActionTimer = 0f;
    float enemyActionInterval = 0.1f;

    CardOnFeild currentEnemySkillCard = null;

    [SerializeField] SkillEffect[] equipBrokenPanelties;

    private void Start()
    {
        skillController = GetComponent<SkillController>();
    }

    private void Update()
    {
        if (TurnManager.instance.currentState == GameState.Battle)
        {
            if (GameManager.instance.GetControlBlockCount() > 0)
                handBlockSprite.SetActive(true);
            else
                handBlockSprite.SetActive(false);

            manaChargeTimer += Time.deltaTime;

            if (manaChargeTimer >= manaChargeInterval) 
            {
                player.EntityManaCharge(manaChargeTimer);
                enemy.EntityManaCharge(manaChargeTimer);

                manaChargeTimer = 0f;
            }

            enemyActionTimer += Time.deltaTime;

            if (enemy.entityStat[EntityStat.CurrentHP] > 0 && enemyActionTimer > enemyActionInterval)
            {
                if (currentEnemySkillCard == null)
                {
                    currentEnemySkillCard = CardManager.instance.GetRandomEnemySkillCard();
                }
                else
                {
                    SkillCard skill = (SkillCard)currentEnemySkillCard.card;

                    if (enemy.mana >= skill.cost)
                    {
                        int equipNum = GetEmptySlot(enemy);
                        if (UseSkill(skill, equipNum, false))
                        {
                            CardManager.instance.SetSkillCard(currentEnemySkillCard, equipNum, false);

                            currentEnemySkillCard = null;

                        }
                    }
                }

                enemyActionTimer = 0f;
            }
        }
    }

    public void StartBattle()
    {
        enemy.UpdateEntity(CardManager.instance.enemyCard);
        player.isEnchanted[0] = false;
        player.isEnchanted[1] = false;
        player.isEnchanted[2] = false;
        enemy.isEnchanted[0] = false;
        enemy.isEnchanted[1] = false;
        enemy.isEnchanted[2] = false;
        player.manaChargeBlock = false;
        enemy.manaChargeBlock = false;
        currentEnemySkillCard = null;

        player.StartAllBuffTimer();

        rewardEquips.Clear();
    }

    public bool UseSkill(SkillCard skill, int equipNum, bool isPlayer)
    {
        Entity entity;
        if (isPlayer)
            entity = player;
        else
            entity = enemy;

        if (equipNum < 3)
        {
            if (entity.isEnchanted[equipNum])
                return false;
        }
        else if (TurnManager.instance.currentState == GameState.Event)
        {
            skillController.UseSkill(skill, enemy, player, equipNum);
            return true;
        }
        else
            return false;
        //equipmentList에서 스킬 발동

        if (entity.mana >= skill.cost)
        {
            entity.isEnchanted[equipNum] = true;

            entity.mana -= skill.cost;
            entity.entityCard.UpdateManaGage(entity.mana);
            if (isPlayer)
                skillController.UseSkill(skill, enemy, player, equipNum);
            else
                skillController.UseSkill(skill, player, enemy, equipNum);
        }
        else
        {
            if (entity.isPlayer)
                entity.EntityPopUp(Utils.color_miss + GameManager.instance.GetLocaleString("Battle-NotEnoughMana"));
            return false;
        }

        return true;
    }

    public void AddBuff(SkillEffect buffEffect, Entity target)
    {
        Transform iconTarget = target.buffTransform;

        if (buffEffect.skillType == SkillType.Debuff)
        {
            iconTarget = target.debuffTransform;
        }

        GameObject buffObject = Instantiate(buffPrefab, iconTarget);
        Buff buff = buffObject.GetComponent<Buff>();
        buff.InIt(buffEffect, target);

        if (TurnManager.instance.currentState == GameState.Battle)
            buff.ActivateBuff();

        string buffPopup = "";

        if (buffEffect.skillType == SkillType.Buff)
            buffPopup += Utils.color_buff + Utils.GetStatName(buffEffect.skillStatType, false);
        else
            buffPopup += Utils.color_debuff + Utils.GetStatName(buffEffect.skillStatType, false);

        target.EntityPopUp(buffPopup);
    }

    public int GetEmptySlot(Entity entity)
    {
        List<int> emptySlots = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            if (!entity.isEnchanted[i])
                emptySlots.Add(i);
        }

        if (emptySlots.Count > 0)
        {
            return UnityEngine.Random.Range(0, emptySlots.Count);
        }
        else
            return 3;
    }

    public void EquipPanelty(Entity entity, int equipNum)
    {
        AddBuff(equipBrokenPanelties[equipNum], entity);
    }

    public void EntityDied(Entity entity)
    {
        if (entity == player)
        {
            GameManager.instance.ShowGameOver();
            return;
        }
        StartCoroutine(EndBattle());
    }

    public void DamageReaction(Entity entity)
    {
        if (!entity.isOnReaction)
            StartCoroutine(DamageReactionEffect(entity));
    }


    public void BattleReward()
    {
        EquipmentCard rewardEquip;

        if (rewardEquips.Count > 0)
        {
            rewardEquip = rewardEquips[UnityEngine.Random.Range(0, rewardEquips.Count)];
        }
        else
            rewardEquip = enemy.equipCards[UnityEngine.Random.Range(0, 3)];

        SkillCard rewardSkill = enemy.skillCards[UnityEngine.Random.Range(0, enemy.skillCards.Count)];

        CardManager.instance.SetRewardCards(rewardEquip, rewardSkill);
    }

    IEnumerator DamageReactionEffect(Entity entity)
    {
        entity.isOnReaction = true;
        entity.entityCard.characterSprite.transform.DOShakePosition(0.1f);
        Material org = entity.entityCard.characterSprite.material;
        entity.entityCard.characterSprite.material = hitFlashMaterial;
        yield return new WaitForSeconds(0.1f);
        entity.entityCard.characterSprite.material = org;

        entity.isOnReaction = false;
    }

    IEnumerator EndBattle()
    {
        GameManager.instance.AddControlBlock();
        yield return new WaitForSeconds(0.3f);
        player.manaChargeBlock = true;
        enemy.manaChargeBlock = true;
        player.StopAllBuffTimer();
        enemy.StopAllBuffTimer();
        enemy.EntityPopUp(GameManager.instance.GetLocaleString("Battle-Die"));
        yield return new WaitForSeconds(1f);
        TurnManager.instance.ChangeTurnTo(GameState.Event);
        enemy.entityCard.NextDialogue();
        handBlockSprite.SetActive(false);
        GameManager.instance.RemoveControlBlock();
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(0.2f);
    }
}
