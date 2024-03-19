using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EntityStat
{
    None = 0,
    level,
    maxHP,
    currentHP,
    attack,
    defense,
    maxExp,
    currentExp,
    apCharge
};

[System.Serializable]
public class Entity
{
    public CardOnFeild entityCard = null;
    public bool isPlayer;
    public Transform entityPopUpPosition;
    public Dictionary<EntityStat, int> stat = new Dictionary<EntityStat, int>();
    public List<SkillCard> skillCards;

    public float actionPoint = 0;

    public void SetEntityStat(int level)
    {
        stat.Clear();
        stat.Add(EntityStat.level, level);
        stat.Add(EntityStat.maxHP, 100 + (50 * level));
        stat.Add(EntityStat.currentHP, stat[EntityStat.maxHP]);
        stat.Add(EntityStat.attack, (int)((1 + 0.2 * level) * 10));
        actionPoint = 0;
    }

    public void TakeDamage(int damage)
    {
        EntityController.instance.DamageReaction(this);

        string damageAmount = "<color=red>" + damage.ToString();
        EntityPopUp(damageAmount);

        int afterHp = stat[EntityStat.currentHP] - damage;

        if (afterHp <= 0) 
        {
            afterHp = 0;
            Die();
        }
        stat[EntityStat.currentHP] = afterHp;

        entityCard.UpdateHealthbar(stat[EntityStat.maxHP], afterHp);
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
    public int initialPlayerLevel;
    public int initialEnemyLevel;

    public Entity player;
    public Entity enemy;

    public float apChargeSpeed = 2f;
    public int apChargeBlock = 1;

    public Material hitFlashMaterial;

    private void Start()
    {
        skillController = GetComponent<SkillController>();
        SetPlayerStat();
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

    public void SetPlayerStat()
    {
        CardManager.instance.SetPlayerCard();
        player.SetEntityStat(initialPlayerLevel);
        player.entityCard = CardManager.instance.playerCard;
    }

    public void StartBattle()
    {
        enemy.SetEntityStat(initialEnemyLevel + GameManager.instance.GetTurnCount());
        enemy.entityCard = CardManager.instance.enemyCard;

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
        apChargeBlock--;
        if (apChargeBlock < 0) 
            apChargeBlock = 0;
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
        apChargeBlock += 1;
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
