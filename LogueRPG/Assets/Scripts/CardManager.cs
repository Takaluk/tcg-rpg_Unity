using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


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
    public List<CardOnFeild> enemySkill;

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
    public GameObject acceptButtons;
    public GameObject selectPlayerUI;
    public GameObject inventoryUI;

    public GameObject coinToss_success;
    public GameObject coinToss_failure;
    public Transform[] coinTransfoms;

    public Button acceptButton;
    public Button selectPlayerButton;
    public Button showEquipButton;
    public Button showSkillButton;
    public Button showMenuButton;

    Image showEquipButtonImage;
    Image showSkillButtonImage;


    System.Random random = new System.Random();
    WaitForSeconds delay01 = new WaitForSeconds(0.1f);
    WaitForSeconds delay03 = new WaitForSeconds(0.3f);

    public SpriteRenderer background;

    public StageData currentStageData { get; private set; }

    [Header("FirstStageCard")]
    public StageCard firstStageCard;

    private void Start()
    {
        main.Add(DrawCard(firstStageCard, cardSpawnPoint));
        CardMoveTo(main[0], mainCardPosition);
        showEquipButtonImage = showEquipButton.GetComponent<Image>();
        showSkillButtonImage = showSkillButton.GetComponent<Image>();
    }

    private void Update()
    {
        if (isMyCardDrag && chosenHand != null)
        {
            CardDrag();
        }
    }

    public void SelectPlayerCharacter()
    {
        if (chosenHand.card.type == CardType.Player)
        {
            hand.Remove(chosenHand);
            chosenHand.CardSelectedEffect(false);
            chosenHand.GetComponent<RenderOrder>().SetOrder(-1);
            SetPlayerCard(chosenHand);
            selectPlayerUI.GetComponent<UIEnableAnimation>().ShowUI(false);

            TurnManager.instance.ChangeStageTo(StageType.Wood);
            TurnManager.instance.ChangeTurnTo(GameState.PathSelection);
        }
    }

    public void SetPlayerCard(CardOnFeild card)
    {
        playerCard = card;
        EntityController.instance.player.UpdateEntity(playerCard);
        playerCard.NextDialogue();
        playerCard.isMinimized = false;
        playerCard.isMoveable = false;
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

            PutEnemySkillCards();
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
                EmptyHand();
                foreach (CardOnFeild cof in enemyEquip)
                {
                    cof.DiscardTo(cardSpawnPoint, Vector3.one);
                }
                enemyEquip.Clear();

                foreach (CardOnFeild cof in enemySkill)
                {
                    cof.DiscardTo(cardSpawnPoint, Vector3.one);
                }
                enemySkill.Clear();

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

    public void ShowRewardAcceptUI(bool on)
    {
        if (acceptButtons.activeSelf != on)
            acceptButtons.GetComponent<UIEnableAnimation>().ShowUI(on);
        if (TurnManager.instance.currentState == GameState.Reward)
            acceptButton.interactable = false;
        else
            acceptButton.interactable = true;

    }

    public void ShowInventoryUI(bool on)
    {
        if (inventoryUI.activeSelf != on)
        {
            inventoryUI.GetComponent<UIEnableAnimation>().ShowUI(on);

            showSkillButtonImage.color = Color.white;
            showEquipButtonImage.color = Color.white;
        }
    }

    public void ShowPlayerSelection()
    {
        List<Card> cards = new List<Card>();

        foreach (EnemyCard player in cardSO.playerCharacters)
        {
            cards.Add(player);
        }

        StartCoroutine(PutCardInHand(cards));

        selectPlayerUI.GetComponent<UIEnableAnimation>().ShowUI(true);
        selectPlayerButton.interactable = false;
    }

    public void ShowMainCardSelection()
    {
        if (main.Count > 0)
        {
            main[0].DiscardTo(dummyBackCard.transform, Vector3.one * 2);
            main.Clear();
        }

        List<Card> selectionCards = new List<Card>();

        int currenTurn = GameManager.instance.GetTurnCount();
        int mainEnemyIndex = Array.IndexOf(currentStageData.mainEnemyTurns, currenTurn);
        int mainEventIndex = Array.IndexOf(currentStageData.mainEventTurns, currenTurn);

        if (mainEnemyIndex > -1)
        {
            selectionCards.Add(currentStageData.mainEnemies[mainEnemyIndex]);
        }
        else if (mainEventIndex > -1)
        {
            selectionCards.Add(currentStageData.mainEvents[mainEventIndex]);
        }
        else
            for (int i = 0; i < 3; i++) //player data
            {
                Card mainCardData = ChooseRandomCard(cardSO.mainCardTypes, random);
                selectionCards.Add(GetRandomCard(mainCardData.type));
            }

        StartCoroutine(PutCardInSelection(selectionCards, false));
    }

    public void ShowSkillCards()
    {
        if (TurnManager.instance.currentState != GameState.Battle)
        {
            if (GameManager.instance.GetControlBlockCount() > 0)
                return;
            if (hand.Count > 0 && hand[0].card.type == CardType.Skill)
            {
                EmptyHand();
                showSkillButtonImage.color = Color.white;
                return;
            }
            showEquipButtonImage.color = Color.white;
            showSkillButtonImage.color = Color.gray;
        }

        List<Card> cards = new List<Card>();

        foreach (SkillCard skill in EntityController.instance.player.skillCards)
        {
            cards.Add(skill);
        }

        StartCoroutine(PutCardInHand(cards));
    }

    public void ShowEquipCards()
    {
        if (TurnManager.instance.currentState == GameState.PathSelection || TurnManager.instance.currentState == GameState.Event)
        {
            if (GameManager.instance.GetControlBlockCount() > 0)
                return;
            if (hand.Count > 0 && hand[0].card.type == CardType.Equip)
            {
                EmptyHand();
                showEquipButtonImage.color = Color.white;
                return;
            }
            showEquipButtonImage.color = Color.gray;
            showSkillButtonImage.color = Color.white;
        }

        List<Card> cards = new List<Card>();

        foreach (EquipmentCard equip in EntityController.instance.player.equipCards)
        {
            cards.Add(equip);
        }

        StartCoroutine(PutCardInHand(cards));
    }

    public void SetRewardCards(Card rewardEquip, Card rewardSkill)
    {
        main[0].DOKill();
        main[0].DiscardTo(dummyBackCard.transform, Vector3.one * 2);
        main.Clear();

        List<Card> rewards = new List<Card>
        {
            rewardEquip,
            rewardSkill
        };

        StartCoroutine(PutCardInSelection(rewards, false));

        selection[0].SetEquipDescription(false);
    }

    public void EmptyHand()
    {
        if (hand.Count > 0)
            StartCoroutine(PutHandInInven());
    }

    public void PutEnemySkillCards()
    {
        foreach (Card card in EntityController.instance.enemy.skillCards)
        {
            CardOnFeild cof = DrawCard(card, cardSpawnPoint);
            cof.isMoveable = true;
            cof.isMinimized = true;
            cof.originPRS = new PRS(cof.transform.position, cof.transform.rotation, cof.transform.localScale);
            enemySkill.Add(cof);
        }
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
            if (cof.card.type == CardType.Equip)
                cof.SetEquipDescription(true);
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
                playerEquip[i].SetEquipDescription(true);
                playerEquip[i].gameObject.SetActive(false);
                playerEquip[i].isMinimized = true;
                playerEquip[i].ShowEquipDurability(true);
                playerEquip[i].GetComponent<RenderOrder>().SetOriginOrder(-1);

                enemyEquip.Add(DrawCard(enemy.equipCards[i], enemyEquipSpawnPosition));
                enemyEquip[i].SetEquipDescription(false);
                enemyEquip[i].gameObject.SetActive(false);
                enemyEquip[i].isMinimized = true;
                enemyEquip[i].ShowEquipDurability(true);
                enemyEquip[i].GetComponent<RenderOrder>().SetOriginOrder(-1);
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

        switch (cardType)
        {
            case CardType.Enemy:
                card = ChooseRandomEnemy();
                break;
            case CardType.Event:
                card = ChooseRandomEvent();
                break;
            case CardType.Stage:
                card = ChooseRandomStage();
                break;
        }

        return card;
    }

    public Card ChooseRandomEvent()
    {
        return ChooseRandomCard(currentStageData.events, random);
    }
    public Card ChooseRandomEnemy()
    {
        return ChooseRandomCard(currentStageData.enemies, random);
    }

    public Card ChooseRandomStage()
    {
        return ChooseRandomCard(currentStageData.connectedStages, random);
    }

    public CardOnFeild GetRandomEnemySkillCard()
    {
        SkillCard skill = (SkillCard)ChooseRandomCard(EntityController.instance.enemy.skillCards.ToArray(), random);

        foreach (CardOnFeild cof in enemySkill)
        {
            if (skill == cof.card && cof.isMoveable)
            {
                return cof;
            }
        }

        return null;
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

    public void SetStageData(StageType stage)
    {
        switch (stage)
        {
            case StageType.Wood:
                currentStageData = cardSO.woodStageData;
                break;

            case StageType.Castle:
                currentStageData = cardSO.castleStageData;
                break;

            case StageType.Town:
                currentStageData = cardSO.townStageData;
                break;
        }

        background.sprite = currentStageData.stageBackground;
    }

    public void CardMouseDrag(CardOnFeild cof)
    {
        DetectCardArea();
        if (chosenHand != null)
            chosenHand.CardSelectedEffect(false);
        chosenHand = cof;
        if (TurnManager.instance.currentState == GameState.Reward)
        {
            chosenHand.CardSelectedEffect(true);
            acceptButton.interactable = true;
        }
        else if (TurnManager.instance.currentState == GameState.PlayerCharacterSelection)
        {
            chosenHand.CardSelectedEffect(true);
            selectPlayerButton.interactable = true;
        }

        isMyCardDrag = true;
        EnLargeCard(true, cof);
    }

    public void CardMouseUp(CardOnFeild cof)
    {
        isMyCardDrag = false;
        EnLargeCard(false, cof);

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

                    if (EntityController.instance.UseSkill(skill, equipNum, true))
                        SetSkillCard(cof, equipNum, true);
                }
                return;
        }
    }

    public void SetSkillCard(CardOnFeild cof, int equipNum, bool isPlayer = true)
    {
        Transform targetTransform = null;

        if (isPlayer)
            targetTransform = playerEquipSkillPositions[equipNum];
        else
            targetTransform = enemyEquipSkillPositions[equipNum];

        PRS targetPRS = new PRS(targetTransform.position, targetTransform.rotation, targetTransform.localScale);
        cof.MoveTransform(targetPRS, true, 0.2f);
        cof.handPRS = cof.originPRS;
        cof.originPRS = targetPRS;
        cof.isMoveable = false;
        cof.setNum = equipNum;

        StartCoroutine(UnsetSkillCard(cof, equipNum, isPlayer));
    }

    IEnumerator UnsetSkillCard(CardOnFeild cof, int equipNum, bool isPlayer)
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
        cof.setNum = -1;
        if (isPlayer)
            EntityController.instance.player.isEnchanted[equipNum] = false;
        else
            EntityController.instance.enemy.isEnchanted[equipNum] = false;
    }

    private void CardDrag()
    {
        if (!onMyCardArea && chosenHand.isMoveable)
        {
            chosenHand.MoveTransform(new PRS(Utils.MousePos, Utils.QI, chosenHand.originPRS.scale), false);
        }
    }

    public void AcceptEvent()
    {
        if (GameManager.instance.GetControlBlockCount() > 0)
            return;

        switch (TurnManager.instance.currentState)
        {
            case GameState.Stage:
                StageCard card = (StageCard)main[0].card;
                TurnManager.instance.ChangeStageTo(card.stageType);
                break;
            case GameState.Reward:
                if (chosenHand.card.type == CardType.Equip)
                    EntityController.instance.player.ChangeEquip((EquipmentCard)main[0].card, GameManager.instance.GetEnemyLevel());
                else if (chosenHand.card.type == CardType.Skill)
                {
                    for (int i = 0; i < hand.Count; i++)
                    {
                        if (hand[i] == chosenHand)
                        {
                            EntityController.instance.player.ChangeSkill((SkillCard)main[0].card, i);
                        }
                    }
                }
                hand.Remove(chosenHand);
                hand.Add(main[0]);
                main[0] = chosenHand;
                break;
            case GameState.ButtonEvent:
                ShowRewardAcceptUI(false);
                EventCointToss();
                return;
        }

        TurnManager.instance.ChangeTurnTo(GameState.PathSelection);
        ShowRewardAcceptUI(false);
    }

    public void EventCointToss()
    {
        if (main[0] == null || main[0].card.type != CardType.Event)
            return;
        GameManager.instance.AddControlBlock();
        GameObject coin;
        EventCard ecard = (EventCard)main[0].card;
        if (ecard.eventRewardType == EventRewardType.Trap)
        {
            coin = Instantiate(coinToss_failure, coinTransfoms[0]);
        }
        else
        {
            coin = Instantiate(coinToss_success, coinTransfoms[0]);
        }
        Sequence coinSequence = DOTween.Sequence();
        coinSequence.Append(coin.transform.DOMove(coinTransfoms[1].position, 0.5f));
        coinSequence.Join(coin.transform.DOScale(coinTransfoms[1].localScale, 0.5f));
        coinSequence.Append(coin.transform.DOMove(coinTransfoms[2].position, 0.5f));
        coinSequence.Join(coin.transform.DOScale(coinTransfoms[2].localScale, 0.5f));
        coinSequence.AppendInterval(1f);
        coinSequence.OnComplete(() =>
        {
            GameManager.instance.RemoveControlBlock();
            coin.transform.DOKill();
            Destroy(coin);

            TurnManager.instance.ChangeTurnTo(GameState.Event);
            main[0].NextDialogue();
        });
    }

    public void SkipEvent()
    {
        if (GameManager.instance.GetControlBlockCount() > 0)
            return;

        TurnManager.instance.ChangeTurnTo(GameState.PathSelection);
        ShowRewardAcceptUI(false);
    }

    public void CardMouseDown(CardOnFeild cof)
    {
        switch (cof.card.type)
        {
            case(CardType.Event):
                if (TurnManager.instance.currentState == GameState.ButtonEvent)
                    return;
                if (TurnManager.instance.currentState == GameState.PathSelection)
                {
                    SelectToMain(cof);
                    EventCard ecard = (EventCard)cof.card;
                    if (ecard.buttonEvent)
                    {
                        TurnManager.instance.ChangeTurnTo(GameState.ButtonEvent);
                    }
                    else
                        TurnManager.instance.ChangeTurnTo(GameState.Event);
                }
                cof.NextDialogue();
                return;

            case (CardType.Skill):
                if (TurnManager.instance.currentState == GameState.Event)
                {
                    if (main.Count == 0 && selection.Contains(cof))
                    {
                        ShowSkillCards();
                        TurnManager.instance.ChangeTurnTo(GameState.Reward);

                        SelectToMain(cof);
                        cof.CardSelectedEffect(true);
                    }
                }
                return;

            case (CardType.Equip):
                if (TurnManager.instance.currentState == GameState.Event)
                {
                    if (main.Count == 0 && selection.Contains(cof))
                    {
                        TurnManager.instance.ChangeTurnTo(GameState.Reward);

                        SelectToMain(cof);
                        EquipmentCard rewardEquip = (EquipmentCard)main[0].card;
                        List<Card> cards = new List<Card>();
                        EquipmentCard equip = EntityController.instance.player.equipCards[(int)rewardEquip.equipType];
                        cards.Add(equip);
                        StartCoroutine(PutCardInHand(cards));
                        cof.CardSelectedEffect(true);
                    }
                }
                return;

            case (CardType.Enemy):
                if (TurnManager.instance.currentState == GameState.PathSelection)
                {
                    enemyCard = cof;
                    SelectToMain(cof);
                    TurnManager.instance.ChangeTurnTo(GameState.Battle);
                }
                else if (TurnManager.instance.currentState == GameState.Battle)
                {
                    StatusDataController.instance.PopupEntityStatus(EntityController.instance.enemy);
                }
                else if (TurnManager.instance.currentState == GameState.Event)
                {
                    cof.NextDialogue();
                }
                return;

            case (CardType.Player):
                if (TurnManager.instance.currentState != GameState.PlayerCharacterSelection)
                {
                    StatusDataController.instance.PopupEntityStatus(EntityController.instance.player);
                }
                return;

            case CardType.Stage:
                if (TurnManager.instance.currentState == GameState.PathSelection)
                {
                    TurnManager.instance.ChangeTurnTo(GameState.Stage);
                    acceptButton.interactable = false; //Stage제한
                    SelectToMain(cof);
                }
                else if (TurnManager.instance.currentState == GameState.PlayerCharacterSelection)
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

            GameManager.instance.SetTimeScale(0f);
        }
        else
        {
            cof.MoveTransform(cof.originPRS, true, 0.1f);
            GameManager.instance.SetTimeScale(1f);
        }
        cof.GetComponent<RenderOrder>().setMostFrontOrder(isEnlarge);
    }
}