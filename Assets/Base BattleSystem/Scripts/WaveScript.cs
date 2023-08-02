using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WaveScript : MonoBehaviour
{
    [SerializeField] protected List<EnemySettings> EnemySettingsList = new List<EnemySettings>();

    public int waveCount = 0;

    public abstract void Setup(int waveIndex);

    public List<EnemySettings> GetEnemySettingsList(){
        return this.EnemySettingsList;
    }

    protected void ResetEnemyInfos(){
        this.EnemySettingsList = new List<EnemySettings>();
    }

    protected void FillEnemyArrays(GameObject[] PrefabArray, int[] LevelArray){
        for(int i = 0; i < PrefabArray.Length; i++){
            EnemySettings cES = new EnemySettings();
            cES.CopyFromEnemyPrefab(PrefabArray[i]);
            cES.level = LevelArray[i];
            this.EnemySettingsList.Add(cES);
        } 
    }
}