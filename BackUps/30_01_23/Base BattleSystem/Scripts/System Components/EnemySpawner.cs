using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{   
    private BattleSystem BattleSystem;
    private GameObject MainCanvas;
    public List<Enemy> SpawnedEnemies = new List<Enemy>();
    private int enemyIndex = 0;

    [SerializeField] private List<GameObject> EnemiesToBeSpawned = new List<GameObject>();

    [SerializeField] public bool stopSpawn = true;
    [SerializeField] private bool waveFullySpawned = false;
    [SerializeField] private int spawnTimeBetween_ms = 2000;

    void OnApplicationQuit(){
        this.stopSpawn = true;
        // CleanUpEnemyGO();
    }

    void Update(){
        if(this.BattleSystem.state == BattleState.PLAYERTURN || this.BattleSystem.state == BattleState.QUEUE){
            UpdateCurrentEnemy();
        }
    }

    private void UpdateCurrentEnemy(){
        Enemy FurthestEnemy;
        if(this.SpawnedEnemies.Count <= 0) FurthestEnemy = null;
        else FurthestEnemy = GetFurthestEnemy();
        
        this.BattleSystem.Enemy = FurthestEnemy;
    }

    private Enemy GetFurthestEnemy(){
        Enemy res = this.SpawnedEnemies[0];
        foreach(Enemy E in this.SpawnedEnemies){
            if(E.GetWay() > res.GetWay()) res = E;
        }

        return res;
    }

    public void CleanUpEnemyGO(){
        foreach(Enemy E in this.SpawnedEnemies){
            this.SpawnedEnemies.Remove(E);
            Destroy(E.gameObject);
        }
    }

    public void Setup(BattleSystem BS, List<GameObject> EnemiesToBeSpawned){
        this.BattleSystem = BS;
        this.MainCanvas = GameObject.Find("Main");
        this.EnemiesToBeSpawned = EnemiesToBeSpawned;
    }

    public async void StartSpawn(int delay_ms = 0){
        Debug.Log("StartSpawn called");
        await Task.Delay(delay_ms);
        this.stopSpawn = false;
        this.waveFullySpawned = false;
        SpawnLoop();
    }

    private async void SpawnLoop(){
        while(!this.stopSpawn && this.enemyIndex < this.EnemiesToBeSpawned.Count){
            if(this.BattleSystem.state == BattleState.PLAYERTURN || this.BattleSystem.state == BattleState.QUEUE){
                SpawnNewEnemy();
                await Task.Delay(this.spawnTimeBetween_ms);
            }else await Task.Yield();
        }
    }

    private void SpawnNewEnemy(){
        GameObject Enemy_GO = Instantiate(this.EnemiesToBeSpawned[this.enemyIndex], this.MainCanvas.transform);
        Enemy CurrentEnemy = Enemy_GO.GetComponent<Enemy>();

        CurrentEnemy.Setup();
        CurrentEnemy.Hide(false);
        
        this.SpawnedEnemies.Add(CurrentEnemy);
        this.enemyIndex++;

        if(this.enemyIndex == EnemiesToBeSpawned.Count){
            this.stopSpawn = true;
            this.waveFullySpawned = true;
        }
    }

    public void DeSpawnEnemy(Enemy E){
        RemoveEnemyFromList(E);
        if(this.waveFullySpawned && this.SpawnedEnemies.Count <= 0){
            this.BattleSystem.WaveOver();
        }else Debug.Log(this.waveFullySpawned);
    }

    private void RemoveEnemyFromList(Enemy E){
        foreach(Enemy SpawnedE in this.SpawnedEnemies){
            if(E == SpawnedE){
                this.SpawnedEnemies.Remove(SpawnedE);
                Destroy(SpawnedE.gameObject);
                return;
            }
        }
        Debug.Log("Enemy not found!");
    }
}
