using System.Collections;
using System.Collections.Generic;
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

        stat.Add(EntityStat.level, level);
        stat.Add(EntityStat.maxHP, 100 + (50 * level));
        stat.Add(EntityStat.currentHP, stat[EntityStat.maxHP]);
        stat.Add(EntityStat.attack, (int)((1 + 0.2 * level) * 10));
    }

    public void TakeDamage(int damage)
    {
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

    public int initialPlayerLevel;
    public int initialEnemyLevel;

    public Entity player;
    public Entity enemy;

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
}
