using UnityEngine;

public enum GameState
{
    PathSelection = 0,
    Battle,
    Event
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
                CardManager.instance.ShowSkillCards();
                EntityController.instance.StartBattle();
                CardManager.instance.SetBattlePosition(true);
                return;

            case GameState.Event:
                currentState = GameState.Event;
                CardManager.instance.EmptyHand();
                CardManager.instance.SetBattlePosition(false);
                return;

            //case GameState.Reward:
                //currentState = GameState.Reward;
                //���� battle, event ī�忡�� ������ ������ ��, reward ���������� �����ֱ�
                //or reward �Ŵ���?
                //return;
            default:
                Debug.LogError("Unknown state");
                break;
        }
    }


}
