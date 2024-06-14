using System;
using System.Drawing;
using System.Linq;
using TMPro;
using UnityEditor.Localization.Plugins.XLIFF.V20;
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
        entityStatusText += "<b><size=0.7>" + GameManager.instance.GetLocaleString(entity.entityCard.card.name) + "</size></b>\n";
        entityStatusText += "\n";

        if (true)
        {
            entityStatusText += "<b><size=0.55>" + GameManager.instance.GetLocaleString("Battle-Stat-Status") + "</size></b>\n";
            foreach (EntityStat stat in Enum.GetValues(typeof(EntityStat)))
            {
                int pow = entity.entityStat[stat];
                if (pow != 0)
                {
                    switch (stat)
                    {
                        case EntityStat.Critical:
                            if (pow > 100)
                                pow = 100;
                            if (pow < 0)
                                pow = 0;
                            break;
                        case EntityStat.Dodge:
                            if (pow > 90)
                                pow = 90;
                            break;
                    }
                    entityStatusText += "<link=" + stat.ToString() + ">" + Utils.GetStatName(stat) + ": " + pow.ToString() + "</link>\n";
                }
            }
            entityStatusText += "\n";
        }

        entityStatusText += "<b><size=0.55>" + GameManager.instance.GetLocaleString("Battle-Stat-Buff") + "</size></b>\n";
        foreach (Buff buff in entity.buffList)
        {
            entityStatusText += "<link=" + buff.stat.ToString() + ">" + Utils.GetStatName(buff.stat) + " (" + Utils.color_buff + "+" + buff.pow + "</color>/" + (int)buff.currentTime + "s)</link>\n";
        }
        entityStatusText += "\n";

        entityStatusText += "<b><size=0.55>" + GameManager.instance.GetLocaleString("Battle-Stat-Debuff") + "</size></b>\n";
        foreach (Buff buff in entity.debuffList)
        {
            entityStatusText += "<link=" + buff.stat.ToString() + ">" + Utils.GetStatName(buff.stat) + " (" + Utils.color_debuff + "-" + buff.pow + "</color>/" + (int)buff.currentTime + "s)</link>\n";
        }

        entityStatusTMP.text += entityStatusText;

        GameManager.instance.SetTimeScale(0f);
    }

    public void PopUpStatusDescription(string key)
    {
        statusDescriptionPanel.SetActive(true);
        var data = stats.FirstOrDefault(t => t.Key == key);

        statNameTMP.text = GameManager.instance.GetLocaleString(data.Name);
        statDescriptionTMP.text = GameManager.instance.GetLocaleString(data.Description);
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
