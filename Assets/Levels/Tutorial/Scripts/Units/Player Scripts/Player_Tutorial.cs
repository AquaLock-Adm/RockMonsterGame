using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Tutorial : PlayerCharacter
{
    public int TutorialNumber = 42;

    protected override void SetupBattleComponents(){
        // Reminder: Setup Actionshandler before Player control so the start menu knows that the heatcharge is done on apr < 5
        this.ActionHandler = this.gameObject.AddComponent<PlayerAction_Tutorial>();
        this.ActionHandler.Setup(this, this.ReferencesForActionHandler);

        this.ResourceHandler = this.gameObject.AddComponent<PlayerResource_Tutorial>();
        this.ResourceHandler.Setup(this, BattleSystem.PlayerNameText, BattleSystem.PlayerHpSlider);

        this.Controls = this.gameObject.AddComponent<PlayerControl_Tutorial>();
        this.Controls.Setup(this, BattleSystem.InputDarkFilter, this.MenuTexts);
    }

    public void SetPlayerMaxActionLevel(int level){
        gameObject.GetComponent<PlayerAction_Tutorial>().SetMaxComboLevel(level);
    }
}
