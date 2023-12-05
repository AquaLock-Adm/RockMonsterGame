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
        EnemySettings Eng_Settings = new EnemySettings();
        Eng_Settings.level = 1;
        Eng_Settings.name = "Engenia";
        Eng_Settings.hp = 100;
        // Eng_Settings.battleSpeed = 5;
        Eng_Settings.battleSpeed = 1;

        Eng_Settings.InitDefensiveMode();

        List<ShieldMode> shields = new List<ShieldMode>();
        shields.Add(ShieldMode.ANY);

        Eng_Settings.DefensiveModes[0].Add(shields);

        Eng_Settings.InitDiscoveredShieldsList();

        Eng_Settings.InitAttackLibrary(1,1,3);

        this.EnemySettingsList.Add(Eng_Settings);
    }
}
