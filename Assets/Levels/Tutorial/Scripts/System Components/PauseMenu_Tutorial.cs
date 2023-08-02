using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu_Tutorial : PauseMenu
{
    public override void Setup(BattleSystem BS){
        this.BattleSystem = BS;
        ShowPauseMenu(false);
    }
}
