using System.Collections.Generic;
using UnityEngine;

public class WaveRandomizer : MonoBehaviour
{
    private class StageSettings{
        public List<int> WaveSizes = new List<int>();
        public int bossWaveIndex;
        public int standartWaveRewardCredits;
        public int afterStageClearWaveRewardCredits;
        public int afterStageClearedWaveSize;
        public int enemiesFromLowerStage = 0;
        public EnemySettings BossSettings;
    }

    public const int MAX_STAGES_CURRENTLY_AVAILABLE = 2;

    private char[] ALPHABET = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'}; 
    private const int MAX_ENEMY_INDEX_NUMBER = 2600;

    private const int MAX_TRIES_FOR_LOOP = 40;

    public BattleSystem BattleSystem;

    [SerializeField] private List<string> EnemyNameLibrary = new List<string>();

    private List<StageSettings> StageSettingsList = new List<StageSettings>();

    public int stageIndex = 0;
    private int waveIndex = 0;
    private int lastStageIndex = 0;

    public int GetNextWaveSize(){
        if(this.waveIndex < this.StageSettingsList[this.stageIndex].WaveSizes.Count) return this.StageSettingsList[this.stageIndex].WaveSizes[this.waveIndex];
        else if(this.waveIndex == this.StageSettingsList[this.stageIndex].WaveSizes.Count) return 1; // Boss wave
        else return this.StageSettingsList[this.stageIndex].afterStageClearedWaveSize;
    }

    public void Setup(BattleSystem BS){
        this.stageIndex = BS.startStage-1;

        this.BattleSystem = BS;
        SetupStageSettings();
        this.lastStageIndex = this.StageSettingsList.Count-1;
        if(this.lastStageIndex == 0) BattleSystem.finalStageReached = true;
    }

    public void NextStage(){
        this.stageIndex++;
        BattleSystem.inBossWave = false;
        BattleSystem.afterFinalWave = false;
        if(this.stageIndex == this.lastStageIndex) BattleSystem.finalStageReached = true;
        this.waveIndex = 0;
    }

    public List<EnemySettings> GetEnemySettingsList(){
        List<EnemySettings> res = new List<EnemySettings>();

        StageSettings CurrentStageSettings = this.StageSettingsList[this.stageIndex];

        if(this.waveIndex == CurrentStageSettings.bossWaveIndex) {
            res.Add(CurrentStageSettings.BossSettings);
            BattleSystem.inBossWave = true;
            this.waveIndex++;
            return res;
        }

        List<EnemySettings> EnemySettingsPool = new List<EnemySettings>();

        if(this.stageIndex > 0) EnemySettingsPool = AddLowerStageEnemies(EnemySettingsPool, CurrentStageSettings);

        int spawnWaveSize = 0;

        if(this.waveIndex > CurrentStageSettings.bossWaveIndex) {
            spawnWaveSize = CurrentStageSettings.afterStageClearedWaveSize;
            BattleSystem.afterFinalWave = true;
        } else spawnWaveSize = CurrentStageSettings.WaveSizes[this.waveIndex];

        EnemySettingsPool = AddCurrentStageEnemies(EnemySettingsPool, CurrentStageSettings);
        int retries = 0;

        for(int enemyI = 0; enemyI < spawnWaveSize; enemyI++){
            EnemySettings RandomEnemySettings;

            int ranInd = Random.Range(0, EnemySettingsPool.Count);

            RandomEnemySettings = EnemySettingsPool[ranInd];

            if(stageIndex == 0){
                if(!ThreeShieldsInFirstTwoEnemiesCheck(res, RandomEnemySettings)){
                    enemyI--;
                    retries++;
                    if(retries < MAX_TRIES_FOR_LOOP) continue;
                }
            }
            
            retries = 0;
            res.Add(RandomEnemySettings);
        }

        this.waveIndex++;
        // PrintEnemySettingsList(res);
        return res;
    }

    public int GetCurrentWaveClearRewardCredits(){
        StageSettings CurrentStageSettings = this.StageSettingsList[this.stageIndex];
        int reward = 0;

        if (this.waveIndex-1 < CurrentStageSettings.bossWaveIndex){
            reward = CurrentStageSettings.standartWaveRewardCredits;
        }else if(this.waveIndex-1 > this.StageSettingsList[this.stageIndex].bossWaveIndex){
            reward = CurrentStageSettings.afterStageClearWaveRewardCredits;
        }
        return reward;
    }

