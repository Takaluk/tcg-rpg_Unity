using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableText : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log("clicked");
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Utils.MousePos, null);
        Debug.Log(linkIndex);
        if (linkIndex > -1)
        {
            var linkInfo = text.textInfo.linkInfo[linkIndex];
            var linkId = linkInfo.GetLinkID();

            StatusDataController.instance.PopUpStatusDescription(linkId);
        }
    }
}
