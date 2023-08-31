using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleSystem_Tutorial : BattleSystem
{
    [Header("Tutorial References")]
    [SerializeField] private TutorialHandler TutorialHandler;

    protected override void PreStartActions(){
        RunTests();
        TutorialHandler.Setup(this);
        TutorialHandler.Continue();
    }

    public void BlockPlayerBattleModeSwitch(bool blockOn){
        this.Player.GetComponent<Player_Tutorial>().blockBattleModeSwitch = blockOn;
    }

    public void SetPlayerMaxActionLevel(int level){
        this.Player.GetComponent<Player_Tutorial>().SetPlayerMaxActionLevel(level);
    }

    public void SetPlayerComboLevel(int level){
        this.Player.GetComponent<Player_Tutorial>().SetPlayerComboLevel(level);
    }

    public void WaitForExecutedActions(bool on){
        this.Player.GetComponent<Player_Tutorial>().WaitForExecutedActions(on);
    }

    public void UpdateAwaitedActions(List<Action> ActionsList){
        TutorialHandler.UpdateAwaitedActions(ActionsList);
    }

    public void WaitForQueuedActions(bool on){
        this.Player.GetComponent<Player_Tutorial>().WaitForQueuedActions(on);
    }

    protected override void UpdateStageClearRewards(){
        // if(!this.useWaveScript){
        //     if(!this.stagesClearedBefore[this.WaveRandomizer.stageIndex]){
        //         this.earnedCredits += this.stageFirstClearRewards[this.WaveRandomizer.stageIndex];
        //         this.ResultHandler.ScoreText.text = this.ResultHandler.FormatIntegerCount(this.earnedCredits) + " Cd";
        //         this.stagesClearedBefore[this.WaveRandomizer.stageIndex] = true;
        //     }
        // }else Debug.Log("Not implemented yet!");
    }
}