    private List<EnemySettings> AddLowerStageEnemies(List<EnemySettings> Pool, StageSettings sSettings){
        int bossIndex = BattleSystem.GetSettingsIndexByName(this.StageSettingsList[this.stageIndex-1].BossSettings.name, this.stageIndex-1);
        List<EnemySettings> lowEn = SelectRandomEnemiesFrom(BattleSystem.GetAllEnemiesFromStage(this.stageIndex-1), sSettings.enemiesFromLowerStage, bossIndex);
        foreach(EnemySettings E in lowEn){
            Pool.Add(E);
        }
        return Pool;
    }
    private List<EnemySettings> AddCurrentStageEnemies(List<EnemySettings> Pool, StageSettings sSettings){
        foreach(EnemySettings E in BattleSystem.GetAllEnemiesFromStage(this.stageIndex)){
            if(E.name != sSettings.BossSettings.name) Pool.Add(E);
        }
        return Pool;
    }

    private bool ThreeShieldsInFirstTwoEnemiesCheck(List<EnemySettings> CurrentEnemySettingsList, EnemySettings Settings){
        if(CurrentEnemySettingsList.Count > 2) return true;

        foreach(List<ShieldMode> defenseMode in Settings.DefensiveModes[0]){
            if(defenseMode.Count >= 3) return false;
        }

        return true;
    }

