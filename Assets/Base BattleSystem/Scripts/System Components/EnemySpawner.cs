using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{   
    private BattleSystem BattleSystem;
    private GameObject MainCanvas;
    private int enemyIndex = 0;

    [Header("Enemy Spawn Data")]
    [SerializeField] private List<int> EnemyLevels = new List<int>();

    [SerializeField] private List<EnemySettings> EnemySettingsList = new List<EnemySettings>();
    [SerializeField] private GameObject BaseEnemyPrefab;

    [SerializeField] private int waveSize;

    public void Setup(List<EnemySettings> enemySettingsList){
        if(this.BattleSystem == null) this.BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        if(this.MainCanvas == null) this.MainCanvas = GameObject.Find("Main");

        this.enemyIndex = 0;

        this.EnemySettingsList = enemySettingsList;

        this.waveSize = enemySettingsList.Count;
    }

    public void SpawnNextEnemy(){
        if(this.enemyIndex >= this.EnemySettingsList.Count){
            BattleSystem.WaveOver();
            return;
        }

        GameObject Enemy_GO = Instantiate(this.BaseEnemyPrefab, this.MainCanvas.transform);
        Enemy CurrentEnemy = Enemy_GO.GetComponent<Enemy>();

        EnemySettings Settings;

        Settings = BattleSystem.GetEnemySettingsByName(this.EnemySettingsList[this.enemyIndex].name, this.EnemySettingsList[this.enemyIndex].level-1);
        

        // CurrentEnemy.CopyFromEnemySettings(this.EnemySettingsList[this.enemyIndex]);
        CurrentEnemy.CopyFromEnemySettings(Settings);

        Enemy_GO.name = CurrentEnemy.unitName;
        CurrentEnemy.BattleSetup();
        // CurrentEnemy.PrintStatus();

        BattleSystem.Enemy = CurrentEnemy;
        this.enemyIndex++;
    }

    public void SpawnEnemyExtern(GameObject EnemyPrefab, int level, int timeBeforeSpawn = 0){
        Debug.LogError("Change the way boss enemies work!");
    }
}