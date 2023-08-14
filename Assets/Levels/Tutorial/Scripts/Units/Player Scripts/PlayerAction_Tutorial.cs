using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerAction_Tutorial : PlayerActionHandler
{
    protected override void ActionQueueEnd(){
        ClearActionQueue();
        UpdateActionBoxList();

        if(Player.state == PlayerState.QUEUE){
            Player.state = PlayerState.PLAYERTURN;
            // Player.SwitchBattleModes();
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
}
