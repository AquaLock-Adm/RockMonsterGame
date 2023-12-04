/*
    Hauptstück des gesamten Battle ablaufs. ALLE prozesse die gestartet werden kommen entweder hier vorbei oder werden größtenteils auch von hier gestartet.
    Viele wichtige daten und referenzen werden hier gehalten und können per referenz auch von hier geholt werden.
*/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum BattleState {SETUP, RUNNING, DIALOGUE, WAVEOVER, PLAYERDIED, ENEMYDIED, RESULT} // bsvg1


public class BattleSystem : MonoBehaviour
{
    [SerializeField] private GameObject GameHandlerPrefab;
    [Header("Dungeon Run Variables")]
    public bool useBattleSpeed = false;
    public int enemyDefeatCount = 0;
    public int wavesClearedCount = 0;
    public int startStage = 1;

    public WaveScript WaveScript;
    public bool useWaveScript = false;

    public bool inBossWave = false;
    public bool finalStageReached = false;
    public bool afterFinalWave = false;

    [SerializeField] private bool[] stagesClearedBefore = new bool[5];
    [SerializeField] private int[] stageFirstClearRewards = {
        5000,
        0,
        0,
        0,
        0
    };

    [SerializeField] private int waveClearedHeal_p = 15; // heal 15% of maxHp after a wave

    // bsvg2

    [Header("State")]
    public BattleState state; // bsvg3

    [Header("Unit Variables")]

    public int standartTimeBetweenSpawnes = 2000; // ms

    public GameObject TestRunPlayerPrefab;
    public GameObject TestRunWeaponPrefab;
    public GameObject TestRunArmorPrefab;

    [SerializeField] public PlayerCharacter Player;
    [SerializeField] public Enemy Enemy;

    [Header("System Counters")]
    public int earnedCredits = 0;

    private BattleState stateBeforeDialogueSwitch;
    private PlayerState playerStateBeforeDialogueSwitch;
    // bsvg8

    [Header("Former BattleHUD")]
    // bsvg9
    [SerializeField] public TextMeshProUGUI PlayerNameText;
    [SerializeField] public Slider PlayerHpSlider;
    [SerializeField] public Slider PlayerManaSlider;
    // bsvg9

    [SerializeField] private GameObject ActionHUD;
    public GameObject ActionBoxPrefab;

    public Text KillCountText; // bsvg10
    public Text WaveCountText;

    [Header("Other System References")]
    // bsvg11
    public GameHandler GameHandler;

    public EnemySpawner EnemySpawner;
    public BattleMenuHandler BattleMenuHandler;
    public PauseMenu PauseMenu;
    public ResultHandler ResultHandler;
    public WaveRandomizer WaveRandomizer;

    [Header("Screen References")]

    public GameObject MainMenu;
    public GameObject ResultScreen;
    public GameObject InputDarkFilter;
    public GameObject PauseScreen;


    [Header("For Testing")]

    [SerializeField] private int[] log1;  // counts of Elements in ElementsToCombine (see RunTests())
    //[SerializeField] public List<SpellElement> log2; // empty 
    //[SerializeField] private List<int> log3; // empty
    // bsvg16

    [SerializeField] private bool setWeaponStartLevel = false;
    [SerializeField] private int testWeaponStartLevel = 1;

    [SerializeField] private bool setStartStage = false;
    [SerializeField] private int testStartStage = 1;



#region Unity Functions
    private void Awake(){
        GameObject GHGO = GameObject.Find("GameHandler");

        if(GHGO == null){
            if(this.GameHandlerPrefab == null){
                Debug.LogError("No Game Handler Prefab Set!");
                return;
            }

            GHGO = Instantiate(this.GameHandlerPrefab);
            GHGO.name = "GameHandler";
        }
    }

    private void OnApplicationQuit(){

    }
#endregion



    public void GameStart(GameHandler GH){
        this.GameHandler = GH;
        this.state = BattleState.SETUP;
        CheckEnemyLibraryInit();
        CheckDisplays();
        SetupEverything();
        PreStartActions();
    }

    private void CheckDisplays(){
        this.ResultScreen.SetActive(false);
        this.InputDarkFilter.SetActive(false);

        this.MainMenu.SetActive(true);
        this.PauseScreen.SetActive(true);
    }

    protected virtual void PreStartActions(){ // Changed in: NBS
        RunTests();
        // this.PauseMenu.ShowPauseMenu(true);
    } // Changed in: BattleSystem_Tutorial.cs

