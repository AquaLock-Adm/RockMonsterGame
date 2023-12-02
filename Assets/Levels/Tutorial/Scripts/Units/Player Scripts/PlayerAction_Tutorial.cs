using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerAction_Tutorial : PlayerActionHandler
{
    [Header("Tutorial Only!")]
    public bool checkForExecutedActions = false;
    public bool checkForQueuedActions = false;

    public override async void StartActionQueue(){
        if(this.checkForExecutedActions) gameObject.GetComponent<Player_Tutorial>().UpdateAwaitedActions(this.Actions);

        this.inAttackRushPreLoop = false;
        if(Player.state == PlayerState.START) {
            this.heatChargeDone = true;
        }
        Player.state = PlayerState.QUEUE;
        await ExecuteAllActions();
    }

    public override void AddAction(Action A){
        this.ActionText.text = A.name;

        this.Actions.Add(A);

        if(this.checkForQueuedActions) gameObject.GetComponent<Player_Tutorial>().UpdateAwaitedActions(this.Actions);

        A.QueueAction(this);

        UpdateVisualizer();
        if(PositionsLeftInActionQueue() == 1){
            Player.LoadComboAbilityMenu();
        }else if(PositionsLeftInActionQueue() == 0){
            Player.LoadRoundOverMenu();
        }else{
            Player.LoadMainMenu();
        }
    }

    public void SetMaxComboLevel(int level){
        this.maxComboLv = level;
        this.currentMaxAttackLength = level;

        if(this.comboLevel > level) this.comboLevel = level;

        if(this.comboLevel >= this.maxComboLv){
            this.ComboLevelText.text = "Lv.MAX";
        }else this.ComboLevelText.text = "Lv."+comboLevel.ToString();
        
        UpdateHeatBar();
        UpdateActionBoxList();
        UpdateVisualizer();
    }

    public void SetComboLevel(int level){
        if(level > this.maxComboLv) {
            Debug.LogError("Tried to set ComboLevel higher then max!");
            return;
        }

        this.comboLevel = level;

        if(this.comboLevel >= this.maxComboLv){
            this.ComboLevelText.text = "Lv.MAX";
        }else this.ComboLevelText.text = "Lv."+comboLevel.ToString();
        
        UpdateHeatBar();
        UpdateActionBoxList();
        UpdateVisualizer();
    }
}
