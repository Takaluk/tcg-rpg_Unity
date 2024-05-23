using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEnableAnimation : MonoBehaviour
{
    private RectTransform originTransform;

    public void ShowUI(bool on)
    {
        if (on)
        {
            if (!gameObject.activeSelf)
                Enable();
        }
        else
            Disable();
    }

    void Enable()
    {
        gameObject.SetActive(true);
        originTransform = GetComponent<RectTransform>();
        originTransform.DOAnchorPosX(originTransform.anchoredPosition.x - 30f, 0.3f);
    }

    void Disable()
    {
        originTransform = GetComponent<RectTransform>();
        originTransform.DOAnchorPosX(originTransform.anchoredPosition.x + 30f, 0.3f).OnComplete(() => gameObject.SetActive(false));
    }
}