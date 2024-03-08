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
    //영향받는 스탯 타입
    //계수
    //명중률
}

//토템 카드

[System.Serializable]
public class EnemyCard : Card
{
    //enemy 스탯 계수
    public SkillCard[] enemySkills;
    //public equip enemyEquip;
}


[System.Serializable]
public class EventCard : Card
{
    public CardType rewardType;
    //buff, debuff skill (턴)
}

[CreateAssetMenu(fileName = "CardSO", menuName = "Scriptable Object/CardSO")]
public class CardSO : ScriptableObject
{
    public Card[] mainCardTypes;
    public EventCard[] events;
    public EnemyCard[] enemies; //적정보에 스킬과 장비 미리 입력
    public Card[] quests;


    public SkillCard[] playerSkills;
    //n skillcard
    //r skill card
}
