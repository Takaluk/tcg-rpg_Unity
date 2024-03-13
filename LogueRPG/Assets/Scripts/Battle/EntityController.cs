using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public enum EntityStat
{
    None = 0,
    level,
    maxHP,
    currentHP,
    attack,
    defense,
    maxExp,
    currentExp
};

[System.Serializable]
public class Entity
{
    public bool isPlayer;
    public string entityName;
    public Dictionary<EntityStat, int> stat = new Dictionary<EntityStat, int>();
    public List<SkillCard> skillCards;

    public void SetEntityStat(int level)
    {
        entityName = "defaultName";
        if (isPlayer)
        {
            entityName = "player";
        }

        stat.Add(EntityStat.level, level);
        stat.Add(EntityStat.maxHP, 100 + (50 * level));
        stat.Add(EntityStat.currentHP, stat[EntityStat.maxHP]);
        stat.Add(EntityStat.attack, (int)((1 + 0.2 * level) * 10));
    }

    public void TakeDamage(int damage)
    {
        string damageAmount = "<color=red><b>" + damage.ToString() + "<b>";
        if (isPlayer)
        {
            EntityController.instance.PlayerPopUp(damageAmount);
        }
        else
            EntityController.instance.EnemyPopUp(damageAmount);

        int afterHp = stat[EntityStat.currentHP] - damage;

        if (afterHp <= 0) 
        {
            Die();
        }
        else
        {
            stat[EntityStat.currentHP] = afterHp;
        }
    }

    public void TakeHeal(int heal)
    {
        string healAmount = "<color=green><b>" + heal.ToString() + "<b>";

        if (isPlayer)
        {
            EntityController.instance.PlayerPopUp(healAmount);
        }
        else
            EntityController.instance.EnemyPopUp(healAmount);
    }

    public void FullRecover()
    {
        stat[EntityStat.currentHP] = stat[EntityStat.maxHP];
    }

    public void Die()
    {
        stat[EntityStat.currentHP] = 0;

        if (isPlayer)
        {
            Debug.Log("player died");
            return;
        }
        else
        {
            TurnManager.instance.ChangeTurnTo(GameState.Event);
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

    public GameObject popUpPrefeb;
    public int initialPlayerLevel;
    public int initialEnemyLevel;

    public Entity player;
    public Entity enemy;

    public Transform playerPopUpPosition;
    public Transform enemyPopUpPosition;

    private void Start()
    {
        skillController = GetComponent<SkillController>();
        player.SetEntityStat(initialPlayerLevel);
        enemy.SetEntityStat(initialEnemyLevel);
    }

    public void PlayerUseSkill(SkillCard skill)
    {
        skillController.UseSkill(skill, enemy, player);
    }

    public void PlayerPopUp(string line)
    {
        var popUp = Instantiate(popUpPrefeb, playerPopUpPosition);
        TMP_Text popUpTmp = popUp.GetComponent<TextMeshPro>();
        popUpTmp.text = line;
        Vector3 dest = new Vector3(playerPopUpPosition.position.x, playerPopUpPosition.position.y + 2f, playerPopUpPosition.position.z);
        popUp.gameObject.transform.DOMove(dest, 1f)
            .OnComplete(() => Destroy(popUp));
    }

    public void EnemyPopUp(string line)
    {
        var popUp = Instantiate(popUpPrefeb, enemyPopUpPosition);
        TMP_Text popUpTmp = popUp.GetComponent<TextMeshPro>();
        popUpTmp.text = line;
        Vector3 dest = new Vector3(enemyPopUpPosition.position.x, enemyPopUpPosition.position.y + 2f, enemyPopUpPosition.position.z);
        popUp.gameObject.transform.DOMove(dest, 1f)
            .OnComplete(() => Destroy(popUp));
    }
}
