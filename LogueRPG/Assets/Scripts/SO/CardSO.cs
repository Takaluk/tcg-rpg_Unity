using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
public enum CardType
{
    None = 0,
    Player,
    Enemy,
    Event,
    Quest,
    Skill,
    Equip,
    Stage
};

public enum EquipType
{
    Weapon = 0,
    Armor,
    Artifact
}

public enum EventRewardType
{
    None = 0,
    Effect,
    Card,
    Trap
};


[System.Serializable]
public class Card
{
    public string name;
    public string[] context;
    public int weight;
    public CardType type;
    public Sprite sprite;
}

[System.Serializable]
public class SkillCard : Card
{
    public int cost;
    public int coolTime;
    public int acc;
    public List<SkillEffect> skillEffects;
}

[System.Serializable]
public class SkillEffect
{
    public SkillType skillType;
    public EntityStat skillStatType;
    public int pow;
    public int buffDur;
    public Sprite buffIcon;
    public GameObject vfx;
}

[System.Serializable]
public class EnemyCard : Card
{
    public List<EquipmentCard> equipmentCards;
    public List<SkillCard> enemySkills;
}

[System.Serializable]
public class EquipmentCard : Card
{
    public EquipType equipType;
    public List<EquipmentStats> equipStats;
    public int Durability;
}

[System.Serializable]
public class EquipmentStats
{
    public EntityStat equipStat;
    //enchant buff
    public int basePow;
    public int PowPL;
}

[System.Serializable]
public class EventCard : Card
{
    public bool buttonEvent;
    public EventRewardType eventRewardType;
    public int eventLineIndex;

    public SkillCard eventEffect;
    //public SkillCard trapEffect;

    public SkillCard rewardSkill;
    public EquipmentCard rewardEquip;
}

[System.Serializable]
public class StageCard : Card
{
    public StageType stageType;
}

[System.Serializable]
public class StageData
{
    public Sprite stageBackground;

    public EnemyCard[] enemies;
    public EventCard[] events;
    public StageCard[] connectedStages;

    public int[] mainEnemyTurns;
    public EnemyCard[] mainEnemies;
    public int[] mainEventTurns;
    public EventCard[] mainEvents;
}

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    public Card[] mainCardTypes;

    public EnemyCard[] playerCharacters;

    [Header("MapData")]
    public StageData woodStageData;
    public StageData castleStageData;
    public StageData townStageData;

    [Header("Backgrounds")]
    public Sprite skillBakcground;
    public Sprite equipBakcground;
    public Sprite playerBakcground;
}
