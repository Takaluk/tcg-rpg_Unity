using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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
    [SerializeField] int enemyPenalty = 100;
    [SerializeField] GameObject gameOver;
    int controlBlock = 0;
    int turnCount = 0;

    public int GetEnemyLevel()
    {
        return turnCount / 2;
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

    public int GetEnemyPenalty()
    {
        return enemyPenalty;
    }

    public void ShowGameOver()
    {
        gameOver.SetActive(true);
    }

    public void LoadNewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}