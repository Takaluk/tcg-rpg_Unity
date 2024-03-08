using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class CardOnFeild : MonoBehaviour
{
    [SerializeField] SpriteRenderer cardSprite;
    [SerializeField] SpriteRenderer characterSprite;
    [SerializeField] SpriteRenderer backgroundSprite;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text descriptionTMP;
    [SerializeField] Sprite cardFront;
    [SerializeField] Sprite cardBack;
    [SerializeField] Sprite[] background;

    public Card card;
    public PRS originPRS;

    public bool isReactable = false;
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
        //종류에따라 프론트 변경
        characterSprite.sprite = card.sprite;
        backgroundSprite.sprite = background[0];
        //if turn == 10, back[1]... 턴 넘길때마다 배경이 달라짐 10층, 100층..

        string name = card.name.Replace("\\n", "\n");
        nameTMP.text = name;

        descriptionTMP.text = "defaultText";
    }    

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {

        if (useDotween)
        {
            GameManager.instance.controlBlock += 1;
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime).OnComplete(() => GameManager.instance.controlBlock -= 1);
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
        GameManager.instance.controlBlock += 1;

        Quaternion targetRotation = Quaternion.Euler(0f, -180f, 0f);

        transform.DOMove(discardPoint.position, 0.3f).OnComplete(() => GameManager.instance.controlBlock -= 1);
        transform.DORotateQuaternion(targetRotation, 0.3f);
        transform.DOScale(targetScale, 0.3f).OnComplete(() => Destroy(gameObject));
    }

    private void OnMouseDrag()
    {
        if (GameManager.instance.controlBlock > 0)
            return;

        if (isMinimized)
        {
            CardManager.instance.CardMouseDrag(this);
        }
    }

    private void OnMouseUp()
    {
        if (GameManager.instance.controlBlock > 0)
            return;

        if (isMinimized)
        {
            CardManager.instance.CardMouseUp(this);
        }
    }

    private void OnMouseDown()
    {
        if (GameManager.instance.controlBlock > 0)
            return;

        CardManager.instance.CardMouseDown(this);

        if (card.type == CardType.Event || TurnManager.instance.currentState == GameState.Event)
        {
            NextDialogue();
        }
    }

    #region Dialogue

    public void NextDialogue()
    {
        if (lineIndex < card.context.Length)
        {
            TypeDescription(card.context[lineIndex]);
            lineIndex++;
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
        GameManager.instance.controlBlock += 1;

        if (card.type == CardType.Event && lineIndex == 0)
        {
            yield return new WaitForSeconds(0.4f);
        }

        else if (card.type == CardType.Enemy && GameManager.instance.controlBlock > 1)
        {
            yield return new WaitForSeconds(0.4f * (GameManager.instance.controlBlock - 2));
        }

        line = line.Replace("\\n", "\n");

        foreach (char c in line.ToCharArray())
        {
            descriptionTMP.text += c;
            yield return new WaitForSeconds(textSpeed);
        }


        GameManager.instance.controlBlock -= 1;
    }
    #endregion
}
