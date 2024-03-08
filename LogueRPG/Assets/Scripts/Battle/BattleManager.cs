using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    None = 0,
    Enemy,
    Player
}

public class BattleManager : MonoBehaviour
{
    #region Instance
    private static BattleManager m_instance;
    public static BattleManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<BattleManager>();
            }

            return m_instance;
        }
    }
    #endregion

    

    public BattleState battleState;

    public void SetBattle()
    {
        
    }
}
