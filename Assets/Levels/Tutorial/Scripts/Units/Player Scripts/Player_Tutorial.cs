using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Tutorial : PlayerCharacter
{
    public bool blockBattleModeSwitch = false;

    protected override void SetupBattleComponents(){
        // Reminder: Setup Actionshandler before Player control so the start menu knows that the heatcharge is done on apr < 5
        this.ActionHandler = this.gameObject.AddComponent<PlayerAction_Tutorial>();
        this.ActionHandler.Setup(this, this.ReferencesForActionHandler);

        this.ResourceHandler = this.gameObject.AddComponent<PlayerResource_Tutorial>();
        this.ResourceHandler.Setup(this, BattleSystem.PlayerNameText, BattleSystem.PlayerHpSlider);

        this.Controls = this.gameObject.AddComponent<PlayerControl_Tutorial>();
        this.Controls.Setup(this, BattleSystem.InputDarkFilter, this.MenuTexts);
    }

    public override void SwitchBattleModes(){
        if(!this.blockBattleModeSwitch){
            this.defendModeActive = !this.defendModeActive;

            BattleSystem.Enemy.SwitchModes(this.defendModeActive);
            this.ActionHandler.SwitchModes(this.defendModeActive);
            this.Controls.LoadMainMenu();
        }
    }

    public void SetPlayerMaxActionLevel(int level){
        gameObject.GetComponent<PlayerAction_Tutorial>().SetMaxComboLevel(level);
    }

    public void SetPlayerComboLevel(int level){
        gameObject.GetComponent<PlayerAction_Tutorial>().SetComboLevel(level);
    }

    public void WaitForExecutedActions(bool on){
        gameObject.GetComponent<PlayerAction_Tutorial>().checkForExecutedActions = on;
    }

    public void WaitForQueuedActions(bool on){
        gameObject.GetComponent<PlayerAction_Tutorial>().checkForQueuedActions = on;
    }

    public void UpdateAwaitedActions(List<Action> ActionsList){
        BattleSystem.gameObject.GetComponent<BattleSystem_Tutorial>().UpdateAwaitedActions(ActionsList);
    }
}
