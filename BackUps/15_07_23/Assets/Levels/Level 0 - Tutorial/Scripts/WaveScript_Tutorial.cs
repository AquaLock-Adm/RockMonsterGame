using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveScript_Tutorial : WaveScript
{
    [Header("Enemy Prefab Lib")]
    [SerializeField] private GameObject Enemy1Prefab;
    [SerializeField] private GameObject Enemy2Prefab;
    [SerializeField] private GameObject Enemy3Prefab;
    [SerializeField] private GameObject Enemy4Prefab;
    [SerializeField] private GameObject Enemy5Prefab;
    [SerializeField] private GameObject Enemy6Prefab;


    public override void Setup(int waveIndex){
        ResetEnemyInfos();

        switch(waveIndex){
            case 0:
                SetupWave0();
            break;

            default:
                Debug.Log("Reached End of WaveScript");
            break;
        }

        this.waveCount = 1;
    }

    private void SetupWave0(){
        GameObject E1 = this.Enemy1Prefab;
        GameObject E2 = this.Enemy2Prefab;
        GameObject E3 = this.Enemy3Prefab;
        GameObject E4 = this.Enemy4Prefab;
        GameObject E5 = this.Enemy5Prefab;
        GameObject E6 = this.Enemy6Prefab;
        GameObject[] EnemyArray =   {  E1,   E2,   E3,   E4,   E5,   E6};
        int[] LevelArray =          {   1,    1,    1,    1,    1,    1};

        FillEnemyArrays(EnemyArray, LevelArray);
    }
}
