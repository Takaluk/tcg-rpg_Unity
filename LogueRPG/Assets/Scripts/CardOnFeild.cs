using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardOnFeild : MonoBehaviour
{
    [SerializeField] SpriteRenderer cardSprite;
    [SerializeField] SpriteRenderer backgroundSprite;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text cardTypeTMP;
    [SerializeField] TMP_Text descriptionTMP;
    [SerializeField] Sprite cardFront;
    [SerializeField] Sprite cardBack;
    [SerializeField] Sprite[] background;
    [SerializeField] GameObject healthBar;
    [SerializeField] Image healthbarSprite;
    [SerializeField] Image[] manaGageSprites;
    public SpriteRenderer characterSprite;

    public Card card;
    public PRS originPRS;

    public int lineIndex = 0;
    public bool isMinimized = false;
    public float textSpeed = 0.3f;

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
            
            case (CardType.Enemy):
                backgroundSprite.sprite = background[1];
                cardTypeTMP.text = "<color=red>Enemy";
                return;
            case (CardType.Skill):
                backgroundSprite.sprite = background[0];
                cardTypeTMP.text = "<color=blue>Skill";
                return;
            case (CardType.Event):
                backgroundSprite.sprite = background[1];
                cardTypeTMP.text = "<color=yellow>Event";
                return;
        }
        descriptionTMP.text = "defaultText";
    }    

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {

        if (useDotween)
        {
            GameManager.instance.AddControlBlock();
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime).OnComplete(() => GameManager.instance.RemoveControlBlock());
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

        transform.DOMove(discardPoint.position, 0.299f).OnComplete(() => GameManager.instance.RemoveControlBlock());
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
                    EntityController.instance.PlayerUseSkill(ecard.skill);
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

    public void ShowHealthbar(bool on)
    {
        healthBar.SetActive(on);
    }

    public void UpdateHealthbar(float maxHealth, float currentHealth)
    {
        float healthbarAmount = currentHealth / maxHealth;
        healthbarSprite.fillAmount = healthbarAmount;
    }

    public void UpdateApGage(float maxHealth)
    {
        manaGageSprites[0].fillAmount = maxHealth;
        manaGageSprites[1].fillAmount = maxHealth - 1;
        manaGageSprites[2].fillAmount = maxHealth - 2;
        manaGageSprites[3].fillAmount = maxHealth - 3;
    }
}
