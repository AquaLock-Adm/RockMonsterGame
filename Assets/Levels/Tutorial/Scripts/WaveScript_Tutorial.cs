using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveScript_Tutorial : WaveScript
{

    public override void Setup(int waveIndex){
        ResetEnemyInfos();

        switch(waveIndex){
            case 0:
                SetupTutorialEnemies();
            break;

            default:
                Debug.Log("Reached End of WaveScript");
            break;
        }

        this.waveCount = 1;
    }

    private void SetupTutorialEnemies(){
        this.EnemySettingsList = new List<EnemySettings>();
        SetupEnemy1();
        SetupEnemy2();
        SetupEnemy3();
    }

    private void SetupEnemy1(){
        EnemySettings Enemy1 = new EnemySettings();
        Enemy1.level = 1;
        Enemy1.name = "Enemy A";
        Enemy1.hp = 1;

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

    private void SetupEnemy2(){
        EnemySettings Enemy2 = new EnemySettings();
        Enemy2.level = 1;
        Enemy2.name = "Enemy B";
        Enemy2.hp = 1;

        Enemy2 = InitDefensiveMode(Enemy2);

        List<ShieldMode> shields = new List<ShieldMode>();
        shields.Add(ShieldMode.LIGHT);
        shields.Add(ShieldMode.HEAVY);
        Enemy2.DefensiveModes[0].Add(shields);

        InitDiscoveredShieldsList(Enemy2, shields.Count);

        Enemy2.AttackLibrary = new Dictionary<AbilityType, EnemyAttack>(); 

        Enemy2.minAttackSequenceLength = 0;
        Enemy2.maxAttackSequenceLength = 0;

        this.EnemySettingsList.Add(Enemy2);
    }

    private void SetupEnemy3(){
        EnemySettings Enemy3 = new EnemySettings();
        Enemy3.level = 1;
        Enemy3.name = "Enemy C";
        Enemy3.hp = 1;

        Enemy3 = InitDefensiveMode(Enemy3);

        List<ShieldMode> shields = new List<ShieldMode>();
        shields.Add(ShieldMode.ANY);
        shields.Add(ShieldMode.SPECIAL);
        Enemy3.DefensiveModes[0].Add(shields);

        InitDiscoveredShieldsList(Enemy3, shields.Count);

        Enemy3.AttackLibrary = new Dictionary<AbilityType, EnemyAttack>(); 

        Enemy3.minAttackSequenceLength = 0;
        Enemy3.maxAttackSequenceLength = 0;

        this.EnemySettingsList.Add(Enemy3);
    }

    private EnemySettings InitDefensiveMode(EnemySettings Settings){
        Settings.DefensiveModes[0] = new List<List<ShieldMode>>();
        Settings.DefensiveModes[1] = new List<List<ShieldMode>>();

        return Settings;
    }

    private EnemySettings InitDiscoveredShieldsList(EnemySettings Settings,int listCount){
        for(int i=0;i<listCount;i++){
            Settings.DefensiveModes[1].Add(new List<ShieldMode>());
        }
        return Settings;
    }
}
