using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_Tutorial : Enemy
{
    [SerializeField] private int tutorialEnemyIndex;
    public override async Task Death(){
        if(!this.deathTriggered){
            this.deathTriggered = true;
            BattleSystem.state = BattleState.ENEMYDIED;
            UpdateEnemyHUD();
            while(this.displayingDamageTextCount > 0 || this.heldByCombo){
                await Task.Yield();
            }
            await Task.Delay(this.timeAfterDeath);
            
            // Load Dialogue with enemy index
            
            // BattleSystem.EnemyDied(this);
            Destroy(this.gameObject);
        }
    }
}