    private void SetupStageSettings(){
        SetupStageOne();
        SetupStageTwo();
    }



#region Stage 1 Setup
    private void SetupStageOne(){
        StageSettings StageOneSettings = new StageSettings();
        int myStageIndex = 0;

        // StageOneSettings.WaveSizes.Add(1);
        
        StageOneSettings.WaveSizes.Add(3);
        StageOneSettings.WaveSizes.Add(4);
        StageOneSettings.WaveSizes.Add(5);
        StageOneSettings.WaveSizes.Add(6);
        StageOneSettings.WaveSizes.Add(7);
        StageOneSettings.WaveSizes.Add(7);

        StageOneSettings.bossWaveIndex = StageOneSettings.WaveSizes.Count;
        if(!BattleSystem.EnemyLibraryEmpty()){
            Debug.Log("Setting up with BS.EnemyLibrary");

            string bossName = GetStageOneBossName();
            StageOneSettings.BossSettings = BattleSystem.GetEnemySettingsByName(bossName, myStageIndex);
            AddNamesToNameLibrary(BattleSystem.GetAllEnemiesFromStage(myStageIndex));
        }else{
            StageOneSettings.BossSettings = SetupStageOneBoss();
            this.EnemyNameLibrary.Add(StageOneSettings.BossSettings.name);

            List<EnemySettings> NewStageOneEnemies = new List<EnemySettings>();
            NewStageOneEnemies = RandomizeStageOneEnemies(15);
            // NewStageOneEnemies = RandomizeStageOneEnemies(1);
            NewStageOneEnemies.Add(StageOneSettings.BossSettings);

            BattleSystem.InitNewEnemyLibraryStage(0, NewStageOneEnemies);
        }

        StageOneSettings.standartWaveRewardCredits = 150;
        StageOneSettings.afterStageClearWaveRewardCredits = 220;

        StageOneSettings.afterStageClearedWaveSize = 10;

        this.StageSettingsList.Add(StageOneSettings);
    }
    private EnemySettings SetupStageOneBoss(){
        EnemySettings bossSettings = new EnemySettings();
        List<ShieldMode> defMode1 = new List<ShieldMode>();
        List<ShieldMode> defMode2 = new List<ShieldMode>();
        List<ShieldMode> defMode3 = new List<ShieldMode>();

        defMode1.Add(ShieldMode.SPECIAL);
        defMode1.Add(ShieldMode.HEAVY);

        defMode2.Add(ShieldMode.ANY);
        defMode2.Add(ShieldMode.HEAVY);

        defMode3.Add(ShieldMode.SPECIAL);
        defMode3.Add(ShieldMode.LIGHT);
        defMode3.Add(ShieldMode.LIGHT);

        bossSettings.InitDefensiveMode();

        bossSettings.DefensiveModes[0].Add(defMode1);
        bossSettings.DefensiveModes[0].Add(defMode2);
        bossSettings.DefensiveModes[0].Add(defMode3);
        // bossSettings.InitDiscoveredShieldsList(bossSettings.DefensiveModes[0].Count);
        bossSettings.InitDiscoveredShieldsList();

        bossSettings.defensiveModeIndex = Random.Range(0,3);

        bossSettings.InitAttackLibrary(1, 3, 5);

        bossSettings.name = GetStageOneBossName();
        bossSettings.damage = 3;
        bossSettings.level = 1;
        bossSettings.hp = 120;
        bossSettings.battleSpeed = 6;
        bossSettings.baseKillPrice = 1000;
        bossSettings.maxKillPrice = 10000;

        return bossSettings;
    }
    private string GetStageOneBossName(){
        return "IX245";
    }
    private List<EnemySettings> RandomizeStageOneEnemies(int enemyCount){
        List<EnemySettings> res = new List<EnemySettings>();
        int shieldModeThreeChance_p = 20;

        for(int enemyI = 0; enemyI < enemyCount; enemyI++){
            int ran = Random.Range(1, 101);
            if(ran <= shieldModeThreeChance_p) res.Add(RandomizeSingleStageOneEnemy(3));
            else res.Add(RandomizeSingleStageOneEnemy(2));
        }
        return res;
    }
    private EnemySettings RandomizeSingleStageOneEnemy(int shieldCount){
        /*
        shieldStances: 1 (=different shield mode combinations)
        grey shield chance: 50%
        */

        EnemySettings Res = new EnemySettings();
        string name = GenerateRandomEnemyName();
        int hp = Random.Range(35, 56);

        if(shieldCount >= 3) {
            hp = 0;
        }

        List<ShieldMode> Shields = RandomizeShields(shieldCount, 50);
        Res.InitDefensiveMode();
        Res.DefensiveModes[0].Add(Shields);

        Res.InitDiscoveredShieldsList();
        // Res.InitDiscoveredShieldsList(Shields.Count);   // <- this is wrong (((I think))) 

        Res.name = name;
        Res.level = 1;
        Res.hp = hp;
        Res.battleSpeed = 4;

        if(shieldCount >= 3){
            Res.InitAttackLibrary(1, 2, 3);
            Res.baseKillPrice = 23;
            Res.maxKillPrice = 200;
            Res.damage = 4;
        }else{
            Res.InitAttackLibrary(1, 2, 3);
            Res.baseKillPrice = 15;
            Res.maxKillPrice = 100;
            Res.damage = 2;
        }

        return Res;
    }
#endregion

    

#region Stage 2 Setup
    private void SetupStageTwo(){
        StageSettings StageTwoSettings = new StageSettings();
        int myStageIndex = 1;
        
        StageTwoSettings.WaveSizes.Add(5);
        StageTwoSettings.WaveSizes.Add(7);
        StageTwoSettings.WaveSizes.Add(7);
        StageTwoSettings.WaveSizes.Add(10);
        StageTwoSettings.WaveSizes.Add(10);
        StageTwoSettings.WaveSizes.Add(10);
        StageTwoSettings.WaveSizes.Add(10);
        StageTwoSettings.WaveSizes.Add(15);

        StageTwoSettings.bossWaveIndex = StageTwoSettings.WaveSizes.Count;
        if(BattleSystem.GetEnemyLibraryStageCount() >= myStageIndex+1){
            Debug.Log("Setting up with BS.EnemyLibrary");

            string bossName = GetStageTwoBossName();
            StageTwoSettings.BossSettings = BattleSystem.GetEnemySettingsByName(bossName, myStageIndex);
            AddNamesToNameLibrary(BattleSystem.GetAllEnemiesFromStage(myStageIndex));
        }else{
            StageTwoSettings.BossSettings = SetupStageTwoBoss();
            this.EnemyNameLibrary.Add(StageTwoSettings.BossSettings.name);

            List<EnemySettings> NewStageTwoEnemies = new List<EnemySettings>();
            NewStageTwoEnemies = RandomizeStageTwoEnemies(20);
            NewStageTwoEnemies.Add(StageTwoSettings.BossSettings);

            BattleSystem.InitNewEnemyLibraryStage(1, NewStageTwoEnemies);
        }

        StageTwoSettings.enemiesFromLowerStage = 5;

        StageTwoSettings.standartWaveRewardCredits = 300;
        StageTwoSettings.afterStageClearWaveRewardCredits = 440;

        StageTwoSettings.afterStageClearedWaveSize = 20;

        this.StageSettingsList.Add(StageTwoSettings);
    }
    private EnemySettings SetupStageTwoBoss(){
        EnemySettings bossSettings = new EnemySettings();
        List<ShieldMode> defMode1 = new List<ShieldMode>();
        List<ShieldMode> defMode2 = new List<ShieldMode>();
        List<ShieldMode> defMode3 = new List<ShieldMode>();
        List<ShieldMode> defMode4 = new List<ShieldMode>();
        List<ShieldMode> defMode5 = new List<ShieldMode>();

        defMode1.Add(ShieldMode.SPECIAL);
        defMode1.Add(ShieldMode.HEAVY);
        defMode1.Add(ShieldMode.ANY);
        defMode1.Add(ShieldMode.HEAVY);

        defMode2.Add(ShieldMode.ANY);
        defMode2.Add(ShieldMode.LIGHT);
        defMode2.Add(ShieldMode.LIGHT);
        defMode2.Add(ShieldMode.HEAVY);
        defMode2.Add(ShieldMode.SPECIAL);

        defMode3.Add(ShieldMode.SPECIAL);
        defMode3.Add(ShieldMode.LIGHT);
        defMode3.Add(ShieldMode.HEAVY);
        defMode3.Add(ShieldMode.LIGHT);
        defMode3.Add(ShieldMode.ANY);

        defMode4.Add(ShieldMode.LIGHT);
        defMode4.Add(ShieldMode.ANY);
        defMode4.Add(ShieldMode.ANY);
        defMode4.Add(ShieldMode.HEAVY);

        defMode5.Add(ShieldMode.HEAVY);
        defMode5.Add(ShieldMode.HEAVY);
        defMode5.Add(ShieldMode.ANY);
        defMode5.Add(ShieldMode.ANY);
        defMode5.Add(ShieldMode.ANY);

        bossSettings.InitDefensiveMode();

        bossSettings.DefensiveModes[0].Add(defMode1);
        bossSettings.DefensiveModes[0].Add(defMode2);
        bossSettings.DefensiveModes[0].Add(defMode3);
        bossSettings.DefensiveModes[0].Add(defMode4);
        bossSettings.DefensiveModes[0].Add(defMode5);

        bossSettings.InitDiscoveredShieldsList();

        bossSettings.defensiveModeIndex = Random.Range(0,bossSettings.DefensiveModes[0].Count);

        // bossSettings = InitAttackLibrary(bossSettings, 2, 5, 3);
        bossSettings.InitAttackLibrary(2, 5, 6);

        bossSettings.name = GetStageTwoBossName();
        bossSettings.level = 2;
        bossSettings.hp = 4000;
        bossSettings.battleSpeed = 6;
        bossSettings.baseKillPrice = 4000;
        bossSettings.maxKillPrice = 40000;

        return bossSettings;
    }
    private string GetStageTwoBossName(){
        return "DE556";
    }
    private List<EnemySettings> RandomizeStageTwoEnemies(int enemyCount){
        List<EnemySettings> Res = new List<EnemySettings>();
        int shieldModeFourChance_p = 30;
        int defensiveModesThreeChance_p = 30;

        for(int enemyI = 0; enemyI < enemyCount; enemyI++){
            int ran = Random.Range(1, 101);
            int shieldCount = 3;
            int defensiveModeCount = 2;

            if(ran <= shieldModeFourChance_p) shieldCount = 4;
            else{
                ran = Random.Range(1, 101);
                if(ran <= defensiveModesThreeChance_p) defensiveModeCount = 3;
            }

            Res.Add(RandomizeSingleStageTwoEnemy(shieldCount, defensiveModeCount));
        }
        return Res;
    }
    private EnemySettings RandomizeSingleStageTwoEnemy(int shieldCount, int defensiveModeCount){
        /*
        Difficulties:
            1:
                if shieldCount >= 4
                 hp: 0
                if defensiveModeCount > 1
                 hp = 20 <-> 45
                 grey shield chance: 50%
                else
                 hp: 50 <-> 100
                 grey shield chance: 40%
        */
        EnemySettings Res = new EnemySettings();
        string name = GenerateRandomEnemyName();

        int hp;
        int greyShield_p;

        if(shieldCount > 3){
            hp = 0;
            greyShield_p = 40;
        }else{
            if(defensiveModeCount > 3){
                hp = 0;
                greyShield_p = 50;
            }else {
                hp = Random.Range(90,130);
                greyShield_p = 40;
            }
        }

        List<ShieldMode> Shields = RandomizeShields(shieldCount, greyShield_p);
        Res.InitDefensiveMode();
        Res.DefensiveModes[0].Add(Shields);

        Res.InitDiscoveredShieldsList();

        Res.name = name;
        Res.level = 2;
        Res.hp = hp;
        Res.battleSpeed = 5;

        if(shieldCount > 3){
            Res.baseKillPrice = 70;
            Res.maxKillPrice = 700;
            Res.damage = 7;
            Res.InitAttackLibrary(1, 4, 3);
        }else{
            Res.baseKillPrice = 45;
            Res.maxKillPrice = 480;
            Res.damage = 6;
            Res.InitAttackLibrary(1, 4, 4);
        }

        return Res;
    }
#endregion



