using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;


public class CardManager : MonoBehaviour
{
    #region Instance
    private static CardManager m_instance;
    public static CardManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<CardManager>();
            }

            return m_instance;
        }
    }
    #endregion

    public CardSO cardSO;
    public GameObject cardPrefeb;
    public CardOnFeild baseMainCard;
    public List<CardOnFeild> main;
    public List<CardOnFeild> hand;
    public List<CardOnFeild> selection;

    public CardOnFeild playerCard;
    public CardOnFeild enemyCard;
    public CardOnFeild chosenHand;
    public CardOnFeild chosenSelection;
    bool isMyCardDrag;
    bool onMyCardArea;

    public Transform cardSpawnPoint;
    public Transform selectionLeftPoint;
    public Transform selectionRightPoint;
    public Transform selectionLeftPointFlipped;
    public Transform selectionRightPointFlipped;
    public Transform handSpawnPoint;
    public Transform handLeftPoint;
    public Transform handRightPoint;
    public Transform mainCardPosition;

    public Transform playerCardPosition;
    public Transform playerBattlePosition;
    public Transform playerWeapon;
    public Transform playerEquipment;
    public Transform playerAccessory;

    public Transform enemyPosition;
    public Transform enemyWeapon;
    public Transform enemyEquipment;
    public Transform enemyAccessory;

    public GameObject dummyBackCard;
    public GameObject EnemySkillPositions;
    public GameObject PlayerSkillPositions;

    System.Random random = new System.Random();
    WaitForSeconds delay01 = new WaitForSeconds(0.1f);
    WaitForSeconds delay03 = new WaitForSeconds(0.3f);

    private void Start()
    {
        playerCard = DrawCard(GetRandomCard(CardType.Enemy), handSpawnPoint);
        CardMoveTo(playerCard, playerCardPosition);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            TurnManager.instance.ChangeTurnTo(GameState.PathSelection);
        }

        if (isMyCardDrag)
        {
            CardDrag();
        }
    }

    public void SetBattlePosition(bool on)
    {
        if (on)
        {
            if (playerCard != null)
            {
                CardMoveTo(playerCard, playerBattlePosition);
            }
            if (enemyCard != null)
                CardMoveTo(enemyCard, enemyPosition);
            dummyBackCard.SetActive(false);
            EnemySkillPositions.transform.DOMove(Vector3.zero, 0.3f);
            PlayerSkillPositions.transform.DOMove(Vector3.zero, 0.3f);
        }
        else
        {
            if (playerCard != null)
            {
                CardMoveTo(playerCard, playerCardPosition);
            }
            if (enemyCard != null)
                CardMoveTo(enemyCard, mainCardPosition);

            dummyBackCard.SetActive(true);
            EnemySkillPositions.transform.DOMove(Vector3.one * 15.5f, 0.3f);
            PlayerSkillPositions.transform.DOMove(Vector3.one * -15.5f, 0.3f);
        }
    }

    public void SelectToMain(CardOnFeild cof)
    {
        for (int i = 0; i < selection.Count; i++)
        {
            if (cof == selection[i])
            {
                main.Add(selection[i]);
                main[0].isMinimized = false;

                CardMoveTo(main[0], mainCardPosition);
                main[0].GetComponent<RenderOrder>().SetOriginOrder(-1);
            }
            else
            {
                selection[i].DiscardTo(cardSpawnPoint, Vector3.one);
            }
        }

        selection.Clear();

        //change turn to selectedcard
    }

    public void ShowMainCardSelection()
    {
        if (main.Count > 0)
        {
            main[0].DiscardTo(mainCardPosition, Vector3.one * 2);
            main.Clear();
        }


        List<Card> selectionCards = new List<Card>();

        for (int i = 0; i < 3; i++) //player data
        {
            Card mainCardData = ChooseRandomCard(cardSO.mainCardTypes, random);
            selectionCards.Add(GetRandomCard(mainCardData.type));
        }

        StartCoroutine(PutCardInSelection(selectionCards, false));
    }

    public void ShowSkillCards()
    {
        List<Card> cards = new List<Card>();

        foreach (SkillCard skill in cardSO.playerSkills)
        {
            cards.Add(skill);
        }

        StartCoroutine(PutCardInHand(cards));
    }

    public void EmptyHand()
    {
        if (hand.Count > 0)
            StartCoroutine(PutHandInInven());
    }

    IEnumerator PutCardInHand(List<Card> CardList)
    {
        GameManager.instance.AddControlBlock();
        if (hand.Count > 0)
        {
            hand.Reverse();

            foreach (CardOnFeild cof in hand)
            {
                cof.DiscardTo(handSpawnPoint, Vector3.one);
                yield return delay01;
            }
        }

        hand.Clear();
        yield return delay03;

        foreach (Card card in CardList)
        {
            CardOnFeild cof = DrawCard(card, handSpawnPoint);
            cof.isMinimized = true;
            hand.Add(cof);

            SetOriginOrder(hand);
            CardAlignment(hand, handLeftPoint, handRightPoint);
            yield return delay01;
        }

        GameManager.instance.RemoveControlBlock();
    }
    IEnumerator PutHandInInven()
    {
        GameManager.instance.AddControlBlock();
        hand.Reverse();

        foreach (CardOnFeild cof in hand)
        {
            cof.DiscardTo(handSpawnPoint, Vector3.one);
            yield return delay01;
        }

        hand.Clear();
        GameManager.instance.RemoveControlBlock();
    }

    IEnumerator PutCardInSelection(List<Card> cardList, bool isFront)
    {
        GameManager.instance.AddControlBlock();

        foreach (Card card in cardList)
        {
            CardOnFeild cof = DrawCard(card, cardSpawnPoint);
            selection.Add(cof);

            SetOriginOrder(selection);
            if (isFront)
            {
                cof.isMinimized = true;
                CardAlignment(selection, selectionLeftPoint, selectionRightPoint);
            }
            else
            {
                cof.isMinimized = false;
                CardAlignment(selection, selectionLeftPointFlipped, selectionRightPointFlipped);
            }
            yield return delay03;
        }

        GameManager.instance.RemoveControlBlock();
    }

    IEnumerator PutCardInAlignment(List<Card> cardList, List<CardOnFeild> target, Transform leftPoint, Transform rightPoint, Transform spawnPoint)
    {
        GameManager.instance.AddControlBlock();
        foreach (Card card in cardList)
        {
            CardOnFeild cof = DrawCard(card, spawnPoint);
            cof.isMinimized = true;
            target.Add(cof);

            SetOriginOrder(target);
            CardAlignment(target, leftPoint, rightPoint);
            yield return delay01;
        }
        GameManager.instance.RemoveControlBlock();
    }

    public CardOnFeild DrawCard(Card card, Transform spawnPoint)
    {
        var cardObject = Instantiate(cardPrefeb, spawnPoint.position, spawnPoint.rotation);
        var cardOnFeild = cardObject.GetComponent<CardOnFeild>();
        cardOnFeild.SetUp(card);

        return cardOnFeild;
    }

    private void CardMoveTo(CardOnFeild card, Transform target)
    {
        PRS targetPRS = new PRS(target.position, target.rotation, target.localScale);
        card.MoveTransform(targetPRS, true, 0.3f);
    }

    void SetOriginOrder(List<CardOnFeild> cards)
    {
        int count = cards.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = cards[i];
            targetCard.GetComponent<RenderOrder>().SetOriginOrder(i);
        }
    }

    void CardAlignment(List<CardOnFeild> cards, Transform left, Transform right)
    {
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(left, right, cards.Count, 1f, left.localScale);

        int count = cards.Count;
        for (int i = 0; i < count; i++)
        {
            var targetCard = cards[i];

            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.3f);
        }
    }

    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);

        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2: objLerps = new float[] { 0.27f, 0.73f }; break;
            case 3: objLerps = new float[] { 0.1f, 0.5f, 0.9f }; break;
            default:
                float interval = 1f / (objCount - 1);
                for (int i = 0; i < objCount; i++  )
                {
                    objLerps[i] = interval * i;
                }
                break;
        }

        for (int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Utils.QI;
            float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
            targetPos.y += curve;
            targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);

            results.Add(new PRS(targetPos, targetRot, scale));
        }

        return results;
    }


    public Card GetRandomCard(CardType cardType)
    {
        Card card = null;

        if (cardType == CardType.Enemy)
        {
            card = ChooseRandomEnemy();
        }
        else if (cardType == CardType.Event)
        {
            card = ChooseRandomEvent();
        }
        else if (cardType == CardType.Quest)
        {
            card = ChooseRandomQuest();
        }
        else
        {
            Debug.Log("GetRandomCard: unknown card type");
        }

        return card;
    }

    public Card ChooseRandomEvent()
    {
        return ChooseRandomCard(cardSO.events, random);
    }
    public Card ChooseRandomEnemy()
    {
        return ChooseRandomCard(cardSO.enemies, random);
    }

    public Card ChooseRandomQuest()
    {
        return ChooseRandomCard(cardSO.quests, random);
    }

    Card ChooseRandomCard(Card[] cards, System.Random random)
    {
        int totalWeight = cards.Sum(card => card.weight);
        int randomValue = random.Next(totalWeight);

        foreach (var card in cards)
        {
            if (randomValue < card.weight)
            {
                return card;
            }
            randomValue -= card.weight;
        }

        return cards.Last();
    }


    #region MyCardControl

    public void CardMouseDrag(CardOnFeild cof)
    {
        DetectCardArea();
        chosenHand = cof;
        isMyCardDrag = true;
        EnLargeCard(true, cof);
    }

    public void CardMouseUp(CardOnFeild cof)
    {
        isMyCardDrag = false;
        EnLargeCard(false, cof);

        if (!onMyCardArea && TurnManager.instance.currentState == GameState.Battle && cof.card.type == CardType.Skill)
        {
            SkillCard skill = (SkillCard) cof.card;

            EntityController.instance.PlayerUseSkill(skill);
        }
    }

    private void CardDrag()
    {
        if (!onMyCardArea)
        {
            chosenHand.MoveTransform(new PRS(Utils.MousePos, Utils.QI, chosenHand.originPRS.scale), false);
        }
    }

    public void CardMouseDown(CardOnFeild cof)
    {
        switch (cof.card.type)
        {
            case(CardType.Event):
                if (TurnManager.instance.currentState == GameState.PathSelection)
                {
                    SelectToMain(cof);
                    TurnManager.instance.ChangeTurnTo(GameState.Event);
                }
                cof.NextDialogue();
                return;

            case(CardType.Enemy):
                if (cof == playerCard)
                {
                    Debug.Log("player status");
                    return;
                }

                if (TurnManager.instance.currentState == GameState.PathSelection)
                {
                    enemyCard = cof;
                    SelectToMain(cof);
                    TurnManager.instance.ChangeTurnTo(GameState.Battle);
                }
                else if (TurnManager.instance.currentState == GameState.Battle)
                {
                    Debug.Log("enemy status");
                }
                else if (TurnManager.instance.currentState == GameState.Event)
                {
                    cof.NextDialogue();
                }
                return;

            case (CardType.Player):
                Debug.Log("enetity status");
                return;

            default:
                Debug.Log("unkown card type");
                return;
        }

        // if (onMyCardArea)
        // {
        //     chosenHand = cof;
        // }
        // else
        // {
        //     chosenSelection = cof;
        // }
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("DragCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    void EnLargeCard(bool isEnlarge, CardOnFeild cof)
    {
        if (isEnlarge)
        {
            float newX = cof.originPRS.pos.x;
            float newY = cof.originPRS.pos.y + 4f;

            if (cof.originPRS.pos.x < -2.9f || cof.originPRS.pos.x > 2.9f)
                if (cof.originPRS.pos.x < 0)
                    newX = -2.9f;
                else
                    newX = 2.9f;

            Vector3 enlargePos = new Vector3(newX, newY, -50f);
            cof.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 1.5f), false);
        }
        else
            cof.MoveTransform(cof.originPRS, true, 0.3f); //ī�� �ǵ��ư��� Ȱ��ȭ/��Ȱ��ȭ

        cof.GetComponent<RenderOrder>().setMostFrontOrder(isEnlarge);
    }
    #endregion
}