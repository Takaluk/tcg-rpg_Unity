using UnityEngine;

public enum GameState
{
    PathSelection = 0,
    Battle,
    Event,
    Reward
};

public class TurnManager : MonoBehaviour
{
    #region Instance
    private static TurnManager m_instance;
    public static TurnManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<TurnManager>();
            }

            return m_instance;
        }
    }
    #endregion
    public GameState currentState;

    private void Start()
    {
        currentState = GameState.PathSelection;
        ChangeTurnTo(currentState);
    }

    public void ChangeTurnTo(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.PathSelection:
                currentState = GameState.PathSelection;
                GameManager.instance.AddTurnCount();
                CardManager.instance.EmptyHand();
                CardManager.instance.SetBattlePosition(false);
                CardManager.instance.ShowMainCardSelection();
                return;

            case GameState.Battle:
                currentState = GameState.Battle;
                EntityController.instance.StartBattle();
                CardManager.instance.SetBattlePosition(true);
                CardManager.instance.ShowSkillCards();
                return;

            case GameState.Event:
                currentState = GameState.Event;
                CardManager.instance.EmptyHand();
                CardManager.instance.SetBattlePosition(false);
                return;

            case GameState.Reward:
                currentState = GameState.Reward;
                CardManager.instance.EmptyHand();
                CardManager.instance.SetBattlePosition(false);

                EntityController.instance.BattleReward();
                return;

            default:
                Debug.LogError("Unknown state");
                break;
        }
    }


}
