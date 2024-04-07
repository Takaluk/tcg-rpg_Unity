using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderOrder : MonoBehaviour
{
    [SerializeField] Renderer[] renderers;
    [SerializeField] string sortingLayerName;
    [SerializeField] Canvas CardStatusCanvas;
    int originOrder;

    public void SetOriginOrder(int originOrder)
    {
        this.originOrder = originOrder;
        SetOrder(originOrder);
    }

    public void setMostFrontOrder(bool isMostFront)
    {
        SetOrder(isMostFront ? 100 : originOrder);
    }

    public void SetOrder(int order)
    {
        int mulOrder = order * 10;

        foreach (var renderer in renderers)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = mulOrder + 1;
        }

        CardStatusCanvas.sortingOrder = mulOrder + 1;
    }
}
