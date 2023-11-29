using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveScript_BossEnemy : WaveScript
{
    public override void Setup(int waveIndex){
        ResetEnemyInfos();

        switch(waveIndex){
            case 0:
                SetupBossEnemy();
            break;

            default:
                Debug.Log("Reached End of WaveScript");
            break;
        }

        this.waveCount = 1;
    }

    private void SetupBossEnemy(){
        this.EnemySettingsList = new List<EnemySettings>();
        EnemySettings Enemy1 = new EnemySettings();
        Enemy1.level = 1;
        Enemy1.name = "Engenia";
        Enemy1.hp = 100;
        Enemy1.battleSpeed = 1;

        Enemy1 = InitDefensiveMode(Enemy1);

        List<ShieldMode> shields = new List<ShieldMode>();
        shields.Add(ShieldMode.ANY);
        Enemy1.DefensiveModes[0].Add(shields);

        InitDiscoveredShieldsList(Enemy1, 1);

        Enemy1.AttackLibrary = new Dictionary<AbilityType, EnemyAttack>(); 

        Enemy1.minAttackSequenceLength = 0;
        Enemy1.maxAttackSequenceLength = 0;

        this.EnemySettingsList.Add(Enemy1);
    }
}
