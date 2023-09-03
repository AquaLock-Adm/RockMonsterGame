using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu_Tutorial : PauseMenu
{
    public override void Setup(BattleSystem BS){
        this.BattleSystem = BS;
        ShowPauseMenu(false);
    }

    public override void ShowPauseMenu(bool turnOn){
        if(!Application.isPlaying) return;
        // Debug.Log("ShowPauseMenu() called");
        this.DarkFilter.SetActive(turnOn);

        if(turnOn){
            switch(this.BattleSystem.state){
                case BattleState.SETUP:
                    this.StartMenuScreen.SetActive(turnOn);
                    this.PauseMenuScreen.SetActive(false);
                    SetupStartButtonTest();
                break;
                case BattleState.WAVEOVER:
                    this.PauseMenuScreen.SetActive(turnOn);
                    if(!this.BattleSystem.inBossWave){
                        LoadText(this.waveOverText);
                        this.showContinueButton = true;
                        this.showLeaveButton = true;
                        this.showNextStageButton = false;
                    }else{
                        if(BattleSystem.finalStageReached){
                            LoadText(this.gameWonText);
                            this.showLeaveButton = true;
                            this.showContinueButton = false;
                            this.showNextStageButton = false;
                        }else if(BattleSystem.afterFinalWave){
                            LoadText(this.afterBossWaveText);
                            this.showLeaveButton = true;
                            this.showContinueButton = true;
                            this.showNextStageButton = true;
                        }else {
                            LoadText(this.stageOverText);
                            this.showLeaveButton = true;
                            this.showContinueButton = true;  
                            this.showNextStageButton = true;
                        }
                    }
                break;
                case BattleState.PLAYERDIED:
                    this.PauseMenuScreen.SetActive(turnOn);
                    LoadText(this.gameOverText);
                    this.showContinueButton = false;
                    this.showLeaveButton = true; 
                    this.showNextStageButton = false;
                break;
                default:
                    Debug.LogError("Unexpected Pause State!");
                    Debug.Log(this.BattleSystem.state);
                break;
            }

            this.ContinueButton.SetActive(this.showContinueButton);
            this.LeaveButton.SetActive(this.showLeaveButton); 
            this.NextStageButton.SetActive(this.showNextStageButton);
            if(BattleSystem.state == BattleState.WAVEOVER){
                this.NextWaveInfo.SetActive(false);
                this.PauseHpSlider.gameObject.SetActive(false); 
            }else{
                this.NextWaveInfo.SetActive(false);
                this.PauseHpSlider.gameObject.SetActive(false);
            }     
        } else this.PauseMenuScreen.SetActive(turnOn);
    }
}
