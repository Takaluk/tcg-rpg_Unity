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
    Equip
};

public enum EquipType
{
    None = 0,
    Weapon,
    Armor,
    Artifact
}

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
    public int acc;
    public List<SkillEffect> skillEffects;
}

[System.Serializable]
public class SkillEffect
{
    public SkillType skillType;
    public EntityStat skillStatType;
    public int pow;
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
}

[System.Serializable]
public class EquipmentStats
{
    public EntityStat equipStat;
    public int basePow;
    public int PowPL;
}

[System.Serializable]
public class EventCard : Card
{
    public int eventLineIndex;
    public SkillCard skill;
}

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    public Card[] mainCardTypes;
    public EventCard[] events;
    public EnemyCard[] enemies; //�������� ��ų�� ��� �̸� �Է�
    public Card[] quests;

    public EnemyCard player;
    //n skillcard
    //r skill card
}
