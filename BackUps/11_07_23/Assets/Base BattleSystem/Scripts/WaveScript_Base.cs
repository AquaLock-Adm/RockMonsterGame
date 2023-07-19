using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveScript_Base : WaveScript
{
    [Header("Enemy Prefab Lib")]
    [SerializeField] private GameObject LightEnemyPrefab;
    [SerializeField] private GameObject HeavyEnemyPrefab;
    [SerializeField] private GameObject SpecialEnemyPrefab;

    [SerializeField] private GameObject TestBossEnemy;


    public override void Setup(int waveIndex){
        ResetEnemyInfos();

        switch(waveIndex){
            case 0:
                SetupWave0();
            break;

            case 1:
                SetupWave1();
            break;

            case 2:
                SetupWave2();
            break;

            case 3:
                SetupWave3();
            break;

            case 4:
                SetupWave4();
            break;

            default:
                Debug.Log("Reached End of WaveScript");
            break;
        }

        this.waveCount = 5;
    }

    // private void SetupWave0(){
    //     GameObject B = this.TestBossEnemy;
    //     GameObject[] EnemyArray =   {   B};
    //     int[] LevelArray =          {   1};
    //     int[] TimeArray =           {1000};

        // FillEnemyArrays(EnemyArray, LevelArray, TimeArray);
    // }

    private void SetupWave0(){
        GameObject LE = this.LightEnemyPrefab;
        GameObject HE = this.HeavyEnemyPrefab;
        GameObject SE = this.SpecialEnemyPrefab;

        GameObject[] EnemyArray =   {  LE,   HE,   SE,   LE,   LE,   HE,   LE,   LE,   SE,   HE,   LE,   SE};
        int[] LevelArray =          {   1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1};

        FillEnemyArrays(EnemyArray, LevelArray);
    }

    private void SetupWave1(){
        GameObject LE = this.LightEnemyPrefab;
        GameObject HE = this.HeavyEnemyPrefab;
        GameObject SE = this.SpecialEnemyPrefab;

        GameObject[] EnemyArray =   {  LE,   LE,   LE,   LE,   SE,   HE,   LE,   LE,   SE,   SE,   LE,   LE,   SE};
        int[] LevelArray =          {   1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1};

        FillEnemyArrays(EnemyArray, LevelArray);
    }

    private void SetupWave2(){
        GameObject LE = this.LightEnemyPrefab;
        GameObject HE = this.HeavyEnemyPrefab;
        GameObject SE = this.SpecialEnemyPrefab;

        GameObject[] EnemyArray =   {  LE,   LE,   LE,   HE,   LE,   LE,   SE,   LE,   LE,   LE,   HE,   HE,   LE,   LE,   LE,   LE,   SE,   HE,   SE,   SE};
        int[] LevelArray =          {   1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1};

        FillEnemyArrays(EnemyArray, LevelArray);
    }

    private void SetupWave3(){
        GameObject LE = this.LightEnemyPrefab;
        GameObject HE = this.HeavyEnemyPrefab;
        GameObject SE = this.SpecialEnemyPrefab;

        GameObject[] EnemyArray =   {  LE,   LE,   LE,   LE,   LE,   HE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   SE,   SE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   HE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   SE,   HE,   SE,   SE,   SE,   SE};
        int[] LevelArray =          {   1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1};

        FillEnemyArrays(EnemyArray, LevelArray);
    }

    private void SetupWave4(){
        GameObject LE = this.LightEnemyPrefab;
        GameObject HE = this.HeavyEnemyPrefab;
        GameObject SE = this.SpecialEnemyPrefab;

        GameObject[] EnemyArray =   {  HE,   HE,   HE,   HE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   LE,   SE,   SE,   SE,   SE,   SE,   LE,   LE,   HE,   LE,   LE,   LE,   LE,   LE,   SE,   SE,   SE,   SE};
        int[] LevelArray =          {   1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,    1};

        FillEnemyArrays(EnemyArray, LevelArray);
    }
}
