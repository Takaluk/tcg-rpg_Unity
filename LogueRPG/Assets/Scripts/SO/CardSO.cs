using System.Collections.Generic;
using UnityEngine;
public enum CardType
{
    None = 0,
    Enemy,
    Event,
    Quest,
    Skill,
    Equip
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
    public int acc;
    public SkillEffect[] skillEffects;
    //����޴� ���� Ÿ��
    //���
    //���߷�
}

//���� ī��

[System.Serializable]
public class EnemyCard : Card
{
    //enemy ���� ���
    public SkillCard[] enemySkills;
    //public equip enemyEquip;
}


[System.Serializable]
public class EventCard : Card
{
    public CardType rewardType;
    //buff, debuff skill (��)
}

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    public Card[] mainCardTypes;
    public EventCard[] events;
    public EnemyCard[] enemies; //�������� ��ų�� ��� �̸� �Է�
    public Card[] quests;


    public SkillCard[] playerSkills;
    //n skillcard
    //r skill card
}
