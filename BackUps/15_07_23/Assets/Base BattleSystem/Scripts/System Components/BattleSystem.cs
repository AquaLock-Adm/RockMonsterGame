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

// bsvg1
public enum BattleState {SETUP, START, PLAYERTURN, QUEUE,  DIALOGUE, WAVEOVER, ATTACKRUSH, PLAYERDIED, ENEMYDIED, RESULT} // bsvg1

public class BattleSystem : MonoBehaviour
{
    [Header("Dungeon Run Variables")]
    public int enemyDefeatCount = 0;
    public int wavesClearedCount = 0;
    public bool runWithoutGameHandler = false;

    public WaveScript WaveScript;
    public bool useWaveScript = false;

    public bool defendModeActive = false;

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

    public List<List<EnemySettings>> EnemyLibrary = new List<List<EnemySettings>>(); // Lib[0] => Stage 1 Enemies

    [Header("System Counters")]
    
    public int earnedCredits = 0;

    public bool inAttackRushPreLoop = false;
    public bool attackRushUsed = false;
    // bsvg8

    [Header("Former BattleHUD")]
    // bsvg9
    [SerializeField] public Text PlayerNameText;
    [SerializeField] public Slider PlayerHpSlider;
    [SerializeField] public Slider PlayerShieldSlider;
    [SerializeField] public Slider PlayerManaSlider;
    // bsvg9

    public Text KillCountText; // bsvg10
    public Text WaveCountText;

    [Header("Other System References")]
    // bsvg11
    public GameHandler GameHandler;

    public EnemySpawner EnemySpawner;
    public BattleMenuHandler BattleMenuHandler;
    public ActionQueue ActionQueue;
    public PauseMenu PauseMenu;
    public ResultHandler ResultHandler;
    public WaveRandomizer WaveRandomizer;

    [Header("Screen References")]

    public GameObject MainMenu;
    public GameObject DescriptionScreen;
    public GameObject ResultScreen;
    public GameObject DialogueScreen;
    public GameObject InputDarkFilter;
    public GameObject PauseScreen;


    [Header("For Testing")]

    [SerializeField] private int[] log1;  // counts of Elements in ElementsToCombine (see RunTests())
    //[SerializeField] public List<SpellElement> log2; // empty 
    //[SerializeField] private List<int> log3; // empty
    // bsvg16

    [SerializeField] private bool setWeaponStartLevel = false;
    [SerializeField] private int testWeaponStartLevel = 1;


#region Unity Functions
    private void Awake(){
        GameStart();
    }
    private void OnApplicationQuit(){

    }
#endregion

    public void GameStart(){
        this.state = BattleState.SETUP;
        CheckDisplays();
        SetGameHandler();
        SetupEverything();
        if(this.Player != null){
            PreStartActions();
        }
    }

    private void SetGameHandler(){
        GameObject GameHandler_GO = GameObject.Find("GameHandler");
        if(GameHandler_GO != null) {
            this.GameHandler = GameHandler_GO.GetComponent<GameHandler>();
            this.runWithoutGameHandler = false;
        }else this.runWithoutGameHandler = true;
    }

    private void CheckDisplays(){
        this.DescriptionScreen.SetActive(false);
        this.ResultScreen.SetActive(false);
        this.DialogueScreen.SetActive(false);
        this.InputDarkFilter.SetActive(false);

        this.MainMenu.SetActive(true);
        this.PauseScreen.SetActive(true);
    }

    public virtual void PreStartActions(){ // Changed in: NBS
        RunTests();
        // this.PauseMenu.ShowPauseMenu(true);
    }

    public virtual void RunTests(){
        /*
        SpellElement AttackElement = SpellElement.ARCANUM;
        SpellElement DefendElement = SpellElement.EARTH;

        int res = ElementWeakTo(AttackElement, DefendElement);

        Debug.Log(res);*/
    }

    public virtual void SetupEverything(){ // Changed in: NBS
        if(this.runWithoutGameHandler) SetupStandartSettings();
        else ImportStartSettingsFromGameHandler();

        if(this.Player != null){
            SetupCurrentPlayer();
            SetupSystemComponents();
        }else Debug.LogError("Player has not been set during Setup.");
    }

    private void SetupStandartSettings(){
        if(this.TestRunPlayerPrefab == null){
            Debug.LogError("TestRunPlayerPrefab must be set for a run without GameHandler!");
            return;
        }
        Debug.Log("Starting without GameHandler.");

        this.Player = this.TestRunPlayerPrefab.GetComponent<PlayerCharacter>();
        Weapon StandartWeapon = this.TestRunWeaponPrefab.GetComponent<Weapon>();
        Armor StandartArmor = this.TestRunArmorPrefab.GetComponent<Armor>();

        this.Player.SetWeapon(StandartWeapon);
        this.Player.SetArmor(StandartArmor); 

        StandartWeapon.Init(this.Player);
        StandartArmor.Init(this.Player);
    }

