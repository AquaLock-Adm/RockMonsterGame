using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner_BossEnemy : EnemySpawner
{
    [Header("BossEnemy Scene")]
    [SerializeField] private GameObject BossEnemyPrefab;

    public override void SpawnNextEnemy(){
        if(this.enemyIndex >= this.EnemySettingsList.Count){
            BattleSystem.WaveOver();
            return;
        }

        GameObject BossEnemy_GO = Instantiate(this.BossEnemyPrefab, this.MainCanvas.transform);
        Engenia BigE = BossEnemy_GO.GetComponent<Engenia>();
        BossEnemy_GO.name = "Boss";

        BigE.BattleSetup();
        this.enemyIndex++;

        // EnemySettings Settings;

        // Settings = BattleSystem.GetEnemySettingsByName(this.EnemySettingsList[this.enemyIndex].name, this.EnemySettingsList[this.enemyIndex].level-1);
        
        // CurrentEnemy.CopyFromEnemySettings(Settings);

        // CurrentEnemy.BattleSetup();
        // // CurrentEnemy.PrintStatus();

        // BattleSystem.Enemy = CurrentEnemy;
        // this.enemyIndex++;
    }
}