    public virtual void RunTests(){
        /*
        SpellElement AttackElement = SpellElement.ARCANUM;
        SpellElement DefendElement = SpellElement.EARTH;

        int res = ElementWeakTo(AttackElement, DefendElement);

        Debug.Log(res);*/
    }

    protected virtual void SetupEverything(){ // Changed in: NBS
        if(this.GameHandler.Player == null) CreateNewPlayerGameObjects();
        else this.Player = this.GameHandler.Player;

        if(this.Player != null){
            SetupSystemComponents();
            SetupCurrentPlayer();
        }else Debug.LogError("Player has not been set during Setup.");
    } // Changed in: BattleSystem_Tutorial.cs

    protected void CreateNewPlayerGameObjects(){
        this.Player = Instantiate(this.TestRunPlayerPrefab).GetComponent<PlayerCharacter>();
        Weapon StandartWeapon = this.TestRunWeaponPrefab.GetComponent<Weapon>();
        Armor StandartArmor = this.TestRunArmorPrefab.GetComponent<Armor>();

        this.Player.SetWeapon(StandartWeapon);
        this.Player.SetArmor(StandartArmor); 

        StandartWeapon.Init(this.Player);
        StandartArmor.Init(this.Player);
    }



#region SetCurrentPlayer Functions
    protected void SetupCurrentPlayer(){
        if(this.setWeaponStartLevel) {
            this.Player.BattleSetup(this, this.ActionHUD, this.testWeaponStartLevel);
        }else this.Player.BattleSetup(this, this.ActionHUD);
    }
#endregion



#region SetupSystemComponents Functions
    public virtual void SetupSystemComponents(){ // Changed in: NBS
        SetBattleSystemsComponentReferenzes();
        if(!Application.isPlaying) return;

        SetupComponentsInnerReferenzes();
    }
    private void SetBattleSystemsComponentReferenzes(){
        SetEnemySpawnerReferenz();
        SetBattleMenuHandlerReferenz();
        SetPauseMenuReferenz();
        SetResultHandlerReferenz();
        if(!this.useWaveScript) SetWaveRandomizerReferenz();
    }
    private void SetupComponentsInnerReferenzes(){
        // SetEnemySpawnerInnerReferenz(); // EnemySpawner Setup happens later in SetupEnemies;
        SetBattleMenuHandlerInnerReferenzes();
        SetPauseMenuInnerReferenzes();
        SetResultHandlerInnerReferenzes();
        if(!this.useWaveScript) SetWaveRandomizerInnerReferenzes();
    }

    public void SetEnemySpawnerReferenz(){
        GameObject EnemySpawnerGameObject = GetSystemComponentGameObject("EnemySpawner");
        this.EnemySpawner = EnemySpawnerGameObject.GetComponent<EnemySpawner>();
    }
    public void SetBattleMenuHandlerReferenz(){
        GameObject BattleMenuHandlerGameObject = GetSystemComponentGameObject("BattleMenuHandler");
        this.BattleMenuHandler = BattleMenuHandlerGameObject.GetComponent<BattleMenuHandler>();
    }
    private void SetPauseMenuReferenz(){
        GameObject PauseMenuGameObject = GetSystemComponentGameObject("PauseMenu");
        this.PauseMenu = PauseMenuGameObject.GetComponent<PauseMenu>();
    }
    private void SetResultHandlerReferenz(){
        GameObject ResultHandlerGameObject = GetSystemComponentGameObject("ResultHandler");
        this.ResultHandler = ResultHandlerGameObject.GetComponent<ResultHandler>();
    }
    private void SetWaveRandomizerReferenz(){
        GameObject WaveRandomizerGameObject = GetSystemComponentGameObject("WaveRandomizer");
        if(WaveRandomizerGameObject != null){
            this.WaveRandomizer = WaveRandomizerGameObject.GetComponent<WaveRandomizer>();
        }
    }

