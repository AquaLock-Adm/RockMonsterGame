using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Tutorial : PlayerCharacter
{
    protected override void SetupBattleComponents(){
        // Reminder: Setup Actionshandler before Player control so the start menu knows that the heatcharge is done on apr < 5
        this.ActionHandler = this.gameObject.AddComponent<PlayerActionHandler>();
        this.ActionHandler.Setup(this, this.ReferencesForActionHandler);

        this.ResourceHandler = this.gameObject.AddComponent<PlayerBattleResourceHandler>();
        this.ResourceHandler.Setup(this, BattleSystem.PlayerNameText, BattleSystem.PlayerHpSlider);

        this.Controls = this.gameObject.AddComponent<PlayerControl_Tutorial>();
        this.Controls.Setup(this, BattleSystem.InputDarkFilter, this.MenuTexts);
    }
}
