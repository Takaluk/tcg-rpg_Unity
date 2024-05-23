using System;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEngine;

public class StatusDataController : MonoBehaviour
{
    [SerializeField] GameObject entityStatusPanel;
    [SerializeField] TMP_Text entityStatusTMP;

    [SerializeField] StatusData[] stats;
    [SerializeField] GameObject statusDescriptionPanel;
    [SerializeField] TMP_Text statNameTMP;
    [SerializeField] TMP_Text statDescriptionTMP;
    [SerializeField] SpriteRenderer statSprite;
    #region Instance
    private static StatusDataController m_instance;
    public static StatusDataController instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<StatusDataController>();
            }

            return m_instance;
        }
    }
    #endregion

    public void PopupEntityStatus(Entity entity)
    {
        if (GameManager.instance.GetControlBlockCount() > 0)
            return;

        entityStatusPanel.SetActive(true);
        entityStatusTMP.text = "";
        string entityStatusText = "";
        entityStatusText += "<b><size=0.7>" + entity.entityCard.card.name + "</size></b>\n";
        entityStatusText += "\n";
        entityStatusText += "<b>Stat</b>\n";
        foreach (EntityStat stat in Enum.GetValues(typeof(EntityStat)))
        {
            if (entity.entityStat[stat] != 0)
            {
                entityStatusText += "<u><link=" + stat.ToString() + ">" + Utils.GetStatColor(stat) + "</link></u>: " + entity.entityStat[stat].ToString() + "\n";
            }
        }

        entityStatusText += "\n";
        entityStatusText += "<b>Buff</b>\n";
        foreach (Buff buff in entity.buffList)
        {
            entityStatusText += "<u><link=" + buff.stat.ToString() + ">" + Utils.GetStatColor(buff.stat) + "</link></u> (" + Utils.color_buff + "+" + buff.pow + "</color>/" + (int)buff.currentTime + "s)\n";
        }

        entityStatusText += "\n";
        entityStatusText += "<b>Debuff</b>\n";
        foreach (Buff buff in entity.debuffList)
        {
            entityStatusText += "<u><link=" + buff.stat.ToString() + ">" + Utils.GetStatColor(buff.stat) + "</link></u> (" + Utils.color_debuff + "-" + buff.pow + "</color>/" + (int)buff.currentTime + "s)\n";
        }

        entityStatusTMP.text += entityStatusText;

        GameManager.instance.SetTimeScale(0f);
    }

    public void PopUpStatusDescription(string key)
    {
        statusDescriptionPanel.SetActive(true);
        var data = stats.FirstOrDefault(t => t.Key == key);

        statNameTMP.text = data.Name;
        statDescriptionTMP.text = data.Description;
        //statSprite.sprite = data.sprite;
    }

    public void CloseEntityStatus()
    {
        GameManager.instance.SetTimeScale(1f);
        entityStatusPanel.SetActive(false);
    }

    public void CloseStatusDescription()
    {
        statusDescriptionPanel.SetActive(false);
    }
}

[Serializable]
public struct StatusData
{
    public string Key;
    public string Name;
    public string Description;
    public Sprite sprite;
}
