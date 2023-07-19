using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveScript : MonoBehaviour {
    public string waveScriptName;
    public int waveCount;
    public WaveSpawner WaveSpawner;
    public List<GameObject> EnemyPrefabs;

    public void Activate(){
        switch(waveScriptName){
            // case "Tutorial":
            //     WaveSpawner = new TutorialWave(EnemyPrefabs);
            //     waveCount = 10;
            // break;
            default:
                Debug.Log("Wave Script Name is not set or the name is not added in WaveScript.Activate()!");
            break;
        }
    }
    
    public List<GameObject> NextWave(){
        return WaveSpawner.NextWave();
    }
}

public abstract class WaveSpawner {
    public int waveCount;
    public int currentWaveSize;
    public int currentWave = 0;
    public List<GameObject> EnemyPrefabs;
    public List<Enemy> EnemyList;

    public abstract List<GameObject> NextWave();
}

// public class TutorialWave : WaveSpawner {
//     private int rmIndex;
//     private int sbIndex;
//     private int mgIndex;

//     public TutorialWave(List<GameObject> EnemyPrefabs) {
//         this.EnemyPrefabs = EnemyPrefabs;

//         this.waveCount = 10;
//         this.waveSize = 5;
//         this.EnemyList = new List<Enemy>();

//         CheckEnemyPrefabs();
//     }

//     private void CheckEnemyPrefabs(){
//         for(int i = 0; i < this.EnemyPrefabs.Count; i++){
//             Enemy CurrentEnemy = this.EnemyPrefabs[i].GetComponent<Enemy>();
//             if(CurrentEnemy.unitName == "Rock Monster") this.rmIndex = i;
//             else if(CurrentEnemy.unitName == "Stone Bat") this.sbIndex = i;
//             else if(CurrentEnemy.unitName == "Malicious Granit") this.mgIndex = i;
//             else Debug.Log("Unexpected Enemy in EnemyPrefabs! Name: "+ CurrentEnemy.unitName);
//         }
//     }

//     public override List<Enemy> NextWave(){
//         switch(this.currentWave){
//             case 0:
//                 SpawnWave0();
//             break;
//             case 1:
//                 SpawnWave1();
//             break;
//             case 2:
//                 SpawnWave2();
//             break;
//             case 3:
//                 SpawnWave3();
//             break;
//             case 4:
//                 SpawnWave4();
//             break;
//             case 9:
//                 SpawnWave9();
//             break;
//             default:
//                 if(this.currentWave >= 4){
//                     SpawnFullMali();
//                 }
//                 else return null;
//             break;
//         }
//         this.currentWave++;
//         return this.EnemyList;
//     }

//     private void SpawnWave0() {
//         this.EnemyList.Add(SpawnRockMonster(2));
//         this.EnemyList.Add(SpawnStoneBat(3));   // switched for aoe testing
//         this.EnemyList.Add(SpawnRockMonster(3));// switched for aoe testing
//         this.EnemyList.Add(SpawnStoneBat(2));
//         this.EnemyList.Add(SpawnRockMonster(3));
//     }

//     private void SpawnWave1() {
//         this.EnemyList.Add(SpawnRockMonster(2));
//         this.EnemyList.Add(SpawnStoneBat(2));
//         this.EnemyList.Add(SpawnRockMonster(3));
//         this.EnemyList.Add(SpawnRockMonster(3));
//         this.EnemyList.Add(SpawnMaliciousGranit(3));
//     }

//     private void SpawnWave2() {
//         this.EnemyList.Add(SpawnStoneBat(3));
//         this.EnemyList.Add(SpawnStoneBat(3));
//         this.EnemyList.Add(SpawnRockMonster(3));
//         this.EnemyList.Add(SpawnMaliciousGranit(3));
//         this.EnemyList.Add(SpawnStoneBat(4));
//     }

//     private void SpawnWave3() {
//         this.EnemyList.Add(SpawnMaliciousGranit(3));
//         this.EnemyList.Add(SpawnStoneBat(3));
//         this.EnemyList.Add(SpawnRockMonster(3));
//         this.EnemyList.Add(SpawnMaliciousGranit(3));
//         this.EnemyList.Add(SpawnStoneBat(4));
//     }

//     private void SpawnWave4() {
//         this.EnemyList.Add(SpawnStoneBat(3));
//         this.EnemyList.Add(SpawnRockMonster(3));
//         this.EnemyList.Add(SpawnMaliciousGranit(3));
//         this.EnemyList.Add(SpawnMaliciousGranit(4));
//         this.EnemyList.Add(SpawnMaliciousGranit(4));
//     }

//     private void SpawnWave9() {
//         for(int i = 0; i < 4; i++){
//             this.EnemyList.Add(SpawnMaliciousGranit(4));
//         }

//         this.EnemyList.Add(SpawnStoneBat(15));
//     }

//     private void SpawnFullMali() {
//         for(int i = 0; i < 5; i++){
//             this.EnemyList.Add(SpawnMaliciousGranit(4));
//         }
//     }

//     private Enemy SpawnRockMonster(int lvl){
//         this.EnemyPrefabs[this.rmIndex].GetComponent<Enemy>().minLevel = lvl;
//         this.EnemyPrefabs[this.rmIndex].GetComponent<Enemy>().maxLevel = lvl;
//         GameObject rmGO = MonoBehaviour.Instantiate(this.EnemyPrefabs[this.rmIndex]);
//         Enemy rmUnit = rmGO.GetComponent<Enemy>();
//         //rmUnit.actSpeed = 1000;

//         return rmUnit;
//     }

//     private Enemy SpawnStoneBat(int lvl){
//         this.EnemyPrefabs[this.sbIndex].GetComponent<Enemy>().minLevel = lvl;
//         this.EnemyPrefabs[this.sbIndex].GetComponent<Enemy>().maxLevel = lvl;
//         GameObject sbGO = MonoBehaviour.Instantiate(this.EnemyPrefabs[this.sbIndex]);
//         Enemy sbUnit = sbGO.GetComponent<Enemy>();
//         //sbUnit.actSpeed = 1000;

//         return sbUnit;
//     }

//     private Enemy SpawnMaliciousGranit(int lvl){
//         this.EnemyPrefabs[this.mgIndex].GetComponent<Enemy>().minLevel = lvl;
//         this.EnemyPrefabs[this.mgIndex].GetComponent<Enemy>().maxLevel = lvl;
//         GameObject mgGO = MonoBehaviour.Instantiate(this.EnemyPrefabs[this.mgIndex]);
//         Enemy mgUnit = mgGO.GetComponent<Enemy>();
//         //mgUnit.actSpeed = 1000;

//         return mgUnit;
//     }
// }