    private void ImportStartSettingsFromGameHandler(){
        this.GameHandler.BattleSystem = this;
        this.Player = this.GameHandler.Player;
    }

#region SetCurrentPlayer Functions
    private void SetupCurrentPlayer(){
        GameObject P_GO = CreatePlayerGO();
        this.Player = P_GO.GetComponent<PlayerCharacter>();
        this.Player.BattleSetup(this);

        if(this.setWeaponStartLevel) this.Player.GetWeapon().GetUpgrade(this.testWeaponStartLevel);
    }
    private GameObject CreatePlayerGO(){
        GameObject P_GO = new GameObject();
        P_GO.AddComponent<PlayerCharacter>();
        P_GO.GetComponent<PlayerCharacter>().CopyFrom(this.Player);
        P_GO.name = this.Player.GetUnitName();
        return P_GO;
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
        SetActionQueueReferenz();
        SetPauseMenuReferenz();
        SetResultHandlerReferenz();
        if(!this.useWaveScript) SetWaveRandomizerReferenz();
    }
    private void SetupComponentsInnerReferenzes(){
        // SetEnemySpawnerInnerReferenz(); // EnemySpawner Setup happens later in SetupEnemies;
        SetBattleMenuHandlerInnerReferenzes();
        SetActionQueueInnerReferenzes();
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
    public void SetActionQueueReferenz(){
        GameObject ActionQueueGameObject = GetSystemComponentGameObject("ActionQueue");
        this.ActionQueue = ActionQueueGameObject.GetComponent<ActionQueue>();
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
        this.BattleMenuHandler.ActionQueue = this.ActionQueue;
        this.BattleMenuHandler.PauseMenu = this.PauseMenu;

        this.BattleMenuHandler.Setup(this);
    }
    public void SetActionQueueInnerReferenzes(){
        this.ActionQueue.Setup(this);
        this.ActionQueue.BattleMenuHandler = this.BattleMenuHandler;
    }
    private void SetPauseMenuInnerReferenzes(){
        this.PauseMenu.Setup(this);
    }
    private void SetResultHandlerInnerReferenzes(){
        this.ResultHandler.Setup(this);
    }
    private void SetWaveRandomizerInnerReferenzes(){
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
        }else {
            EnemySettingsList = this.WaveRandomizer.GetEnemySettingsList();
        }
        this.EnemySpawner.Setup(EnemySettingsList);
    }
#endregion

#region Round Start Functions
    public void NextWave(){
        this.state = BattleState.START;
        ActionQueue.heatChargeDone = true;
        if(this.Player.GetWeapon().actionsPerRound >= 5) ActionQueue.heatChargeDone = false;
        this.attackRushUsed = false;
        this.BattleMenuHandler.LoadMainMenu();
        SetupEnemySpawner();
        this.EnemySpawner.SpawnNextEnemy();
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

    public void SwitchPlayerMode(){
        // if(!this.defendModeActive) Debug.Log("Switching BS to Defend Mode");
        // else Debug.Log("Switching BS to Attack Mode");
        
        this.defendModeActive = !this.defendModeActive;
        this.Enemy.SwitchModes(this.defendModeActive);
        ActionQueue.SwitchModes(this.defendModeActive);
        BattleMenuHandler.LoadMainMenu();
    }

#region Input Functions during Player Turn 
    public virtual void CastAttack(int attackIndex){
        List<Action> PlayerAbilities = this.Player.GetAbilityList();
        if(!IndexInBoundsOfList(attackIndex, PlayerAbilities.Count)){
            Debug.LogError("Attack index is not in Bounds of Player.Abilities");
            return;
        }

        Action A = PlayerAbilities[attackIndex].Copy();
        ActionQueue.AddAction(A);
    }
    public void CastBlock(int blockIndex){
        List<Action> blocks = this.Player.GetBlockAbilities();
        if(!IndexInBoundsOfList(blockIndex, blocks.Count)){
            Debug.LogError("Attack index is not in Bounds of Player.Blocks");
            return;
        }

        Action A = blocks[blockIndex].Copy();
        ActionQueue.AddAction(A);
    }
    public virtual void CancelLastAction(bool gainApBack = true){

        if(ActionQueue.Actions.Count > 0) {

            Action A = ActionQueue.RemoveLastAction();
        }
    }
    public virtual async void PassRound(){
        this.inAttackRushPreLoop = false;
        if(this.state == BattleState.START) {
            ActionQueue.heatChargeDone = true;
        }
        this.state = BattleState.QUEUE;
        await ActionQueue.ExecuteAllActions();
    }
#endregion

#region Enemy Dies Functions
    public void EnemyDied(Enemy E){
        UpdateKillCount();
        UpdateEnemyKillRewards(E);
        if(this.inBossWave && !this.afterFinalWave) UpdateStageClearRewards();
        // CheckEnemyItemDrop(E);
        this.state = BattleState.PLAYERTURN;
        this.EnemySpawner.SpawnNextEnemy();
    }
    private void UpdateKillCount(){
        this.enemyDefeatCount++;
        this.KillCountText.text = this.enemyDefeatCount.ToString();
    }
    private void UpdateEnemyKillRewards(Enemy E){
        this.earnedCredits += E.GetKillPrice();
        this.ResultHandler.ScoreText.text = this.ResultHandler.FormatIntegerCount(this.earnedCredits) + " Cd";
    }
    private void UpdateStageClearRewards(){
        if(!this.useWaveScript){
            if(!this.stagesClearedBefore[this.WaveRandomizer.stageIndex]){
                this.earnedCredits += this.stageFirstClearRewards[this.WaveRandomizer.stageIndex];
                this.ResultHandler.ScoreText.text = this.ResultHandler.FormatIntegerCount(this.earnedCredits) + " Cd";
                this.stagesClearedBefore[this.WaveRandomizer.stageIndex] = true;
            }
        }else Debug.Log("Not implemented yet!");
    }
    private void CheckEnemyItemDrop(Enemy E){
        int ran = Random.Range(1, 101);
        if(E.GetItemDropChance() <= ran) {
            Debug.Log("Item dropped here");
        }
    }
    public async void WaveOver(){
        this.ActionQueue.StopQueue();
        this.state = BattleState.WAVEOVER;
        this.wavesClearedCount++;
        this.WaveCountText.text = this.wavesClearedCount.ToString();

        int waveOverHeal = (int)Mathf.Round(((float)this.Player.GetMaxHealthPoints()/100.0f) * this.waveClearedHeal_p);
        this.Player.Heal(waveOverHeal);

        await Task.Delay(2000);

        ActionQueue.ResetHeatLevel();

        PauseMenu.ShowPauseMenu(true);
    }
#endregion

    public async void PlayerDies(){
        this.state = BattleState.PLAYERDIED;

        this.ActionQueue.stopQueue = true;
        this.ActionQueue.ClearActionQueue(); // not really necessary

        await Task.Delay(1000);

        this.PauseMenu.ShowPauseMenu(true);
    }

    public void End(){
        if(!this.runWithoutGameHandler){
            this.GameHandler.earnedCredits = this.earnedCredits;
            SceneManager.LoadScene("Main Menu");
        }else {
            Debug.Log("BattleSystem.Quit() called");
            Application.Quit();
        }
    }

#region EnemyLibrary Functions
    public EnemySettings GetEnemySettingsByName(string name, int stageIndex){
        if(stageIndex >= this.EnemyLibrary.Count || stageIndex < 0){
            Debug.LogError("Invalid Stage index!");
            return null;
        }

        foreach(EnemySettings E in this.EnemyLibrary[stageIndex]){
            if(E.name == name) return E;
        }
        Debug.LogError("Could not find Enemy in EnemyLibrary!");
        return null;
    }
    public int GetSettingsIndexByName(string name, int stageIndex){
        if(stageIndex >= this.EnemyLibrary.Count || stageIndex < 0) return -1;
        int res = -1;

        for(int i=0;i<this.EnemyLibrary[stageIndex].Count;i++){
            if(this.EnemyLibrary[stageIndex][i].name == name) return i;
        }
        return res;
    }
    public void UpdateEnemyLibrary(EnemySettings E){
        if(E.level == 0) return;    // Case: Enemy is first enemy of Wave

        if(E.level > this.EnemyLibrary.Count){
            Debug.LogError("Stage for Enemy not initialized!");
            return;
        }

        for(int i=0;i<this.EnemyLibrary[E.level-1].Count; i++){
            if(this.EnemyLibrary[E.level-1][i].name == name){
                this.EnemyLibrary[E.level-1][i] = E;
            }
        }
    }
#endregion

#region AttackRush Functions
    public async void StartAttackRushQueue(){
        if(this.attackRushUsed || ActionQueue.comboLevel < 5) return;
        this.inAttackRushPreLoop = true;
        int waitBeforeAttackRushQueued = 1000;
        int currentWait = 0;
        while (currentWait < waitBeforeAttackRushQueued && this.inAttackRushPreLoop){
            currentWait += 50;
            await Task.Delay(50);
        }
        if(this.inAttackRushPreLoop) this.ActionQueue.QueueAttackRush();
        this.inAttackRushPreLoop = false;
    }
    public void StopAttackRushQueue(){
        this.inAttackRushPreLoop = false;
    }
#endregion

#region Ability Helper Functions
    public int GetAbilityIndexByString(string givenActionName){
        Action ActionFromPlayerAbilityList = this.Player.GetAbilityList().Find(action => action.name == givenActionName);

        if(ActionFromPlayerAbilityList != null) return ActionFromPlayerAbilityList.abilityIndex;
        else {
            Debug.LogError("Ability Name not found in Player Abilities");
            return -1;
        }
    }
#endregion
    
    public bool IndexInBoundsOfList(int index, int listSize){

        return index < listSize && index >= 0;
    }
} // EOF