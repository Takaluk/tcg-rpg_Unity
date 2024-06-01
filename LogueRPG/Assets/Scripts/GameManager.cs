using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Instance
    private static GameManager m_instance;
    public static GameManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<GameManager>();
            }

            return m_instance;
        }
    }
    #endregion

    [SerializeField] TMP_Text turnCountTMP;
    [SerializeField] TMP_Text gameOverTurnCountTMP;
    [SerializeField] GameObject gameOver;
    int controlBlock = 0;
    int turnCount = 0;

    [SerializeField] GameObject cardManager;
    [SerializeField] GameObject turnManager;
    [SerializeField] GameObject battleManager;

    public void StartGame()
    {
        cardManager.SetActive(true);
        turnManager.SetActive(true);
        battleManager.SetActive(true);
    }

    public int GetEnemyLevel()
    {
        return 1 + turnCount / 2;
    }

    public int GetControlBlockCount()
    {
        return controlBlock;
    }

    public void AddControlBlock()
    {
        controlBlock++;
    }

    public void RemoveControlBlock()
    {
        controlBlock--;
    }

    public void AddTurnCount()
    {
        turnCount++;
        turnCountTMP.text = turnCount.ToString();
    }

    public int GetTurnCount()
    {
        return turnCount;
    }

    public void ShowGameOver()
    {
        gameOverTurnCountTMP.text = "You survived " + turnCount + " stages";
        gameOver.SetActive(true);
    }

    public void LoadNewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetTimeScale(float scale)
    {
        Time.timeScale = scale;
    }

    public string GetLocaleString(string key)
    {
        Locale currentLocale = LocalizationSettings.SelectedLocale;
        return LocalizationSettings.StringDatabase.GetLocalizedString("LocaleTable", key, currentLocale);
    }
}