    public virtual void SetBattleMenuHandlerInnerReferenzes(){ // Changed in: NBS
        this.BattleMenuHandler.PauseMenu = this.PauseMenu;

        this.BattleMenuHandler.Setup(this);
    }
    private void SetPauseMenuInnerReferenzes(){
        this.PauseMenu.Setup(this);
    }
    private void SetResultHandlerInnerReferenzes(){
        this.ResultHandler.Setup(this);
    }
    private void SetWaveRandomizerInnerReferenzes(){
        if(setStartStage){
            if(testStartStage > WaveRandomizer.MAX_STAGES_CURRENTLY_AVAILABLE){
                Debug.LogError("Error: MAX_STAGES_CURRENTLY_AVAILABLE: "+WaveRandomizer.MAX_STAGES_CURRENTLY_AVAILABLE.ToString());
                testStartStage = startStage;
            }
            startStage = testStartStage;
        }
        this.WaveRandomizer.Setup(this);
    }
    private GameObject GetSystemComponentGameObject(string componentName){
        GameObject Res;
        if((Res = GameObject.Find(componentName)) == null){
            Debug.LogError("Couldn't find SystemComponent "+componentName+"!");
            Application.Quit();
        }
        return Res;
    }
#endregion



#region SetupEnemySpawner Functions
    private void SetupEnemySpawner(){
        if(!this.useWaveScript && this.WaveRandomizer == null) {
            Debug.LogError("ERROR: WaveRandomizer Referenz missing! Forgot to turn on Wavescript?");
            Debug.LogError("ERROR: Quitting application...");
            Application.Quit();
            return;
        }

        if(this.useWaveScript && this.WaveScript != null) {
            this.WaveScript.Setup(this.wavesClearedCount);
            if(this.wavesClearedCount >= this.WaveScript.waveCount-1) this.inBossWave = true;
        }else if(this.useWaveScript && this.WaveScript == null){
            Debug.LogError("Error: WaveScript was not given!");
            Debug.Log("Continuing with randomized wave...");
            this.useWaveScript = false;
            SetWaveRandomizerReferenz();

            if(this.WaveRandomizer != null) SetWaveRandomizerInnerReferenzes();
            else {
                Debug.LogError("ERROR: No way to spawn enemies!");
                Debug.LogError("ERROR: Quitting application...");
                if(Application.isPlaying) Application.Quit();
                return;
            }
        }

        List<EnemySettings> EnemySettingsList = new List<EnemySettings>();

        if(this.useWaveScript) {
            EnemySettingsList = this.WaveScript.GetEnemySettingsList();
            foreach(EnemySettings E in EnemySettingsList){
                if(GetEnemyLibraryStageCount() < E.level) InitNewEnemyLibraryStage(E.level-1, new List<EnemySettings>());
                GameHandler.AddNewEnemyToLibrary(E);
            }
        }else {
            EnemySettingsList = this.WaveRandomizer.GetEnemySettingsList();
        }
        this.EnemySpawner.Setup(EnemySettingsList);
    }
#endregion



#region BattleSequence Functions
    public void PassRound(){
        Player.StartActionQueue();
    }
    public void NextRound(){
        if(this.useBattleSpeed){
            if(Player.battleSpeed >= Enemy.battleSpeed){
                // Debug.Log("Player Attack Turn.\n"+Player.battleSpeed.ToString()+","+Enemy.battleSpeed.ToString());
                Player.battleSpeed -= Enemy.battleSpeed;
                Enemy.SwitchBattleModes(false);
                Player.SwitchBattleModes(false);
            }else{
                // Debug.Log("Player Defend Turn.\n"+Player.battleSpeed.ToString()+","+Enemy.battleSpeed.ToString());
                Player.battleSpeed += Player.baseBattleSpeed;
                Enemy.SwitchBattleModes(true);
                Player.SwitchBattleModes(true);
            }
            Player.UpdateNextRoundModeInfo();  
        }else{
            Enemy.SwitchBattleModes(!Player.defendModeActive);
            Player.SwitchBattleModes(!Player.defendModeActive);
        }
        
        // New Round Starts Now !
    }
#endregion



#region Round Start Functions
    public void NextWave(){
        this.Player.BlockAllInputsFor(100);
        this.Player.NextWave();

        SetupEnemySpawner();
        this.EnemySpawner.SpawnNextEnemy();
        this.Player.state = PlayerState.START;
        NextRound();
        this.state = BattleState.RUNNING;
    }

    public void NextStage(){
        if(!this.useWaveScript) {
            this.state = BattleState.SETUP;
            this.PauseMenu.ShowPauseMenu(true);
            this.WaveRandomizer.NextStage();
        }
        else Debug.Log("NOT ADDED YET!");
    }

    public int GetNextWaveSize(){
        return this.WaveRandomizer.GetNextWaveSize();
    }
#endregion



#region Enemy Dies Functions
    public void EnemyDied(Enemy E){
        UpdateKillCount();
        UpdateEnemyKillRewards(E);
        if(this.inBossWave && !this.afterFinalWave) UpdateStageClearRewards();
        // CheckEnemyItemDrop(E);
        Player.ResetBattleSpeed();
        this.Player.state = PlayerState.PLAYERTURN;
        this.state = BattleState.RUNNING;
        this.EnemySpawner.SpawnNextEnemy();

        if(this.state != BattleState.WAVEOVER){
            Player.defendModeActive = true;
            NextRound();
        }
    }