    private void AddNamesToNameLibrary(List<EnemySettings> EList){
        foreach(EnemySettings E in EList){
            this.EnemyNameLibrary.Add(E.name);
        }
    }

    private string GenerateRandomEnemyName(){
        string res;
        do{
            int nameNum = Random.Range(1, MAX_ENEMY_INDEX_NUMBER+1);
            // Debug.Log("Enemy Index: "+nameNum.ToString());

            int number = (nameNum%100)-1;
            if(number < 0) number = 99;
            string numString;
            if(number > 9) numString = number.ToString();
            else numString = "0"+number.ToString();

            int letterIndex = (nameNum/100);
            char letter;
            if(number == 99) letter = ALPHABET[letterIndex-1];
            else letter = ALPHABET[letterIndex];

            res = letter + numString;
            // Debug.Log("Enemy Name Generated: "+res);
        }while(this.EnemyNameLibrary.Contains(res));

        this.EnemyNameLibrary.Add(res);
        return res;
    }

    private List<ShieldMode> RandomizeShields(int shieldCount, int greyShield_p){
        List<ShieldMode> res = new List<ShieldMode>();

        for(int i=0;i<shieldCount;i++){
            int greyRoll = Random.Range(1,101);

            if(greyRoll <= greyShield_p){
                res.Add(ShieldMode.ANY);
            }else{
                int shieldRoll = Random.Range(1,31);

                if(shieldRoll <= 10) res.Add(ShieldMode.LIGHT);
                else if(shieldRoll <= 20) res.Add(ShieldMode.HEAVY);
                else res.Add(ShieldMode.SPECIAL);
            }
        }

        return res;
    }

    private List<EnemySettings> SelectRandomEnemiesFrom(List<EnemySettings> Library, int enemyCount, int bossIndex){
        List<EnemySettings> res = new List<EnemySettings>();
        List<int> UsedIndeces = new List<int>();

        for(int i=0;i<enemyCount;i++){
            int ranIndex = Random.Range(0,Library.Count-1);
            if(ranIndex >= bossIndex) ranIndex++;
            if(UsedIndeces.Contains(ranIndex)){
                i--;
                continue;
            }
            UsedIndeces.Add(ranIndex);
            res.Add(Library[ranIndex]);
        }

        return res;
    }

    private void PrintEnemySettingsList(List<EnemySettings> list){
        string s = "";

        foreach(EnemySettings E in list){
            s += E.name + " ";
        }
        s += "\n";
        Debug.Log(s);
    }
}