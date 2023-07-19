using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveRandomizer : MonoBehaviour
{
    private class StageSettings{
        public List<int> WaveSizes = new List<int>();
        public int bossWaveIndex;
        public int afterStageClearedWaveSize;
        public int enemiesFromLowerStage = 0;
        public EnemySettings BossSettings;
    }

    private char[] ALPHABET = {'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'}; 
    private const int MAX_ENEMY_INDEX_NUMBER = 2600;

    public BattleSystem BattleSystem;

    [SerializeField] private List<string> EnemyNameLibrary = new List<string>();

    private List<StageSettings> StageSettingsList = new List<StageSettings>();

    public int stageIndex = 0;
    private int waveIndex = 0;
    private int lastStageIndex = 0;

    public int GetNextWaveSize(){
        if(this.waveIndex < this.StageSettingsList[this.stageIndex].WaveSizes.Count) return this.StageSettingsList[this.stageIndex].WaveSizes[this.waveIndex];
        else return this.StageSettingsList[this.stageIndex].afterStageClearedWaveSize;
    }

    public void Setup(BattleSystem BS){
        // TODO: Dont re randomize if level is restarted during normal play
        this.BattleSystem = BS;
        SetupStageSettings();
        this.lastStageIndex = this.StageSettingsList.Count-1;
        if(this.lastStageIndex == 0) BattleSystem.finalStageReached = true;
    }                                                       // TODO

    public void NextStage(){
        this.stageIndex++;
        BattleSystem.inBossWave = false;
        BattleSystem.afterFinalWave = false;
        if(this.stageIndex == this.lastStageIndex) BattleSystem.finalStageReached = true;
        this.waveIndex = 0;
    }

    public EnemySettings GetFirstEnemyOfWave(){
        EnemySettings Res = new EnemySettings();

        List<ShieldMode> Shields = new List<ShieldMode>();
        Shields.Add(ShieldMode.ANY);

        Res = InitDefensiveMode(Res);
        Res.DefensiveModes[0].Add(Shields);
        Res = InitDiscoveredShieldsList(Res, Shields.Count);

        Res.name = "Rock Monster";
        Res.level = 0;
        Res.hp = 0;
        Res.baseKillPrice = 0;
        Res.maxKillPrice = 0;
        
        return Res;
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

        for(int enemyI = 0; enemyI < spawnWaveSize; enemyI++){
            EnemySettings RandomEnemySettings;

            int ranInd = Random.Range(0, EnemySettingsPool.Count);

            RandomEnemySettings = EnemySettingsPool[ranInd];
            
            res.Add(RandomEnemySettings);
        }

        this.waveIndex++;
        // PrintEnemySettingsList(res);
        return res;
    }

    private List<EnemySettings> AddLowerStageEnemies(List<EnemySettings> Pool, StageSettings sSettings){
        int bossIndex = BattleSystem.GetSettingsIndexByName(this.StageSettingsList[this.stageIndex-1].BossSettings.name, this.stageIndex-1);
        List<EnemySettings> lowEn = SelectRandomEnemiesFrom(BattleSystem.EnemyLibrary[this.stageIndex-1], sSettings.enemiesFromLowerStage, bossIndex);
        foreach(EnemySettings E in lowEn){
            Pool.Add(E);
        }
        return Pool;
    }
    private List<EnemySettings> AddCurrentStageEnemies(List<EnemySettings> Pool, StageSettings sSettings){
        foreach(EnemySettings E in BattleSystem.EnemyLibrary[this.stageIndex]){
            if(E.name != sSettings.BossSettings.name) Pool.Add(E);
        }
        return Pool;
    }

    private void SetupStageSettings(){
        SetupStageOne();
        SetupStageTwo();
    }

    private void SetupStageOne(){
        StageSettings StageOneSettings = new StageSettings();
        int myStageIndex = 0;

        StageOneSettings.WaveSizes.Add(3);
        StageOneSettings.WaveSizes.Add(4);
        StageOneSettings.WaveSizes.Add(5);
        StageOneSettings.WaveSizes.Add(5);
        StageOneSettings.WaveSizes.Add(5);
        StageOneSettings.WaveSizes.Add(5);

        StageOneSettings.bossWaveIndex = StageOneSettings.WaveSizes.Count;
        if(BattleSystem.EnemyLibrary.Count >= myStageIndex+1){
            Debug.Log("Setting up with BS.EnemyLibrary");

            string bossName = GetStageOneBossName();
            StageOneSettings.BossSettings = BattleSystem.GetEnemySettingsByName(bossName, myStageIndex);
            AddNamesToNameLibrary(BattleSystem.EnemyLibrary[myStageIndex]);
        }else{
            StageOneSettings.BossSettings = SetupStageOneBoss();
            this.EnemyNameLibrary.Add(StageOneSettings.BossSettings.name);

            BattleSystem.EnemyLibrary.Add(new List<EnemySettings>());

            BattleSystem.EnemyLibrary[myStageIndex] = RandomizeStageOneEnemies(27);
            BattleSystem.EnemyLibrary[myStageIndex].Add(StageOneSettings.BossSettings);
        }

        StageOneSettings.afterStageClearedWaveSize = 5;

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

        bossSettings = InitDefensiveMode(bossSettings);

        bossSettings.DefensiveModes[0].Add(defMode1);
        bossSettings.DefensiveModes[0].Add(defMode2);
        bossSettings.DefensiveModes[0].Add(defMode3);

        bossSettings = InitDiscoveredShieldsList(bossSettings, bossSettings.DefensiveModes[0].Count);

        bossSettings.defensiveModeIndex = Random.Range(0,3);

        bossSettings.name = GetStageOneBossName();
        bossSettings.level = 1;
        bossSettings.hp = 120;
        bossSettings.baseKillPrice = 1000;
        bossSettings.maxKillPrice = 10000;

        return bossSettings;
    }
    private string GetStageOneBossName(){
        return "IX245";
    }
    private List<EnemySettings> RandomizeStageOneEnemies(int enemyCount){
        List<EnemySettings> res = new List<EnemySettings>();
        int shieldModeThreeChance_p = 10;
        for(int enemyI = 0; enemyI < enemyCount; enemyI++){
            int ran = Random.Range(1, 101);
            if(ran <= shieldModeThreeChance_p) res.Add(RandomizeSingleStageOneEnemy(3));
            else res.Add(RandomizeSingleStageOneEnemy(2));
        }
        return res;
    }
    private EnemySettings RandomizeSingleStageOneEnemy(int shieldCount){
        /*
        Difficulties:
            1:
                if shieldCount >= 3
                 hp: 0
                 1 randomShield at index >= 1 
                else
                 hp: 10 <-> 30
                
                shieldStances: 1 (=different shield mode combinations)
                grey shield chance: 50%
        */
        EnemySettings Res = new EnemySettings();
        string name = GenerateRandomEnemyName();
        int hp = Random.Range(10,21);
        // int hp = 0;
        bool addRandomShield = false;

        if(shieldCount >= 3) {
            hp = 0;
            addRandomShield = true;
        }

        List<ShieldMode> Shields = RandomizeShields(shieldCount, addRandomShield, 50);
        Res = InitDefensiveMode(Res);
        Res.DefensiveModes[0].Add(Shields);
        Res = InitDiscoveredShieldsList(Res, Shields.Count);

        Res.name = name;
        Res.level = 1;
        Res.hp = hp;
        if(addRandomShield){
            Res.baseKillPrice = 20;
            Res.maxKillPrice = 1000;
        }else{
            Res.baseKillPrice = 5;
            Res.maxKillPrice = 1200;
        }

        return Res;
    }

    private void SetupStageTwo(){
        StageSettings StageTwoSettings = new StageSettings();
        int myStageIndex = 1;
        
        StageTwoSettings.WaveSizes.Add(5);
        StageTwoSettings.WaveSizes.Add(5);
        StageTwoSettings.WaveSizes.Add(5);
        StageTwoSettings.WaveSizes.Add(7);
        StageTwoSettings.WaveSizes.Add(7);
        StageTwoSettings.WaveSizes.Add(7);
        StageTwoSettings.WaveSizes.Add(7);
        StageTwoSettings.WaveSizes.Add(7);

        StageTwoSettings.bossWaveIndex = StageTwoSettings.WaveSizes.Count;
        if(BattleSystem.EnemyLibrary.Count >= myStageIndex+1){
            Debug.Log("Setting up with BS.EnemyLibrary");

            string bossName = GetStageTwoBossName();
            StageTwoSettings.BossSettings = BattleSystem.GetEnemySettingsByName(bossName, myStageIndex);
            AddNamesToNameLibrary(BattleSystem.EnemyLibrary[myStageIndex]);
        }else{
            StageTwoSettings.BossSettings = SetupStageTwoBoss();
            this.EnemyNameLibrary.Add(StageTwoSettings.BossSettings.name);

            BattleSystem.EnemyLibrary.Add(new List<EnemySettings>());

            BattleSystem.EnemyLibrary[myStageIndex] = RandomizeStageTwoEnemies(35);
            BattleSystem.EnemyLibrary[myStageIndex].Add(StageTwoSettings.BossSettings);
        }

        StageTwoSettings.enemiesFromLowerStage = 15;
        StageTwoSettings.afterStageClearedWaveSize = 7;

        this.StageSettingsList.Add(StageTwoSettings);
    }
    private EnemySettings SetupStageTwoBoss(){
        EnemySettings bossSettings = new EnemySettings();
        List<ShieldMode> defMode1 = new List<ShieldMode>();
        List<ShieldMode> defMode2 = new List<ShieldMode>();
        List<ShieldMode> defMode3 = new List<ShieldMode>();

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

        bossSettings = InitDefensiveMode(bossSettings);

        bossSettings.DefensiveModes[0].Add(defMode1);
        bossSettings.DefensiveModes[0].Add(defMode2);
        bossSettings.DefensiveModes[0].Add(defMode3);

        bossSettings = InitDiscoveredShieldsList(bossSettings, bossSettings.DefensiveModes[0].Count);

        bossSettings.defensiveModeIndex = Random.Range(0,bossSettings.DefensiveModes[0].Count);

        bossSettings.name = GetStageTwoBossName();
        bossSettings.level = 2;
        bossSettings.hp = 520;
        bossSettings.baseKillPrice = 4000;
        bossSettings.maxKillPrice = 40000;

        return bossSettings;
    }
    private string GetStageTwoBossName(){
        return "DE556";
    }
    private List<EnemySettings> RandomizeStageTwoEnemies(int enemyCount){
        List<EnemySettings> res = new List<EnemySettings>();
        int shieldModeFourChance_p = 30;
        int defensiveModesTwoChance_p = 15;

        for(int enemyI = 0; enemyI < enemyCount; enemyI++){
            int ran = Random.Range(1, 101);
            int shieldCount = 3;
            int defensiveModeCount = 1;

            if(ran <= shieldModeFourChance_p) shieldCount = 4;
            else{
                ran = Random.Range(1, 101);
                if(ran <= defensiveModesTwoChance_p) defensiveModeCount = 2;
            }

            res.Add(RandomizeSingleStageTwoEnemy(shieldCount, defensiveModeCount));
        }
        return res;
    }
    private EnemySettings RandomizeSingleStageTwoEnemy(int shieldCount, int defensiveModeCount){
        /*
        Difficulties:
            1:
                if shieldCount >= 4
                 hp: 0
                 1 randomShield at index >= 1
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
        bool useRandomShield = false;

        if(shieldCount > 3){
            hp = 0;
            greyShield_p = 40;
            useRandomShield = true;
        }else{
            if(defensiveModeCount > 2){
                hp = Random.Range(20,46);
                greyShield_p = 50;
            }else {
                hp = Random.Range(50,101);
                greyShield_p = 40;
            }
        }

        List<ShieldMode> Shields = RandomizeShields(shieldCount, useRandomShield, greyShield_p);
        Res = InitDefensiveMode(Res);
        Res.DefensiveModes[0].Add(Shields);
        Res = InitDiscoveredShieldsList(Res, Shields.Count);

        Res.name = name;
        Res.level = 2;
        Res.hp = hp;

        if(useRandomShield){
            Res.baseKillPrice = 55;
            Res.maxKillPrice = 2000;
        }else{
            Res.baseKillPrice = 24;
            Res.maxKillPrice = 2500;
        }

        return Res;
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

    private List<EnemySettings> GetEnemyLibraryFromBattleSystem(int stageIndex, string bossName){
        List<EnemySettings> res = new List<EnemySettings>();

        foreach(EnemySettings E in BattleSystem.EnemyLibrary[stageIndex]){
            if(E.name != bossName) res.Add(E);
        }
        return res;
    }

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

    private List<ShieldMode> RandomizeShields(int shieldCount, bool addRandomShield, int greyShield_p){
        List<ShieldMode> res = new List<ShieldMode>();
        bool randomShieldAdded = false;

        for(int i=0;i<shieldCount;i++){
            if(i > 0 && addRandomShield && !randomShieldAdded){
                int coinFlip = Random.Range(1,101);
                if(coinFlip <= 50){
                    res.Add(ShieldMode.RANDOM);
                    randomShieldAdded = true;
                    continue;
                }
            }
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

        if(addRandomShield && !randomShieldAdded) res[shieldCount-1] = ShieldMode.RANDOM;

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