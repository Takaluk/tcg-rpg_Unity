using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;

public class CardOnFeild : MonoBehaviour
{
    [SerializeField] SpriteRenderer cardSprite;
    [SerializeField] SpriteRenderer backgroundSprite;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text cardTypeTMP;
    [SerializeField] TMP_Text descriptionTMP;
    [SerializeField] TMP_Text skillCostTMP;
    [SerializeField] TMP_Text skillCoolTMP;
    [SerializeField] Sprite cardFront;
    [SerializeField] Sprite cardBack;
    [SerializeField] TMP_Text currentHpTMP;
    [SerializeField] TMP_Text maxHpTMP;
    [SerializeField] GameObject healthBar;
    [SerializeField] GameObject equipDurability;
    [SerializeField] GameObject cardSelectAnimation;
    [SerializeField] Image healthbarSprite;
    [SerializeField] Image durabilitySprite;
    [SerializeField] Image[] manaGageSprites;
    [SerializeField] Image coolDownSprite;
    public SpriteRenderer characterSprite;
    public SpriteRenderer cardBackSymbol;

    public Card card;
    public PRS originPRS;
    public PRS handPRS;

    public int lineIndex = 0;
    public bool isMinimized = false;
    public bool isMoveable = false;
    public float textSpeed = 0.3f;

    [SerializeField] GameObject popUpPrefeb;


    private void OnDestroy()
    {
        transform.DOKill();
    }

    public void SetUp(Card card)
    {
        this.card = card;

        cardSprite.sprite = cardFront;
        characterSprite.sprite = card.sprite;
        string name = card.name.Replace("\\n", "\n");
        nameTMP.text = name;
        Color symbolColor = Color.white;
        switch (card.type)
        {
            case (CardType.Player):
                backgroundSprite.sprite = CardManager.instance.cardSO.playerBakcground;
                cardTypeTMP.text = "<color=white>Player";
                symbolColor = new Color(1, 1, 1, 60f / 255f);
                cardBackSymbol.color = symbolColor;

                NextDialogue();
                break;
            case (CardType.Enemy):
                backgroundSprite.sprite = CardManager.instance.currentStageData.stageBackground;
                cardTypeTMP.text = "<color=red>Enemy";
                symbolColor = new Color(1, 0, 0, 60f/255f);
                cardBackSymbol.color = symbolColor;
                break;
            case (CardType.Skill):
                backgroundSprite.sprite = CardManager.instance.cardSO.skillBakcground;
                SkillCard skill = (SkillCard)card;
                skillCostTMP.text = "<color=#1E90FF>" + skill.cost.ToString();
                skillCoolTMP.text = "<color=grey>" + skill.coolTime.ToString();
                cardTypeTMP.text = "<color=#1E90FF>Skill";
                symbolColor = new Color(0, 0, 1, 60f / 255f);
                cardBackSymbol.color = symbolColor;

                string skillDescription = "";
                foreach(SkillEffect skillEffect in skill.skillEffects)
                {
                    switch (skillEffect.skillType)
                    {
                        case (SkillType.PscDamage):
                            skillDescription += "적에게 <color=red>물리 피해</color>";
                            break;
                        case (SkillType.MgcDamage):
                            skillDescription += "적에게 <color=#5B40FF>마법 피해</color>";
                            break;
                        case (SkillType.Heal):
                            skillDescription += "자신에게 <color=green>체력 회복</color>";
                            break;
                    }

                    skillDescription += "(";

                    switch (skillEffect.skillStatType)
                    {
                        case (EntityStat.Str):
                            skillDescription += "<color=red>Str</color>/";
                            break;
                        case (EntityStat.Int):
                            skillDescription += "<color=#5B40FF>Int</color>/";
                            break;
                        case (EntityStat.MaxHP):
                            skillDescription += "<color=green>MaxHP</color>/";
                            break;
                    }
                    skillDescription += skillEffect.pow;
                    skillDescription += ")\n";
                }
                skillDescription += "(acc/" + skill.acc + ")";
                descriptionTMP.text = skillDescription;
                break;
            case (CardType.Event):
                backgroundSprite.sprite = CardManager.instance.currentStageData.stageBackground;
                cardTypeTMP.text = "<color=yellow>Event";
                symbolColor = new Color(1, 1, 0, 60f / 255f);
                cardBackSymbol.color = symbolColor;
                break;
            case CardType.Equip:
                backgroundSprite.sprite = CardManager.instance.cardSO.equipBakcground;
                cardTypeTMP.text = "<color=grey>Equip";
                symbolColor = new Color(150f / 255f, 150f / 255f, 150f / 255f, 60f / 255f);
                cardBackSymbol.color = symbolColor;
                break;
            case CardType.Stage:
                cardTypeTMP.text = "<color=blue>Location";
                cardBackSymbol.color = Color.blue;
                NextDialogue();
                break;
        }
    }

