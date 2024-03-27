using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EntityStat
{
    None = 0,
    Level,
    MaxHP,
    CurrentHP,
    APCharge,

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

    public int weaponLevel = 1;
    public int armorLevel = 1;
    public int artifactLevel = 1;

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

        SetEntityStat(GameManager.instance.GetTurnCount()/2);

        entityCard.UpdateHealthbar(entityStat[EntityStat.MaxHP], entityStat[EntityStat.CurrentHP]);
    }

    public void SetEntityStat(int level)
    {
        foreach (EntityStat stat in Enum.GetValues(typeof(EntityStat)))
        {
            entityStat[stat] = 0;
        }

        entityStat[EntityStat.MaxHP] = 100;

        foreach(EquipmentCard equip in equipCards)
        {
            foreach(EquipmentStats eStat in equip.equipStats)
            {
                entityStat[eStat.equipStat] += eStat.basePow;
                entityStat[eStat.equipStat] += eStat.PowPL * level;
            }
        }

        entityStat[EntityStat.CurrentHP] = entityStat[EntityStat.MaxHP];

        actionPoint = 0;
    }

    public void TakeDamage(int damage)
    {
        EntityController.instance.DamageReaction(this);

        string damageAmount = "<color=red>" + damage.ToString();
        EntityPopUp(damageAmount);

        int afterHp = entityStat[EntityStat.CurrentHP] - damage;

        if (afterHp <= 0) 
        {
            afterHp = 0;
            Die();
        }
        entityStat[EntityStat.CurrentHP] = afterHp;

        entityCard.UpdateHealthbar(entityStat[EntityStat.MaxHP], afterHp);
    }

    public void TakeHeal(int heal)
    {
        string healAmount = "<color=green>" + heal.ToString();
        EntityPopUp(healAmount);
    }

    public void Die()
    {
        EntityController.instance.EntityDied(this);
    }

    public void EntityPopUp(string line)
    {
        EntityController.instance.EntityPopUp(line, entityPopUpPosition);
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

    public Entity player;
    public Entity enemy;

    public float apChargeSpeed = 2f;
    public int apChargeBlock = 1;

    public Material hitFlashMaterial;

    private void Start()
    {
        skillController = GetComponent<SkillController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            player.TakeDamage(100);
        }

        if (apChargeBlock == 0)
        {
            if (player.actionPoint < 4)
                player.actionPoint += apChargeSpeed * Time.deltaTime;
            else
                player.actionPoint = 4f;

            player.entityCard.UpdateApGage(player.actionPoint);
        }
    }

    public void StartBattle()
    {
        enemy.UpdateEntity(CardManager.instance.enemyCard);

        apChargeBlock = 0;
    }

    public void PlayerUseSkill(SkillCard skill)
    {
        if (player.actionPoint >= skill.cost)
        {
            player.actionPoint -= skill.cost;
            player.entityCard.UpdateApGage(player.actionPoint);
            skillController.UseSkill(skill, enemy, player);
        }
        else
            player.EntityPopUp("<color=blue>Not enough AP");
    }

    public void EntityPopUp(string line, Transform popUpPosition )
    {
        var popUp = Instantiate(popUpPrefeb, popUpPosition);
        TMP_Text popUpTmp = popUp.GetComponent<TextMeshPro>();
        popUpTmp.text = line;
        Vector3 dest = new Vector3(popUpPosition.position.x, popUpPosition.position.y + 1f, popUpPosition.position.z);
        popUp.gameObject.transform.DOMove(dest, 1f)
            .OnComplete(() => Destroy(popUp));
    }

    public void AddApChargeBlock()
    {
        apChargeBlock++;
    }

    public void RemoveApChargeBlock()
    {
        if (TurnManager.instance.currentState == GameState.Battle)
        {
            apChargeBlock--;
            if (apChargeBlock < 0)
                apChargeBlock = 0;
        }
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

    IEnumerator DamageReactionEffect(Entity entity)
    {
        entity.entityCard.characterSprite.transform.DOShakePosition(0.1f);
        Material org = entity.entityCard.characterSprite.material;
        entity.entityCard.characterSprite.material = hitFlashMaterial;
        yield return new WaitForSeconds(0.1f);
        entity.entityCard.characterSprite.material = org;
    }

    IEnumerator EndBattle()
    {
        apChargeBlock += 5;
        yield return new WaitForSeconds(0.3f);
        enemy.EntityPopUp("<color=#606060>Died");
        yield return new WaitForSeconds(0.3f);
        TurnManager.instance.ChangeTurnTo(GameState.Event);
        CardManager.instance.main[0].NextDialogue();
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(0.2f);
    }
}
