using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BattleSystem_Tutorial : BattleSystem
{
    [Header("Tutorial References")]
    [SerializeField] private TutorialHandler TutorialHandler;

    protected override void PreStartActions(){ // Changed in: NBS
        RunTests();
        TutorialHandler.Setup(this);
        TutorialHandler.Continue();
    }
}