    public void SetEquipDescription(Entity entity)
    {
        EquipmentCard equip = (EquipmentCard)card;
        string equipDescription = "";
        foreach (EquipmentStats stat in equip.equipStats)
        {
            switch (stat.equipStat)
            {
                case (EntityStat.MaxHP):
                    equipDescription += "<color=green>MaxHP+";
                    break;
                case (EntityStat.Str):
                    equipDescription += "<color=red>Str+";
                    break;
                case (EntityStat.Int):
                    equipDescription += "<color=#5B40FF>Int+";
                    break;
            }
            int equipPow = stat.basePow + stat.PowPL * entity.entityEquipLevels[(int)equip.equipType];

            equipDescription += equipPow + "\n";
        }
        descriptionTMP.text = equipDescription;
    }

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {

        if (useDotween)
        {
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }

    public void DiscardTo(Transform discardPoint, Vector3 targetScale)
    {
        GameManager.instance.AddControlBlock();

        Quaternion targetRotation = Quaternion.Euler(0f, -180f, 0f);

        transform.DOMove(discardPoint.position, 0.29f).OnComplete(() => GameManager.instance.RemoveControlBlock());
        transform.DORotateQuaternion(targetRotation, 0.3f);
        transform.DOScale(targetScale, 0.3f).OnComplete(() => Destroy(gameObject));
    }

    private void OnMouseDrag()
    {
        if (GameManager.instance.GetControlBlockCount() > 0)
            return;

        if (isMinimized)
        {
            CardManager.instance.CardMouseDrag(this);
        }
    }

    private void OnMouseUp()
    {
        if (GameManager.instance.GetControlBlockCount() > 0)
            return;

        if (isMinimized)
        {
            CardManager.instance.CardMouseUp(this);
        }
    }

    private void OnMouseDown()
    {
        if (GameManager.instance.GetControlBlockCount() > 0)
            return;

        CardManager.instance.CardMouseDown(this);
    }

    #region Dialogue

    public void NextDialogue()
    {
        if (lineIndex < card.context.Length)
        {
            TypeDescription(card.context[lineIndex]);
            lineIndex++;

            if (card.type == CardType.Event)
            {
                EventCard ecard = (EventCard)card;

                if (lineIndex == ecard.eventLineIndex)
                {
                    EntityController.instance.UseSkill(ecard.skill, 3, true);
                }
            }
        }
        else
        {
            if (card.type == CardType.Enemy)
            {
                TurnManager.instance.ChangeTurnTo(GameState.Reward);
            }
            else if (card.type == CardType.Stage)
            {
                if (TurnManager.instance.currentState == GameState.PlayerCharacterSelection)
                    TurnManager.instance.ChangeTurnTo(GameState.PlayerCharacterSelection);
                return;
            }
            else
            {
                TurnManager.instance.ChangeTurnTo(GameState.PathSelection);
            }
        }
            
    }

    public void TypeDescription(string line)
    {
        descriptionTMP.text = "";
        StartCoroutine(Type(line));
    }

    IEnumerator Type(string line)
    {
        GameManager.instance.AddControlBlock();

        line = line.Replace("\\n", "\n");

        int signCounter = 0;
        foreach (char c in line.ToCharArray())
        {
            descriptionTMP.text += c;

            if (c == '<')
                signCounter++;
            else if (c == '>')
                signCounter--;

            if (signCounter > 0)
                continue;
            else
                yield return new WaitForSeconds(textSpeed);
        }

        GameManager.instance.RemoveControlBlock();
    }
    #endregion

    public void CardPopUp(string line)
    {
        Transform popUpPosition = transform;
        var popUp = Instantiate(popUpPrefeb, popUpPosition);
        TMP_Text popUpTmp = popUp.GetComponent<TextMeshPro>();
        popUpTmp.text = line;
        Vector3 dest = new Vector3(popUpPosition.position.x, popUpPosition.position.y + 1f, popUpPosition.position.z - 20f);
        if (isMinimized)
        {
            dest.y -= 2f;
            popUp.transform.localScale = Vector3.one * 1.6f;
            popUp.gameObject.transform.DOMove(dest, 0.6f)
                .OnComplete(() => Destroy(popUp));
        }
        else
        {
            popUp.gameObject.transform.DOMove(dest, 2f)
                .OnComplete(() => Destroy(popUp));
        }
    }

    public void ShowHealthbar(bool on)
    {
        healthBar.SetActive(on);
    }

    public void UpdateHealthbar(float maxHealth, float currentHealth)
    {
        maxHpTMP.text = "/ " + maxHealth.ToString();
        currentHpTMP.text = currentHealth.ToString();
        float healthbarAmount = currentHealth / maxHealth;
        healthbarSprite.fillAmount = healthbarAmount;
    }

    public void ShowEquipDurability(bool on)
    {
        equipDurability.SetActive(on);
    }

    public void UpdateEquipDurability(float maxDurability, float currentDurability)
    {
        float durabilityAmount = currentDurability / maxDurability;
        durabilitySprite.fillAmount = durabilityAmount;
    }

    public void UpdateApGage(float actionPoints)
    {
        for (int i = 0; i < manaGageSprites.Length; i++)
        {
            manaGageSprites[i].fillAmount = actionPoints - i;

            if (manaGageSprites[i].fillAmount == 1f)
                manaGageSprites[i].color = Color.white;
            else
                manaGageSprites[i].color = Color.blue;
        }
    }

    public void StartCoolDown(float coolTime)
    {
        StartCoroutine(CoolDown(coolTime));
    }

    IEnumerator CoolDown(float coolTime)
    {
        float currentTime = coolTime;
        coolDownSprite.fillAmount = currentTime/coolTime;
        while (currentTime > 0)
        {
            yield return new WaitForSeconds(0.1f);
            currentTime -= 0.1f;
            coolDownSprite.fillAmount = currentTime / coolTime;
        }
    }

    public void CardSelectedEffect(bool on)
    {
        cardSelectAnimation.SetActive(on);
    }
}