    private void UpdateKillCount(){
        this.enemyDefeatCount++;
        this.KillCountText.text = this.enemyDefeatCount.ToString();
    }

    private void UpdateEnemyKillRewards(Enemy E){
        this.earnedCredits += E.GetKillPrice();
        this.ResultHandler.ScoreText.text = this.ResultHandler.FormatIntegerCount(this.earnedCredits) + " Cd";
    }

    protected virtual void UpdateStageClearRewards(){
        if(!this.useWaveScript){
            if(!this.stagesClearedBefore[this.WaveRandomizer.stageIndex]){
                this.earnedCredits += this.stageFirstClearRewards[this.WaveRandomizer.stageIndex];
                this.ResultHandler.ScoreText.text = this.ResultHandler.FormatIntegerCount(this.earnedCredits) + " Cd";
                this.stagesClearedBefore[this.WaveRandomizer.stageIndex] = true;
            }
        }else Debug.Log("Not implemented yet!");
    } // Changed in: 

    private void CheckEnemyItemDrop(Enemy E){
        int ran = Random.Range(1, 101);
        if(E.GetItemDropChance() <= ran) {
            Debug.Log("Item dropped here");
        }
    }

    public async void WaveOver(){
        this.Player.StopQueue();
        this.state = BattleState.WAVEOVER;
        this.Player.state = PlayerState.WAITING;
        this.wavesClearedCount++;
        this.WaveCountText.text = this.wavesClearedCount.ToString();

        int waveOverHeal = (int)Mathf.Round(((float)this.Player.maxHealthPoints/100.0f) * this.waveClearedHeal_p);
        this.Player.Heal(waveOverHeal);

        await Task.Delay(2000);

        this.Player.ResetHeatLevel();
        
        PauseMenu.ShowPauseMenu(true);
    }
#endregion



    public void SwitchDialogueState(bool on){
        // Debug.Log("SwitchDialogueState() called");
        if(on){
            if(this.state != BattleState.DIALOGUE){
                this.stateBeforeDialogueSwitch = this.state;
                this.playerStateBeforeDialogueSwitch = this.Player.state;
            }
            this.state = BattleState.DIALOGUE;
            this.Player.state = PlayerState.WAITING;
        }else{
            this.state = this.stateBeforeDialogueSwitch;
            this.Player.state = this.playerStateBeforeDialogueSwitch;
        }
    }

    // list bools in order WASD
    public void BlockPlayerInputs(List<bool> playerInputBlockList){
        this.Player.BlockPlayerInputs(playerInputBlockList);
    }

    public async void PlayerDies(){
        this.state = BattleState.PLAYERDIED;

        await Task.Delay(1000);

        this.PauseMenu.ShowPauseMenu(true);
    }

    public virtual void End(){
        Player.BattleEnd();
        this.GameHandler.earnedCredits += this.earnedCredits;
        this.GameHandler.LoadMainMenu();
    } // Changed in: BattleSystem_Tutorial.cs



#region EnemyLibrary Functions
    public void CheckEnemyLibraryInit(){
        if(!GameHandler.EnemyLibraryInitialized()){
            GameHandler.InitEnemyLibrary();
        }
    }

    public bool EnemyLibraryEmpty(){
        return GameHandler.EnemyLibraryEmpty();
    }

    public EnemySettings GetEnemySettingsByName(string name, int stageIndex){
        return GameHandler.GetEnemySettingsByName(name, stageIndex);
    }

    public List<EnemySettings> GetAllEnemiesFromStage(int stageIndex){
        return GameHandler.GetAllEnemiesFromStage(stageIndex);
    }

    public int GetSettingsIndexByName(string name, int stageIndexOfSettings){
        return GameHandler.GetSettingsIndexByName(name, stageIndexOfSettings);
    }

    public int GetEnemyLibraryStageCount(){
        return GameHandler.GetEnemyLibraryStageCount();
    }

    public void InitNewEnemyLibraryStage(int stageIndex, List<EnemySettings> NewEnemies){
        GameHandler.InitNewEnemyLibraryStage(stageIndex, NewEnemies);
    }

    public void UpdateEnemyLibraryEntryOf(EnemySettings E){
        GameHandler.UpdateEnemyLibraryEntryOf(E);
    }
#endregion
} // EOF