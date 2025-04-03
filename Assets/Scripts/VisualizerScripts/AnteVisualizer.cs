using System;
using System.Collections.Generic;
using UnityEngine;

public class AnteVisualizer : MonoBehaviour
{
    [SerializeField] private RoundButton buttonPrefab;
    [SerializeField] private Transform buttonContainer;
    private RunManager _runManager;
    private void Start()
    {
        _runManager = RunManager.Instance;

        if (_runManager)
        {
            _runManager.StartAnteEvent.AddListener(DisplayAnteRounds);
        }
    }

    private void DisplayAnteRounds(AnteManager.Ante ante)
    {
        Debug.LogWarning("Displaying ante");
        // Clear existing buttons
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create a button for each round
        foreach (var round in ante.rounds)
        {
            var newButton = Instantiate(buttonPrefab, buttonContainer);
            newButton.Setup(round);
        }
    }
}
