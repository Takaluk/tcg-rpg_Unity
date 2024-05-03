using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;


public enum EntityStat
{
    None = 0,
    Level,
    MaxHP,
    CurrentHP,
    APChargeSpeed,

    Str,
    Int,
    Def,
    Wis,
    DEX,
    SAN
};

[System.Serializable]
public class Entity
{
    public CardOnFeild entityCard = null;
    public bool isPlayer;
    public Transform entityPopUpPosition;
    public Dictionary<EntityStat, int> entityStat = new Dictionary<EntityStat, int>();
    public List<SkillCard> skillCards;
    public List<EquipmentCard> equipCards;

    public int[] entityEquipLevels = { 1, 1, 1 };
    public int[] currentEquipDurabilities = { 0, 0, 0 };
    public int[] maxEquipDurabilities = { 0, 0, 0 };
    public bool[] isEnchanted = {false, false, false};

    public bool apChargeBlock = false;
    public float actionPoint = 0;

    public void UpdateEntity(CardOnFeild cof)
    {
        entityCard = cof;
        EnemyCard entityData = (EnemyCard)cof.card;

        skillCards.Clear();
        equipCards.Clear();

        foreach(SkillCard skill in entityData.enemySkills)
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
        int enemyPenalty = 100;
        if (!isPlayer)
            enemyPenalty = GameManager.instance.GetEnemyPenalty();

        for (int i = 0; i < 3; i++)
        {
            entityEquipLevels[i] = level;
            maxEquipDurabilities[i] = 0;
        }

        foreach (EntityStat stat in Enum.GetValues(typeof(EntityStat)))
        {
            entityStat[stat] = 0;
        }

        entityStat[EntityStat.Level] = level;
        entityStat[EntityStat.MaxHP] = 100;

        for (int i =0; i< 3; i++)
        {
            foreach(EquipmentStats eStat in equipCards[i].equipStats)
            {
                entityStat[eStat.equipStat] += eStat.basePow * enemyPenalty / 100;
                entityStat[eStat.equipStat] += eStat.PowPL * level * enemyPenalty / 100;
            }
        }

        entityStat[EntityStat.CurrentHP] = entityStat[EntityStat.MaxHP];

        for (int i = 0; i < 3; i++)
        {
            maxEquipDurabilities[i] += entityStat[EntityStat.MaxHP] * equipCards[i].Durability / 100;
            currentEquipDurabilities[i] = maxEquipDurabilities[i];
        }

        actionPoint = 0;
        entityStat[EntityStat.APChargeSpeed] += 45;

        entityCard.UpdateHealthbar(entityStat[EntityStat.MaxHP], entityStat[EntityStat.CurrentHP]);
    }

    public void TakeDamage(SkillType skillType, int damage, int equipNum = 3)
    {
        EntityController.instance.DamageReaction(this);

        string damageAmount = "";

        switch (skillType)
        {
            case (SkillType.PscDamage):
                damageAmount += "<color=red>" + damage.ToString();
                if (equipNum < 3)
                {
                    TakeDurabilityDamage(equipNum, damage);
                }
                break;
            case (SkillType.MgcDamage):
                damageAmount += "<color=#5B40FF>" + damage.ToString();
                break;
        }


        EntityPopUp(damageAmount);

        int afterHp = entityStat[EntityStat.CurrentHP] - damage;

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

        currentEquipDurabilities[equipNum] -= damage;
        EntityController.instance.EquipDamageReaction(this, equipNum);

        if (currentEquipDurabilities[equipNum] <= 0)
        {
            currentEquipDurabilities[equipNum] = 0;
            if (isPlayer)
                CardManager.instance.playerEquip[equipNum].CardPopUp("<color=grey>파괴됨!");
            else
            {
                CardManager.instance.enemyEquip[equipNum].CardPopUp("<color=grey>파괴됨!");
                EntityController.instance.rewardEquips.Add(equipCards[equipNum]);
            }
        }

        if (currentEquipDurabilities[0] == 0 && currentEquipDurabilities[1] == 0 && currentEquipDurabilities[2] == 0)
            Debug.Log("all broken");

        if (isPlayer)
            CardManager.instance.playerEquip[equipNum].UpdateEquipDurability(maxEquipDurabilities[equipNum], currentEquipDurabilities[equipNum]);
        else
            CardManager.instance.enemyEquip[equipNum].UpdateEquipDurability(maxEquipDurabilities[equipNum], currentEquipDurabilities[equipNum]);
    }

    public void TakeHeal(int heal)
    {
        int afterHp = entityStat[EntityStat.CurrentHP] + heal;
        if (afterHp > entityStat[EntityStat.MaxHP])
            entityStat[EntityStat.CurrentHP] = entityStat[EntityStat.MaxHP];
        else
            entityStat[EntityStat.CurrentHP] = afterHp;

        string healAmount = "<color=green>" + heal.ToString();
        EntityPopUp(healAmount);
        entityCard.UpdateHealthbar(entityStat[EntityStat.MaxHP], entityStat[EntityStat.CurrentHP]);
    }

