using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


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
    public List<CardOnFeild> playerEquip;
    public List<CardOnFeild> enemyEquip;

    public CardOnFeild playerCard;
    public CardOnFeild enemyCard;
    public CardOnFeild chosenHand;
    bool isMyCardDrag;
    bool onMyCardArea;
    bool onMyWeaponArea;
    bool onMyArmorArea;
    bool onMyArtifactArea;

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
    public Transform playerEquipSpawnPosition;
    public Transform[] playerEquipPositions;
    public Transform[] playerEquipSkillPositions;

    public Transform enemyPosition;
    public Transform enemyEquipSpawnPosition;
    public Transform[] enemyEquipPositions;
    public Transform[] enemyEquipSkillPositions;

    public GameObject dummyBackCard;
    public GameObject playerSkillBackground;
    public GameObject enemySkillBackground;
    public GameObject rewardButtons;

    System.Random random = new System.Random();
    WaitForSeconds delay01 = new WaitForSeconds(0.1f);
    WaitForSeconds delay03 = new WaitForSeconds(0.3f);

    private void Start()
    {
        SetPlayerCard();
    }

    private void Update()
    {
        if (isMyCardDrag && chosenHand != null)
        {
            CardDrag();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ShowEquipCards();
        }
    }

    public void SetPlayerCard()
    {
        playerCard = DrawCard(cardSO.player, handSpawnPoint);
        EntityController.instance.player.UpdateEntity(playerCard);
        playerCard.ShowHealthbar(true);
        CardMoveTo(playerCard, playerCardPosition);
    }

    public void SetBattlePosition(bool on)
    {
        if (on)
        {
            if (playerCard != null)
            {
                CardMoveTo(playerCard, playerBattlePosition);
                EntityController.instance.player.entityPopUpPosition = playerBattlePosition;
            }
            if (enemyCard != null)
            {
                CardMoveTo(enemyCard, enemyPosition);
                enemyCard.ShowHealthbar(true);
            }
            playerSkillBackground.transform.DOMove(Vector3.zero, 0.3f);
            enemySkillBackground.transform.DOMove(Vector3.zero, 0.3f);

            StartCoroutine(PutEquipCards());
        }
        else
        {
            if (playerCard != null)
            {
                foreach (CardOnFeild cof in playerEquip)
                {
                    cof.DiscardTo(handSpawnPoint, Vector3.one);
                }
                playerEquip.Clear();

                CardMoveTo(playerCard, playerCardPosition);
                EntityController.instance.player.entityPopUpPosition = playerCardPosition;

            }
            if (enemyCard != null)
            {
                foreach (CardOnFeild cof in enemyEquip)
                {
                    cof.DiscardTo(cardSpawnPoint, Vector3.one);
                }
                enemyEquip.Clear();

                CardMoveTo(enemyCard, mainCardPosition);
                enemyCard.ShowHealthbar(false);
            }
            enemySkillBackground.transform.DOMove(Vector3.one * 16.5f, 0.3f);
            playerSkillBackground.transform.DOMove(Vector3.one * -16.5f, 0.3f);
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
                main[0].GetComponent<RenderOrder>().SetOrder(-1);

                CardMoveTo(main[0], mainCardPosition);
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
            main[0].DiscardTo(dummyBackCard.transform, Vector3.one * 2);
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

        foreach (SkillCard skill in EntityController.instance.player.skillCards)
        {
            cards.Add(skill);
        }

        StartCoroutine(PutCardInHand(cards));
    }

    public void ShowEquipCards()
    {
        List<Card> cards = new List<Card>();

        foreach (EquipmentCard equip in EntityController.instance.player.equipCards)
        {
            cards.Add(equip);
        }

        StartCoroutine(PutCardInHand(cards));
    }

    public void SetRewardCards(Card rewardEquip, Card rewardSkill)
    {
        main[0].DiscardTo(dummyBackCard.transform, Vector3.one * 2);
        main.Clear();

        List<Card> rewards = new List<Card>();
        rewards.Add(rewardEquip);
        rewards.Add(rewardSkill);

        StartCoroutine(PutCardInSelection(rewards, false));

        selection[0].SetEquipDescription(EntityController.instance.enemy.entityStat[EntityStat.Level]);
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
            cof.isMoveable = true;
            hand.Add(cof);

            SetOriginOrder(hand);
            CardAlignment(hand, handLeftPoint, handRightPoint);
            yield return delay01;
        }

        yield return delay03;
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

            if (isFront)
            {
                cof.isMinimized = true;
                CardAlignment(selection, selectionLeftPoint, selectionRightPoint);
            }
            else
            {
                cof.isMinimized = false;
                CardAlignment(selection, selectionLeftPointFlipped, selectionRightPointFlipped);
                cof.cardBackSymbol.transform.DOShakePosition(0.5f,1f);
            }
            yield return delay03;
        }
        GameManager.instance.RemoveControlBlock();
    }

    IEnumerator PutEquipCards()
    {
        GameManager.instance.AddControlBlock();
        Entity player = EntityController.instance.player;
        Entity enemy = EntityController.instance.enemy;

        if (player.entityCard != null && enemy.entityCard != null)
        {
            for (int i = 0; i < 3; i++)
            {
                playerEquip.Add(DrawCard(player.equipCards[i], playerEquipSpawnPosition));
                //playerEquip[i].SetEquipDescription(player.entityEquipLevels[i]);
                playerEquip[i].gameObject.SetActive(false);
                playerEquip[i].isMinimized = true;
                playerEquip[i].ShowEquipDurability(true);

                enemyEquip.Add(DrawCard(enemy.equipCards[i], enemyEquipSpawnPosition));
                //enemyEquip[i].SetEquipDescription(enemy.entityEquipLevels[i]);
                enemyEquip[i].gameObject.SetActive(false);
                enemyEquip[i].isMinimized = true;
                enemyEquip[i].ShowEquipDurability(true);
            }


            yield return delay03;

            for (int i = 0; i < 3; i++)
            {
                playerEquip[i].gameObject.SetActive(true);
                CardMoveTo(playerEquip[i], playerEquipPositions[i]);
                SetCardOriginPRS(playerEquip[i], playerEquipPositions[i]);

                enemyEquip[i].gameObject.SetActive(true);
                CardMoveTo(enemyEquip[i], enemyEquipPositions[i]);
                SetCardOriginPRS(enemyEquip[i], enemyEquipPositions[i]);

                yield return delay01;
            }
        }

        GameManager.instance.RemoveControlBlock();
    }

    public void SetCardOriginPRS(CardOnFeild cof, Transform targetTransform)
    {
        cof.originPRS = new PRS(targetTransform.position, targetTransform.rotation, targetTransform.localScale);
    }

    public CardOnFeild DrawCard(Card card, Transform spawnPoint)
    {
        var cardObject = Instantiate(cardPrefeb, spawnPoint.position, spawnPoint.rotation);
        cardObject.transform.localScale = spawnPoint.localScale;
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

    public void CardMouseDrag(CardOnFeild cof)
    {
        DetectCardArea();
        if (chosenHand != null)
            chosenHand.CardSelectedEffect(false);
        chosenHand = cof;
        chosenHand.CardSelectedEffect(true);
        isMyCardDrag = true;
        EnLargeCard(true, cof);
    }

    public void CardMouseUp(CardOnFeild cof)
    {
        isMyCardDrag = false;
        EnLargeCard(false, cof);

        if (!onMyCardArea && cof.isMoveable && TurnManager.instance.currentState == GameState.Battle && cof.card.type == CardType.Skill)
        {
            SkillCard skill = (SkillCard)cof.card;

            int equipNum;
            if (onMyWeaponArea)
            {
                equipNum = 0;
            }
            else if (onMyArmorArea)
            {
                equipNum = 1;
            }
            else if (onMyArtifactArea)
            {
                equipNum = 2;
            }
            else
                return;

            if (EntityController.instance.PlayerUseSkill(skill, equipNum))
            {
                SetPlayerSkillCard(cof, equipNum);
            }
        }

        switch (cof.card.type)
        {
            case CardType.Skill:
                if (!onMyCardArea && cof.isMoveable && TurnManager.instance.currentState == GameState.Battle)
                {
                    SkillCard skill = (SkillCard)cof.card;

                    int equipNum;
                    if (onMyWeaponArea)
                    {
                        equipNum = 0;
                    }
                    else if (onMyArmorArea)
                    {
                        equipNum = 1;
                    }
                    else if (onMyArtifactArea)
                    {
                        equipNum = 2;
                    }
                    else
                        return;

                    if (EntityController.instance.PlayerUseSkill(skill, equipNum))
                    {
                        SetPlayerSkillCard(cof, equipNum);
                    }
                }
                return;
        }
    }

    public void SetPlayerSkillCard(CardOnFeild cof, int equipNum)
    {
        Transform targetTransform = null;

        targetTransform = playerEquipSkillPositions[equipNum];

        PRS targetPRS = new PRS(targetTransform.position, targetTransform.rotation, targetTransform.localScale);
        cof.MoveTransform(targetPRS, true, 0.2f);
        cof.handPRS = cof.originPRS;
        cof.originPRS = targetPRS;
        cof.isMoveable = false;

        StartCoroutine(UnsetSkillCard(cof, equipNum));
    }

    IEnumerator UnsetSkillCard(CardOnFeild cof, int equipNum)
    {
        yield return new WaitForSeconds(0.2f);
        CameraEffectManager.instance.ShakeCam(0.1f, 0.5f, 30);

        SkillCard skill = (SkillCard)cof.card;
        float coolTime = skill.coolTime;

        if (coolTime > 0)
        {
            cof.StartCoolDown(coolTime);
            yield return new WaitForSeconds(coolTime);
        }

        yield return new WaitForSeconds(0.5f);

        if (cof == null)
            yield break;
        cof.originPRS = cof.handPRS;
        cof.MoveTransform(cof.originPRS, true, 0.1f);
        cof.isMoveable = true;
        EntityController.instance.player.isEnchanted[equipNum] = false;
    }

    private void CardDrag()
    {
        if (!onMyCardArea && chosenHand.isMoveable)
        {
            chosenHand.MoveTransform(new PRS(Utils.MousePos, Utils.QI, chosenHand.originPRS.scale), false);
        }
    }

    public void SkipReward()
    {
        rewardButtons.SetActive(false);
        TurnManager.instance.ChangeTurnTo(GameState.PathSelection);
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

            case (CardType.Skill):
                if (TurnManager.instance.currentState == GameState.Reward)
                {
                    if (main.Count == 0 && selection.Contains(cof))
                    {
                        SelectToMain(cof);
                        cof.transform.DOScale(Vector3.one * 1.5f, 0.3f);

                        ShowSkillCards();
                        rewardButtons.SetActive(true);

                        cof.CardSelectedEffect(true);
                    }
                }
                return;

            case (CardType.Equip):
                if (TurnManager.instance.currentState == GameState.Reward)
                {
                    if (main.Count == 0 && selection.Contains(cof))
                    {
                        SelectToMain(cof);
                        cof.transform.DOScale(Vector3.one * 1.5f, 0.3f);

                        ShowEquipCards();
                        rewardButtons.SetActive(true);

                        cof.CardSelectedEffect(true);
                    }
                }
                return;

            case (CardType.Enemy):
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

            default:
                Debug.Log("unkown card type");
                return;
        }
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int hanLayer = LayerMask.NameToLayer("DragCardArea");
        int weaponLayer = LayerMask.NameToLayer("WeapnoCardArea");
        int armorLayer = LayerMask.NameToLayer("ArmorCardArea");
        int artifactLayer = LayerMask.NameToLayer("ArtifactCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == hanLayer);
        onMyWeaponArea = Array.Exists(hits, x => x.collider.gameObject.layer == weaponLayer);
        onMyArmorArea = Array.Exists(hits, x => x.collider.gameObject.layer == armorLayer);
        onMyArtifactArea = Array.Exists(hits, x => x.collider.gameObject.layer == artifactLayer);
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
            if (newY > 8.5f)
            {
                newY = 8.5f;
            }
            Vector3 enlargePos = new Vector3(newX, newY, -50f);
            cof.MoveTransform(new PRS(enlargePos, Utils.QI, Vector3.one * 1.8f), false);

            Time.timeScale = 0f;
        }
        else
        {
            cof.MoveTransform(cof.originPRS, true, 0.1f);
            Time.timeScale = 1f;
        }
        cof.GetComponent<RenderOrder>().setMostFrontOrder(isEnlarge);
    }
}