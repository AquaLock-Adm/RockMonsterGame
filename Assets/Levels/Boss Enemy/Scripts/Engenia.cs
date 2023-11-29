using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engenia : Enemy
{
    public override void BattleSetup(){
        Debug.Log("I'm alive.");
        base.BattleSetup();
    }
}
