using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMenuHandler_Tutorial : BattleMenuHandler
{
    protected override void CheckInputs(){
        switch(this.BattleSystem.state){

            case BattleState.RESULT: // <<----- KEEP
                this.InputDarkFilter.SetActive(false);
                if(Input.GetKeyDown(KeyCode.A) && this.BattleSystem.ResultHandler.finishedDisplayingResult) this.BattleSystem.End();
                // else if(Input.GetKeyDown(KeyCode.D) && this.PauseMenu.showLeaveButton) PauseMenu.LeaveGame();
            break;

            case BattleState.WAVEOVER: // <<----- KEEP
                this.InputDarkFilter.SetActive(false);
                WaveOverInputs();
            break;

            case BattleState.PLAYERDIED: // <<----- KEEP
                this.InputDarkFilter.SetActive(false);
                if(Input.GetKeyDown(KeyCode.D)) this.PauseMenu.LeaveGame();
            break;

            default:
                this.InputDarkFilter.SetActive(true);
            break;
        }
    }  
}
