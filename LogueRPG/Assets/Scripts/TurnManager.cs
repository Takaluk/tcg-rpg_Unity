using UnityEngine;

public enum GameState
{
    PathSelection = 0,
    Battle,
    Event,
    Reward,
    PlayerCharacterSelection,
    Stage
};

public enum StageType
{
    Wood,
    Town,
    Castle
}

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
    public StageType currentStage;

    private void Start()
    {
        currentState = GameState.PlayerCharacterSelection;
    }
                                    
    public void ChangeStageTo(StageType stage)
    {
        currentStage = stage;

        CardManager.instance.SetStageData(stage);
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
                CardManager.instance.ShowInventoryUI(true);
                return;

            case GameState.Battle:
                currentState = GameState.Battle;
                CardManager.instance.ShowInventoryUI(false);
                EntityController.instance.StartBattle();
                CardManager.instance.SetBattlePosition(true);
                CardManager.instance.ShowSkillCards();
                return;

            case GameState.Event:
                currentState = GameState.Event;
                CardManager.instance.ShowInventoryUI(true);
                CardManager.instance.SetBattlePosition(false);
                return;

            case GameState.Reward:
                currentState = GameState.Reward;
                CardManager.instance.ShowInventoryUI(false);
                CardManager.instance.ShowRewardAcceptUI(true);
                return;

            case GameState.Stage:
                currentState = GameState.Stage;
                CardManager.instance.ShowInventoryUI(false);
                CardManager.instance.ShowRewardAcceptUI(true);
                return;

            case GameState.PlayerCharacterSelection:
                currentState = GameState.PlayerCharacterSelection;
                CardManager.instance.ShowPlayerSelection();
                return;

            default:
                Debug.LogError("Unknown state");
                return;
        }
    }


}
