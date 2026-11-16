using UnityEngine;

/// <summary>
/// Shows one button per blind of the current ante and forwards the player's choice.
/// </summary>
public class AnteVisualizer : MonoBehaviour
{
    public static AnteVisualizer Instance;

    private RunManager _runManager;
    private RoundManager _roundManager;
    private AnteManager _anteManager;

    [SerializeField] private RoundButton buttonPrefab;
    [SerializeField] private Transform buttonContainer;
    public RoundButton[] RoundButtons = new RoundButton[3];

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _runManager = RunManager.Instance;
        _roundManager = RoundManager.Instance;
        _anteManager = AnteManager.Instance;
        if (!_runManager || !_roundManager || !_anteManager)
        {
            Debug.LogError("AnteVisualizer: RunManager, RoundManager or AnteManager is missing from the scene", this);
            return;
        }

        _runManager.AnteDisplayRequested += DisplayAnteRounds;
    }

    private void OnDestroy()
    {
        if (_runManager) _runManager.AnteDisplayRequested -= DisplayAnteRounds;
        foreach (var button in RoundButtons)
        {
            if (!button) continue;
            button.PlayClicked -= OnPlayClicked;
            button.SkipClicked -= OnSkipClicked;
        }
        if (Instance == this) Instance = null;
    }

    public void BackBtn()
    {
        if (_runManager) DisplayAnteRounds(_runManager.CurAnte);
    }

    private void DisplayAnteRounds(AnteManager.Ante ante)
    {
        if (ante == null) return;
        if (ante.rounds.Count != RoundButtons.Length) return;

        // blinds are played in order, and only between rounds of a live run
        int current = ante.CurrentRoundIndex;
        bool canChoose = _runManager.IsRunActive && !_roundManager.IsRoundActive;

        for (int i = 0; i < RoundButtons.Length; i++)
        {
            if (RoundButtons[i] == null)
            {
                // Create a button for each round
                var newButton = Instantiate(buttonPrefab, buttonContainer);
                newButton.PlayClicked += OnPlayClicked;
                newButton.SkipClicked += OnSkipClicked;
                RoundButtons[i] = newButton;
            }

            var button = RoundButtons[i];
            var round = ante.rounds[i];
            bool isCurrent = canChoose && i == current;

            button.Setup(round);
            button.playButton.interactable = isCurrent;
            if (button.skipButton)
            {
                // a boss blind cannot be skipped
                button.skipButton.interactable = isCurrent && round.blind.type != Enums.BlindType.BossBlind;
            }
            button.gameObject.SetActive(true);
        }
    }

    private void HideAnteRounds()
    {
        foreach (var button in RoundButtons)
        {
            if (button) button.gameObject.SetActive(false);
        }
    }

    private void OnPlayClicked(Round round)
    {
        HideAnteRounds();
        _anteManager.SelectRound(round);
    }

    private void OnSkipClicked(Round round)
    {
        _anteManager.SkipRound(round);
    }
}
