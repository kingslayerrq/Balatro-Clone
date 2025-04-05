using System;
using System.Collections.Generic;
using UnityEngine;

public class AnteVisualizer : MonoBehaviour
{
    public static AnteVisualizer Instance;
    private RunManager _runManager;
    
    [SerializeField] private RoundButton buttonPrefab;
    [SerializeField] private Transform buttonContainer;

    public RoundButton[] RoundButtons = new RoundButton[3];
    private bool isButtonInit = false;
    
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        _runManager = RunManager.Instance;

        if (_runManager)
        {
            _runManager.StartAnteEvent.AddListener(DisplayAnteRounds);
        }
        
        RoundButton.SelectRoundEvent.AddListener(HideAnteRounds);
    }

    public void BackBtn()
    {
        DisplayAnteRounds(_runManager.CurAnte);
    }

    private void DisplayAnteRounds(AnteManager.Ante ante)
    {
        if (ante == null) return;
        if (ante.rounds.Count != 3) return;
        Debug.LogWarning("Displaying ante");
        if (!isButtonInit)
        {
            // Create a button for each round
            for (int i = 0; i < 3; i++ )
            {
                var newButton = Instantiate(buttonPrefab, buttonContainer);
                newButton.Setup(ante.rounds[i]);
                RoundButtons[i] = newButton;
            }
            isButtonInit = true;
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                var button = RoundButtons[i];
                var isComplete = _runManager.CurAnte.rounds[i].isComplete;
                var isSkip = _runManager.CurAnte.rounds[i].isSkipped;
                if (isComplete)
                {
                    button.playButton.interactable = false;
                }

                if (isSkip)
                {
                    button.skipButton.interactable = false;
                }
                button.gameObject.SetActive(true);
            }
        }
    }

    private void HideAnteRounds(Round round)
    {
        foreach (var button in RoundButtons)
        {
            button.gameObject.SetActive(false);
        }
    }
}