    public void Die()
    {
        EntityController.instance.EntityDied(this);
    }

    public void EntityPopUp(string line)
    {
        entityCard.CardPopUp(line);
    }

    public void EntityApCharge(float apCharge)
    {
        if (apChargeBlock)
            return;

        if (actionPoint < 4f)
            actionPoint += (entityStat[EntityStat.APChargeSpeed] / 60f) * apCharge;
        else
            actionPoint = 4f;

        entityCard.UpdateApGage(actionPoint);
    }

    public void ChangeEquip(EquipmentCard newEquip, int level)
    {
        int equipNum = (int)newEquip.equipType;
        foreach (EquipmentStats eStat in equipCards[equipNum].equipStats)
        {
            entityStat[eStat.equipStat] -= eStat.basePow;
            entityStat[eStat.equipStat] -= eStat.PowPL * entityEquipLevels[equipNum];
        }

        equipCards[equipNum] = newEquip;
        entityEquipLevels[equipNum] = level;

        foreach (EquipmentStats eStat in newEquip.equipStats)
        {
            entityStat[eStat.equipStat] += eStat.basePow;
            entityStat[eStat.equipStat] += eStat.PowPL * level;

            Debug.Log(entityStat[eStat.equipStat]);
        }

        entityCard.UpdateHealthbar(entityStat[EntityStat.MaxHP], entityStat[EntityStat.CurrentHP]);
    }

    public void ChangeSkill(SkillCard skill, int skillNum)
    {
        skillCards[skillNum] = skill;
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

    public GameObject popUpPrefeb;
    public GameObject handBlockSprite;

    public Entity player;
    public Entity enemy;
    public Material hitFlashMaterial;

    public List<EquipmentCard> rewardEquips = new List<EquipmentCard>();

    float apChargeTimer = 0f;
    float apChargeInterval = 0.1f;
    float enemyActionTimer = 0f;
    float enemyActionInterval = 0.5f;

    SkillCard currentEnemySkill = null;

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

            apChargeTimer += Time.deltaTime;

            if (apChargeTimer >= apChargeInterval) 
            {
                player.EntityApCharge(apChargeTimer);
                enemy.EntityApCharge(apChargeTimer);

                apChargeTimer = 0f;
            }

            enemyActionTimer += Time.deltaTime;

            if (currentEnemySkill == null)
            {
                currentEnemySkill = CardManager.instance.ChooseRandomEnemySkill();
            }

            if (enemyActionTimer > enemyActionInterval)
            {
                CardManager.instance.EnemyUseSkill(currentEnemySkill);
                enemyActionTimer = 0f;

                currentEnemySkill = null;
            }
        }
    }

    public void StartBattle()
    {
        enemy.UpdateEntity(CardManager.instance.enemyCard);
        player.isEnchanted[0] = false;
        player.isEnchanted[1] = false;
        player.isEnchanted[2] = false;

        player.apChargeBlock = false;
        enemy.apChargeBlock = false;

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

        if (entity.actionPoint >= skill.cost)
        {
            entity.isEnchanted[equipNum] = true;

            entity.actionPoint -= skill.cost;
            entity.entityCard.UpdateApGage(entity.actionPoint);
            if (isPlayer)
                skillController.UseSkill(skill, enemy, player, equipNum);
            else
                skillController.UseSkill(skill, player, enemy, equipNum);
        }
        else
        {
            if (entity.isPlayer)
                entity.EntityPopUp("<color=#1E90FF>Not enough AP");
            return false;
        }

        return true;
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

    public void EntityDied(Entity entity)
    {
        if (entity == enemy)
        {
            StartCoroutine(EndBattle());
        }
    }

    public void DamageReaction(Entity entity)
    {
        StartCoroutine(DamageReactionEffect(entity));
    }

    public void EquipDamageReaction(Entity entity, int equipNum)
    {
        if (entity.isPlayer)
            StartCoroutine(EquipDamageReactionEffect(CardManager.instance.playerEquip[equipNum]));
        else
            StartCoroutine(EquipDamageReactionEffect(CardManager.instance.enemyEquip[equipNum]));
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

        //button on -> button function (넘기기, 교체)
    }

    IEnumerator DamageReactionEffect(Entity entity)
    {
        entity.entityCard.characterSprite.transform.DOShakePosition(0.1f);
        Material org = entity.entityCard.characterSprite.material;
        entity.entityCard.characterSprite.material = hitFlashMaterial;
        yield return new WaitForSeconds(0.1f);
        entity.entityCard.characterSprite.material = org;
    }

    IEnumerator EquipDamageReactionEffect(CardOnFeild equipCard)
    {
        equipCard.transform.DOShakePosition(0.1f);
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator EndBattle()
    {
        GameManager.instance.AddControlBlock();

        yield return new WaitForSeconds(0.3f);
        enemy.EntityPopUp("<color=#606060>Died");
        yield return new WaitForSeconds(0.3f);
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
