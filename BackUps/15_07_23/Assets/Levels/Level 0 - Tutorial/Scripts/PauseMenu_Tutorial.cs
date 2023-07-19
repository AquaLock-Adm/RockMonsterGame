using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu_Tutorial : PauseMenu
{
    [SerializeField] private DialogueHandler_Tutorial DialogueHandler;
    public override void ContinueGame(){
        if(BattleSystem.state == BattleState.SETUP){
            ShowPauseMenu(false);
            this.StartMenuScreen.SetActive(false);
            DialogueHandler.LoadDialogue();
        }else if(BattleSystem.state == BattleState.WAVEOVER){
            Debug.LogError("Level 0 - Tutorial shouldnt need this!");
        }else{
            Debug.LogError("Level 0 - Tutorial shouldnt need this!");
        }
    }

    public override void LeaveGame(){
        Debug.Log("Tutorial Over");
    }
}
