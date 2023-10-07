using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Gameplay gameplay;
    public DialogBox main, inGame;
    private void Update()
    {
        main.SetActiveBox(gameplay.isPlaying ? 1 : 0);
        inGame.SetActiveBox((gameplay.isPlayerPaused && !gameplay.isInventoryOpen) && gameplay.isPlaying ? 2 : (gameplay.isPlaying && gameplay.isInventoryOpen ? 1 : 0));
    }
}
