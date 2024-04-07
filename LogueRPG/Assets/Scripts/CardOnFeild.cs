using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Drawing;

public class CardOnFeild : MonoBehaviour
{
    [SerializeField] SpriteRenderer cardSprite;
    [SerializeField] SpriteRenderer backgroundSprite;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text cardTypeTMP;
    [SerializeField] TMP_Text descriptionTMP;
    [SerializeField] TMP_Text skillCostTMP;
    [SerializeField] Sprite cardFront;
    [SerializeField] Sprite cardBack;
    [SerializeField] Sprite[] background;
    [SerializeField] TMP_Text currentHpTMP;
    [SerializeField] TMP_Text maxHpTMP;
    [SerializeField] GameObject healthBar;
    [SerializeField] GameObject equipDurability;
    [SerializeField] Image healthbarSprite;
    [SerializeField] Image durabilitySprite;
    [SerializeField] Image[] manaGageSprites;
    [SerializeField] Image coolDownSprite;
    public SpriteRenderer characterSprite;

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

        switch (card.type)
        {
            //background
            //0:skill
            //1:demo_town
            //2:equip
            case (CardType.Player):
                backgroundSprite.sprite = background[1];
                cardTypeTMP.text = "<color=white>Player";
                return;
            case (CardType.Enemy):
                backgroundSprite.sprite = background[1];
                cardTypeTMP.text = "<color=red>Enemy";
                return;
            case (CardType.Skill):
                backgroundSprite.sprite = background[0];
                SkillCard skill = (SkillCard)card;
                skillCostTMP.text = "<color=#1E90FF>" + skill.cost.ToString();
                cardTypeTMP.text = "<color=#1E90FF>Skill";

                string skillDescription = "";
                foreach(SkillEffect skillEffect in skill.skillEffects)
                {
                    switch (skillEffect.skillType)
                    {
                        case (SkillType.PscDamage):
                            skillDescription += "적에게 <color=red>물리 피해</color>";
                            break;
                        case (SkillType.MgcDamage):
                            skillDescription += "적에게 <color=#7b68ee>마법 피해</color>";
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
                            skillDescription += "<color=#7b68ee>Int</color>/";
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
                return;
            case (CardType.Event):
                backgroundSprite.sprite = background[1];
                cardTypeTMP.text = "<color=yellow>Event";
                return;
            case CardType.Equip:
                backgroundSprite.sprite = background[2];
                cardTypeTMP.text = "<color=grey>Equip";
                return;
        }
    }

    public void SetEquipDescription(int level)
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
                    equipDescription += "<color=#1E90FF>Int+";
                    break;
            }
            int equipPow = stat.basePow + stat.PowPL * level;

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
                    EntityController.instance.PlayerUseSkill(ecard.skill,3);
                }
            }
        }
        else
            TurnManager.instance.ChangeTurnTo(GameState.PathSelection);
    }

    public void TypeDescription(string line)
    {
        descriptionTMP.text = "";
        StartCoroutine(Type(line));
    }

    IEnumerator Type(string line)
    {
        GameManager.instance.AddControlBlock();

        if (lineIndex == 0)
        {
            yield return new WaitForSeconds(0.4f);
        }

        line = line.Replace("\\n", "\n");


        if (line[0] == '<')
        {
            descriptionTMP.text += line;
            yield return new WaitForSeconds(textSpeed);
        }
        else
        {
            foreach (char c in line.ToCharArray())
            {
                descriptionTMP.text += c;
                yield return new WaitForSeconds(textSpeed);
            }
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
        popUp.gameObject.transform.DOMove(dest, 1f)
            .OnComplete(() => Destroy(popUp));
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
        manaGageSprites[0].fillAmount = actionPoints;
        manaGageSprites[1].fillAmount = actionPoints - 1;
        manaGageSprites[2].fillAmount = actionPoints - 2;
        manaGageSprites[3].fillAmount = actionPoints - 3;
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
}
