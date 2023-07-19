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
public enum BattleState {SETUP, START, PLAYERTURN, QUEUE,  DIALOGUE, WAVEOVER, BAILOUT, PLAYERDIED, RESULT} // bsvg1

public class BattleSystem : MonoBehaviour
{
    [Header("Dungeon Run Variables")]
    // bsvg2
    [SerializeField] public int entryCost;
    [SerializeField] public int monsterWaveSize;
    [SerializeField] public int moveCost;
    [SerializeField] public int deathCost;
    [SerializeField] public bool useDialogueScript;
    [SerializeField] public bool useWaveScript;
    // bsvg2

    [Header("State")]
    public BattleState state; // bsvg3

    public bool stopTextCrawl = false; // bsvg4

    [Header("Unit Variables")]
    // bsvg5
    public List<GameObject> EnemyPrefabs;
    [SerializeField] public PlayerCharacter Player;
    [SerializeField] public Enemy Enemy;
    [SerializeField] public Enemy NextEnemy;
    public List<Enemy> EnemyList = new List<Enemy>();
    // bsvg5

    public List<Action> TestAbilities = new List<Action>(); // bsvg6

    [Header("System Counters")]
    // bsvg8
    public int enemyDefeatCount = 0;
    public int enemyWaveIndex = 0;
    
    public int earnedCredits = 0;
    public int earnedExp = 0;

    public int wavesClearedCount = 0;

    public int movesUsedCount = 0;

    public int itemsUsedCount = 0;
    public int totalItemCost = 0;
    //[SerializeField] private int actionScore = 0;
    //[SerializeField] private int maxActionScore = 35;

    public int totalDamageThisRound = 0;
    //public int bonusActionPointGain = 0;
    // bsvg8

    [Header("Battle Multipliers")]
    [SerializeField] public float critMultiplier = 1.5f;
    [SerializeField] public float heavyAttackMultiplier = 1.2f;
    [SerializeField] public float specialAttackMultiplier = 1.1f;
    [SerializeField] public float ultAbilityDamageMultiplier = 1.1f;

    [SerializeField] public float attackIntoShieldMultiplier = 0.9f;

    [SerializeField] public float spellStrongMultiplier = 1.5f;
    [SerializeField] public float spellWeakMultiplier = 0.75f;

    [Header("Former BattleHUD")]
    // bsvg9
    [SerializeField] public Text PlayerNameText;
    [SerializeField] public Slider PlayerHpSlider;
    [SerializeField] public Slider PlayerShieldSlider;
    [SerializeField] public Slider PlayerManaSlider;
    // bsvg9


    public Text KillCountText; // bsvg10


    [Header("Other System References")]
    // bsvg11
    public GameHandler GameHandler;

    //public BattleHUD BattleHUD;
    public EnemySpawner EnemySpawner;
    public BattleMenuHandler BattleMenuHandler;
    public ActionQueue ActionQueue;
    public PauseMenu PauseMenu;
    public ResultHandler ResultHandler;
    public DialogueHandler DialogueHandler;
    public WaveScript WaveScript;

    public GameObject MainMenu;
    // bsvg11

    // bsvg12
    
    [Header("Ability Decay aka Spam Protection")]
    // bsvg13
    [SerializeField] public int[] AbilityDecayArray;
    [SerializeField] private float abilityDecayMultiplierMax = 1.05f;
    [SerializeField] private float abilityDecayMultiplierMin = 0.55f;
    [SerializeField] public int abilityDecayLevelCount = 20;
    //[SerializeField] private int abilityDecayLevelLoss = 1;
    //[SerializeField] private int abilityDecayLevelGain = 1;
    // bsvg13

    [Header("Element Matrix")]
    private int[,] ElementWeaknessMatrix = new int[,] // bsvg15
                                            {
                                            { 0,  0,  1, -1},
                                            { 0,  0, -1,  1},
                                            {-1,  1,  0,  0},
                                            { 1, -1,  0,  0},

                                            {-1,  1,  1, -1},
                                            { 1, -1,  1, -1},
                                            {-1,  1, -1,  1},
                                            { 1, -1, -1,  1},

                                            {-1,  0,  0,  1},
                                            { 1,  0,  0, -1},
                                            { 0, -1,  1,  0},
                                            { 0,  1, -1,  0},
                                            };

    [Header("For Testing")]
    // bsvg16
    public bool disableAbilityDecay = false;

    public bool disableHeat = false;

    public bool disableTestCombo = false;

    [SerializeField] public bool disableApCost = false;

    [SerializeField] private int[] log1;  // counts of Elements in ElementsToCombine (see RunTests())
    //[SerializeField] public List<SpellElement> log2; // empty 
    //[SerializeField] private List<int> log3; // empty
    // bsvg16

#region Unity Functions
    private void Awake(){
        if(SceneManager.GetActiveScene().name != "Main Menu Scene"){
            this.state = BattleState.SETUP;
            SetupEverything();
            PreStartActions();
            // the beginning pause menu unlocks the Enemy attack loop and calls PlayerTurn()
        }
    }
    private void OnApplicationQuit(){
        
        ClearEnemyList();
    }
#endregion

    public virtual void PreStartActions(){ // Changed in: NBS
        RunTests();
        this.PauseMenu.ShowPauseMenu(true);
    }

    public virtual void SetupEverything(){ // Changed in: NBS
        ImportStartSettingsFromGameHandler();
        SetupSystemComponents();
        if(this.useWaveScript) SetupWaveScript();
        SetupAllUnits();
        /*
        SetupCurrentPlayer();
        EnemySpawnSetup(this);
        */
        if(this.useDialogueScript) SetupDialogueScript();
    }

    private void ImportStartSettingsFromGameHandler(){
        GameObject GameHandler_GO;
        if((GameHandler_GO = GameObject.Find("Game Handler")) == null){
            GameHandler_GO = GameObject.Find("GameHandler_Test");
            if(GameHandler_GO == null){
                Debug.LogError("Error: NO GameHandler found!");
            }
        }

        this.GameHandler = GameHandler_GO.GetComponent<GameHandler>();
        this.GameHandler.BattleSystem = this;

        Dungeon CurrentDungeon = this.GameHandler.CurrentDungeon;

        this.entryCost = CurrentDungeon.entryCost;
        this.monsterWaveSize = CurrentDungeon.waveSize;
        this.moveCost = CurrentDungeon.moveCost;
        this.deathCost = CurrentDungeon.deathCost;

        this.useWaveScript = CurrentDungeon.useWaveScript;
        this.useDialogueScript = CurrentDungeon.useDialogueScript;

        this.Player = this.GameHandler.Player;
        this.EnemyPrefabs = CurrentDungeon.EnemyPrefabs;
    }       // TODO: Taking EnemyPrefabs prefab from other scene doesnt work!

    public virtual void RunTests(){
        /*
        SpellElement AttackElement = SpellElement.ARCANUM;
        SpellElement DefendElement = SpellElement.EARTH;

        int res = ElementWeakTo(AttackElement, DefendElement);

        Debug.Log(res);*/
    }

#region SetupSystemComponents Functions
    public virtual void SetupSystemComponents(){ // Changed in: NBS
        SetBattleSystemsComponentReferenzes();
        SetupComponentsInnerReferenzes();
    }
    private void SetBattleSystemsComponentReferenzes(){
        SetEnemySpawnerReferenz();
        SetBattleMenuHandlerReferenz();
        SetActionQueueReferenz();
        SetPauseMenuReferenz();
        SetResultHandlerReferenz();
        SetDialogueHandlerReferenz();
    }
    private void SetupComponentsInnerReferenzes(){
        SetupBattleMenuHandlerInnerReferenzes();
        SetupActionQueueInnerReferenzes();
        SetupPauseMenuInnerReferenzes();
        SetupResultHandlerInnerReferenzes();
        SetupDialogueHandlerInnerReferenzes();
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
    private void SetDialogueHandlerReferenz(){
        GameObject DialogueHandlerGameObject = GetSystemComponentGameObject("DialogueHandler");
        this.DialogueHandler = DialogueHandlerGameObject.GetComponent<DialogueHandler>();
    }
    public virtual void SetupBattleMenuHandlerInnerReferenzes(){ // Changed in: NBS
        this.BattleMenuHandler.BattleSystem = this;
        this.BattleMenuHandler.ActionQueue = this.ActionQueue;
        this.BattleMenuHandler.PauseMenu = this.PauseMenu;
        this.BattleMenuHandler.DialogueHandler = this.DialogueHandler;  
    }
    public void SetupActionQueueInnerReferenzes(){
        
        this.ActionQueue.BattleSystem = this; 
    }
    private void SetupPauseMenuInnerReferenzes(){

        this.PauseMenu.BattleSystem = this;
    }
    private void SetupResultHandlerInnerReferenzes(){

        this.ResultHandler.BattleSystem = this;
    }
    private void SetupDialogueHandlerInnerReferenzes(){
        this.DialogueHandler.BattleSystem = this;
        this.DialogueHandler.ActionQueue = this.ActionQueue; 
    }
    private GameObject GetSystemComponentGameObject(string componentName){
        
        return GameObject.Find(componentName);
    }
#endregion

    private void SetupWaveScript(){
        // spawn wavescript
        this.WaveScript = Instantiate(this.GameHandler.CurrentDungeon.WaveScriptPrefab).GetComponent<WaveScript>();
        this.WaveScript.waveScriptName = this.GameHandler.CurrentDungeon.waveScriptName;
        this.WaveScript.EnemyPrefabs = this.EnemyPrefabs;
        this.WaveScript.Activate();
    }

    public virtual void SetupAllUnits(){ // Changed in NBS
        SetupCurrentPlayer();
        SetupEnemySpawner();
    }

#region SetupEnemySpawner Functions
    private void SetupEnemySpawner(){
        List<GameObject> EnemiesToBeSpawned = GetEnemiesToBeSpawned();
        this.EnemySpawner.Setup(this, EnemiesToBeSpawned);
    }
    private List<GameObject> GetEnemiesToBeSpawned(){
        if(this.useWaveScript && !CurrentWaveIsLastWaveInScript()){
            return SetupEnemyListWithWaveScript();
        }
        else{
            return GetRandomizedEnemyPrefabList();
        }
    }
    private bool CurrentWaveIsLastWaveInScript(){
        
        return this.wavesClearedCount+1 <= WaveScript.waveCount;
    }
    private List<GameObject> SetupEnemyListWithWaveScript(){
        return new List<GameObject>();
        // this.EnemyList = WaveScript.NextWave();
        // foreach (Enemy E in this.EnemyList){
        //     E.Setup();
        // } 
    }                                  // TODO
    private List<GameObject> GetRandomizedEnemyPrefabList(){
        // Erstellt mit GetEnemySpawnValueArray() einen Array mit <enemySpawnValueTotal> elementen, der wert am Punkt enemySpawnValueArray[n] (n -> irgendein Integer) ist der Index von dem EnemyPrefab von der EnemyPrefabs liste
        // dann wird eine zufaellige Zahl z zwischen 0 und <enemySpawnValueTotal - 1> ausgewürfelt und das EnemyPrefab am punkt this.EnemyPrefabList[z] in die randomized Enemy Prefab Liste gepackt.
        
        List<GameObject> RandomizedEnemyPrefabList = new List<GameObject>();

        int enemySpawnValuesTotal = GetEnemySpawnValuesTotal();

        int[] enemySpawnValueArray = GetEnemySpawnValueArray(enemySpawnValuesTotal);

        for(int enemyCount = 0; enemyCount < monsterWaveSize; enemyCount++){
            int spawnValueIndex = Random.Range(0, enemySpawnValuesTotal-1);

            int randomEnemyPrefabIndex = enemySpawnValueArray[spawnValueIndex];
            GameObject RandomEnemyPrefab = this.EnemyPrefabs[randomEnemyPrefabIndex];

            RandomizedEnemyPrefabList.Add(RandomEnemyPrefab);
        }
        return RandomizedEnemyPrefabList;
    }
    private int[] GetEnemySpawnValueArray(int enemySpawnValuesTotal){
        int[] spawnValueArray = new int[enemySpawnValuesTotal];

        for(int enemyIndex = 0; enemyIndex < this.EnemyPrefabs.Count; enemyIndex++){
            Enemy Enemy = this.EnemyPrefabs[enemyIndex].GetComponent<Enemy>();

            for(int count = 0; count < Enemy.spawnValue; count++) spawnValueArray[count] = enemyIndex;
        }
        return spawnValueArray; 
    }
    private int GetEnemySpawnValuesTotal(){
        int total = 0;

        foreach(GameObject EnemyPrefab in this.EnemyPrefabs){
            total += EnemyPrefab.GetComponent<Enemy>().spawnValue;
        }

        return total;
    }
#endregion

#region SetCurrentUnits Functions
    private void SetupCurrentPlayer(){
        GameObject P_GO = CreatePlayerGO();
        this.Player = P_GO.GetComponent<PlayerCharacter>();
        this.Player.Setup(this);

        SetupPlayerAbilities();
        SetupPlayerItems();
    }
    private GameObject CreatePlayerGO(){
        GameObject P_GO = new GameObject();
        P_GO.AddComponent<PlayerCharacter>();
        P_GO.GetComponent<PlayerCharacter>().CopyFrom(this.Player);
        P_GO.name = this.Player.unitName;
        return P_GO;
    }
    private void SetupCurrentEnemy(){
        this.enemyWaveIndex = 0;
        this.Enemy = this.EnemyList[0];
        this.Enemy.Hide(false);
    }                                                         // TODO
    public virtual void SetupPlayerAbilities(){ // Changed in: NBS
        this.TestAbilities = this.Player.Weapon.GetMovesToTest();

        if(!this.disableTestCombo){
            SetupTestCombo();
            Debug.Log(this.TestAbilities.Count.ToString()+" abilities in TestCombo.");
        }

        SetMissingReferencesWithPlayerAbilities();

        // AddWeaponMoveListToPlayerAbilities();

        Debug.Log(this.Player.Abilities.Count.ToString()+" player abilities");
        InitiateAbilityDecayArray();
    }                                               // TODO
    private void SetupPlayerItems(){
        /*Item I = new Item();
        I.type = ItemType.heal;
        I.value = 300;
        I.useCost = 460;

        Player.Items.Add(I);*/

        /*Item R = new Item();
        R.type = ItemType.repair;
        R.value = 2000;
        R.useCost = 250;

        this.Player.Items.Add(R);

        this.Player.CountAllItems();*/
        Debug.Log("Player Items have been set by GameHandler!");
    }
    private void SetupTestCombo(){
        Action A = new TestCombo();
        A.abilityIndex = 1;
        this.Player.Abilities[1] = A;
    }
    public void SetMissingReferencesWithPlayerAbilities(){

        int currentAbilityIndex = 0;

        foreach(Action A in this.Player.Abilities) {
            if(A.comboLevel > 1) {
                ActionQueue.AvailableComboActionsAsStrings.Add(A.comboList);
            }

            A.abilityIndex = currentAbilityIndex;
            currentAbilityIndex++;
            A.SetBattleSystem(this);
        }
    }
    public virtual void InitiateAbilityDecayArray(){
        this.AbilityDecayArray = new int[Player.Abilities.Count];
        foreach(Action A in this.Player.Abilities){
            this.AbilityDecayArray[A.abilityIndex] = this.abilityDecayLevelCount;
        }
    }
#endregion

    private void SetupDialogueScript(){
        DialogueHandler.dialogueScriptName = this.GameHandler.CurrentDungeon.dialogueScriptName;
        DialogueHandler.ActivateDialogueScript();
    }

#region Round Start Functions
    public async void NextWave(){
        this.state = BattleState.START;
        ClearEnemyList();
        SetupEnemySpawner();

        await PlayerTurn();
    }
    // PauseMenu.ContinueGame() calls PlayerTurn() at end of BattleState.SETUP(see above)
    public virtual async Task PlayerTurn(){
        this.state = BattleState.START;
        // await GainAp(this.Player.actionPointGain + this.Player.roundStartActionPointGain);
        // this.Player.ResetActionPointGain();
        await Task.Yield();
        this.state = BattleState.PLAYERTURN;
    }
    private async Task GainAp(int apGain, int totalTime = -1){
        // if(totalTime < 0) totalTime = this.StandartApGainTextShowTime;

        // if(ApGainWouldOverFlowMaxAp(apGain)) apGain = this.maxAp - this.Player.currentActionPoints;
        
        // this.PlayerApGainText.text = "+"+apGain.ToString();

        // int restTime = await WaitBeforeIncrease(totalTime);

        // int[] depletionFactors = CalculateApGainDepletionFactors(apGain, restTime);
        
        // showApGainText(apGain, depletionFactors);

        // await IncreaseApWithDepletionFactors(apGain, depletionFactors);
        Debug.Log("GainAp() got shelved.");
        await Task.Yield();
    }
    private bool ApGainWouldOverFlowMaxAp(int apGain){
        Debug.Log("ApGainWouldOverFlowMaxAp() shelved.");
        return false;
        // return this.Player.currentActionPoints+apGain > this.maxAp;
    }
    private async void showApGainText(int apGain, int[] depletionFactors){

        // int tickRate = depletionFactors[0]; // see CalculateApGainDepletionFactors()
        // int pointsPerTick = depletionFactors[1];
        // int totalTime = depletionFactors[2];

        // int totalApGain = apGain;
        // int timePassed = 0;

        // while(totalApGain >= 0 && this.Player.currentActionPoints <= maxAp){
        //     await Task.Delay(tickRate);
        //     totalApGain -= pointsPerTick;
        //     this.PlayerApGainText.text = "+"+totalApGain.ToString();
        //     timePassed += tickRate;
        // }

        // if(totalApGain != 0) this.PlayerApGainText.text = "+0";

        // await WaitRemainingTime(totalTime, timePassed);

        // this.PlayerApGainText.text = "";
        await Task.Yield();
        Debug.Log("showApGainText() shelved.");
    }
    private async Task<int> WaitBeforeIncrease(int totalTime){

        int waitBeforeIncrease = (int)Mathf.Round(totalTime/2);

        await Task.Delay(waitBeforeIncrease);

        return totalTime - waitBeforeIncrease;
    }
    private int[] CalculateApGainDepletionFactors(int apGain, int time){

        int[] depletionFactors = new int[3];

        int tickRate = 1;
        int pointsPerTick = 1;

        if(apGain > 0){
            int maxGainWithGivenTimeAndTickRate = (int)Mathf.Round(time / tickRate);

            while(apGain < maxGainWithGivenTimeAndTickRate){
                tickRate++;
                maxGainWithGivenTimeAndTickRate = (int)Mathf.Round(time / tickRate);
            }

            pointsPerTick = (int)Mathf.Round(apGain/maxGainWithGivenTimeAndTickRate); 
        }

        //Debug.Log(apGain.ToString()+" Points in "+time.ToString()+"ms: "+tickRate.ToString()+"ms Tick Rate / "+pointsPerTick.ToString()+"ppT");

        depletionFactors[0] = tickRate;
        depletionFactors[1] = pointsPerTick;
        depletionFactors[2] = time;

        return depletionFactors;
    }
    private async Task IncreaseApWithDepletionFactors(int apGain, int[] depletionFactors){
        // int tickRate = depletionFactors[0];             // see CalculateApGainDepletionFactors()
        // int pointsPerTick = depletionFactors[1];
        // int totalTime = depletionFactors[2];

        // int totalCurrentApGain = 0;
        // int timePassed = 0;

        // while(totalCurrentApGain < apGain && this.Player.currentActionPoints <= this.maxAp){
        //     this.Player.currentActionPoints += pointsPerTick;
        //     totalCurrentApGain += pointsPerTick;
        //     await Task.Delay(tickRate);
        //     timePassed += tickRate;
        // }

        // // if the ap gain didnt exactly hit the expected ap gain adjust accordingly
        // if(totalCurrentApGain != apGain) this.Player.currentActionPoints -= apGain - totalCurrentApGain;
        // await WaitRemainingTime(totalTime, timePassed);
        Debug.Log("IncreaseApWithDepletionFactors() shelved.");
        await Task.Yield();
    }
    private async Task WaitRemainingTime(int totalTime, int timePassed){
        int timeWait = (int)Mathf.Clamp(totalTime - timePassed, 0.0f, float.MaxValue);

        await Task.Delay(timeWait);
    }
    private void ClearEnemyList(){
        // foreach (Unit U in this.EnemyList){
        //     GameObject GO = U.gameObject;
        //     Destroy(GO);
        // }

        this.EnemyList = new List<Enemy>();
    }
#endregion

#region Input Functions during Player Turn 
    public virtual void CastAttack(int attackIndex){
        if(AttackIsComboAbility(attackIndex)) {
            Debug.LogError("ComboAbility landed in BattleSystem.CastAttack()");
            return;
        }
        if(!IndexInBoundsOfList(attackIndex, this.Player.Abilities.Count)){
            Debug.LogError("Attack index is not in Bounds of Player.Abilities");
            return;
        }

        Action A = this.Player.Abilities[attackIndex].Copy();
        CastAbility(A);
    }
    private void CastComboAbility(Action CastAction){
        // if(PlayerHasSufficientAp(this.Player.spentActionPoints + CastAction.combinationCost)){
        //     RemoveActionsFromComboList(CastAction.comboLevel);

        //     CastAction.abilityIndex = GetAbilityIndexByString(CastAction.name);

        //     ActionQueue.AddAction(CastAction);

        //     this.Player.spentActionPoints += CastAction.apCost;
        //     if(disableApCost) this.Player.spentActionPoints -= CastAction.apCost;
        // }

        // BattleMenuHandler.LoadMainMenu();
        Debug.Log("CastComboAbility() is shelved apparently.");
    }
    public virtual void CancelLastAction(bool gainApBack = true){

        if(ActionQueue.Actions.Count > 0) {

            // Bug: eine ult form ability wird gecancelt
            CheckUltFormAbilityCanceled();

            Action A = ActionQueue.RemoveLastAction();                 // defined in ActionQueue.cs
            // if(gainApBack) this.Player.spentActionPoints -= A.apCost;
        }
    }
    public virtual async void PassRound(){
        await ActionQueue.ExecuteAllActions();
        if(!ActionQueue.stopQueue) { // Player still lives?
            await PlayerTurn();
        }
        else ActionQueue.stopQueue = false; // Player died?
    }
    private void CastAbility(Action A){
        // if(CheckAddActionToComboListOnly(A)) return;

        // if(PlayerHasSufficientAp(Player.spentActionPoints + A.apCost)){
            ActionQueue.AddAction(A);

            // this.Player.spentActionPoints += A.apCost;
            // if(disableApCost) this.Player.spentActionPoints -= A.apCost;
        // }

        BattleMenuHandler.LoadMainMenu();
    }
    private bool AttackIsComboAbility(int abilityIndex){

        return this.Player.Abilities[abilityIndex].comboLevel > 1;
    }
    private bool PlayerHasSufficientAp(int apCost){

        // return this.Player.currentActionPoints >= apCost;
        Debug.Log("PlayerHasSufficientAp() is shelved.");
        return false;
    }
    private void RemoveActionsFromComboList(int comboLevel){
        // int countRemoveActions = (int)Mathf.Clamp(comboLevel - ActionQueue.comboListOverFlow,1.0f, (float)comboLevel);

        // // can use CancelLastAction() with gainApBack = true, because apCost of combo abilities contains ap Costs of recipe Actions
        // for(int i = 0; i < countRemoveActions; i++) CancelLastAction();

        // Debug.Log("Removed: "+countRemoveActions.ToString());

        
        // ActionQueue.ClearComboList();
        Debug.Log("RemoveActionsFromComboList() is shelved.");
    }
    private void CheckUltFormAbilityCanceled(){
        Action ActionToBeCanceled = ActionQueue.Actions[ActionQueue.Actions.Count-1];
        if(ActionToBeCanceled.comboLevel > ActionQueue.comboLevel) ActionQueue.comboLevel = ActionToBeCanceled.comboLevel;
    }
    private bool CheckAddActionToComboListOnly(Action A){
        // if the action queue is full but the combo queue isnt, abilities still can get added but only to the combo queue
        // if(ActionQueue.MaxActionsQueued()){
        //     return true;
        // }else if(ActionQueue.ActionQueueFull()){
        //     ActionQueue.AddAction(A);
        //     return true;
        // }else return false;
        Debug.Log("CheckAddActionToComboListOnly() is shelved.");
        return false;
    }
#endregion

#region Enemy Dies Functions
    public void EnemyDies(Enemy E){
        UpdateKillCount();
        UpdateEnemyKillRewards(E);
        CheckEnemyItemDrop(E);
        EnemySpawner.DeSpawnEnemy(E);
    }
    private void UpdateKillCount(){
        this.enemyDefeatCount++;
        this.KillCountText.text = this.enemyDefeatCount.ToString();
    }
    private void UpdateEnemyKillRewards(Enemy E){
        this.earnedCredits += E.killPrice;
        this.earnedExp += E.killExp;
        // ResultHandler.UpdateScore(this.earnedCredits);
    }
    private void CheckEnemyItemDrop(Enemy E){
        if(CheckForHit(E.itemDropChance)) {
            Debug.Log("Item dropped here");

            Item R = new Item();
            R.type = ItemType.repair;
            R.value = 2000;
            R.useCost = 250;

            this.Player.Items.Add(R);

            this.Player.CountAllItems();
        }
    }

    public void WaveOver(){
        Debug.Log("WaveOver() called!");
        // this.state = BattleState.WAVEOVER;
        // this.wavesClearedCount++;

        // ResetPlayerActionPoints();
        // SetBackActionQueueComboLevel();
        // InitiateAbilityDecayArray();

        // PauseMenu.ShowPauseMenu(true);
    }
    private void ResetPlayerActionPoints(){
        // this.Player.spentActionPoints = 0;
        // this.Player.currentActionPoints = 0;
        Debug.Log("ResetPlayerActionPoints() is shelved.");
    }
    private void SetBackActionQueueComboLevel(){
        // if(ActionQueue.comboLevel >= 2) ActionQueue.SetComboLevel(2);
        // else ActionQueue.SetComboLevel(1);
        Debug.Log("Changed SetBackActionQueueComboLevel() to use ResetHeatLevel().");
        ActionQueue.ResetHeatLevel();
    }
#endregion

    public void PlayerDies(){
        this.state = BattleState.PLAYERDIED;
        ActionQueue.stopQueue = true;
        ActionQueue.ClearActionQueue(); // not really necessary
        PauseMenu.ShowPauseMenu(true);
    }

    public void End(){ 
        DungeonRun NewRun = new DungeonRun();
        // TODO used items

        NewRun.creditsEarned = this.earnedCredits;
        NewRun.time = 0;                                    // TODO this.ResultHandler.time;
        NewRun.wavesCleared = this.wavesClearedCount;
        NewRun.movesUsed = this.movesUsedCount;
        NewRun.enemiesDefeated = this.enemyDefeatCount;
        NewRun.expEarned = this.earnedExp;

        this.GameHandler.SetNewRun(NewRun);
        SceneManager.LoadScene("Main Menu Scene");

        // TEMP
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public async Task EnemyAttack(Enemy AttackingEnemy){

        if(!CheckForHit(AttackingEnemy.accuracy)){
            this.Player.MissedAttack();
            return;
        }

        int damage = CalcUnitDamage(AttackingEnemy);

        if(CheckForHit(AttackingEnemy.crit)){
            damage = (int)Mathf.Round(damage*this.GameHandler.critMultiplier);
            this.Player.Crit();
        }

        await this.Player.DealDamage(damage);
    }

    public void RepairWeapon(){
        Item R = this.Player.Items.Find(item => item.type == ItemType.repair);
        this.Player.Weapon.Repair(R.value);
        this.Player.Items.Remove(R);
        this.itemsUsedCount++;
        this.totalItemCost += R.useCost;
        this.Player.repairItemCount--;
    }

#region Battle Actions Effect Functions
    public async Task<int> DealAOE(Action AOEAction, int unitCount){
        Debug.Log("DealAOE() changes not implemented yet.");
        await Task.Yield();
        return 0;
        // Debug.Log("AOE("+unitCount.ToString()+") called");
        // int damageDealt = 0;
        // // saving original data for later
        // int origDamage = AOEAction.baseDamage;
        // bool oMH = AOEAction.multiHitON;

        // // loop over unitCount units or all remaining units in the current wave, depending on which one is less
        // for(int currentUnitIndex = (int)Mathf.Min((float)unitCount, (float)(this.monsterWaveSize - this.enemyWaveIndex)); currentUnitIndex > 0; currentUnitIndex-- ){
        //     // getting the Enemy
        //     Enemy Ec = this.EnemyList[this.enemyWaveIndex + currentUnitIndex-1];

        //     // setting new data to the Action
        //     AOEAction.Enemy = Ec;
        //     AOEAction.enemySet = true;
        //     AOEAction.damage = origDamage;
        //     AOEAction.multiHitON = oMH;

        //     // executing the new Action, a lot of steps get skipped after the first NormalExecute() -> see Action.NormalExecute()
        //     damageDealt += await AOEAction.NormalExecute();
        // }
        // return damageDealt;
    }
    public async Task<int> MultiHit(Action MultiHitAction, int hitCount){ 
        Debug.Log("MultiHit() changes not implemented yet.");
        await Task.Yield();
        return 0;
        // Debug.Log("MultiHit("+hitCount.ToString()+") called");

        // int damageDealt = 0;
        // // saving original damage for later
        // int oDamage = MultiHitAction.damage;

        // // deal all hits as long as the targeted enemy lives
        // for(int currentHitCount = 0; currentHitCount < hitCount && MultiHitAction.Enemy.healthPoints > 0; currentHitCount++){
        //     // set damage and check MultiHitAction.enemySet for the case that the multihit doesnt target the main enemy (AOE+multihit)
        //     MultiHitAction.damage = oDamage;
        //     MultiHitAction.enemySet = true;
        //     damageDealt += await MultiHitAction.NormalExecute();
        // }
        // return damageDealt;
    }
    public async Task<int> DealOverFlow(int overFlowDamage){ 
        Debug.Log("DealOverFlow() changes not implemented yet.");
        await Task.Yield();
        return 0;
        // int totalDamageDealt = 0;

        // // while there is still damage left to deal and still enemies left to deal it to
        // while(overFlowDamage > 0 && EnemiesLeftInWave()){
            
        //     int damageDealt = await this.Enemy.DealDamage(overFlowDamage);

        //     totalDamageDealt += damageDealt;
        //     overFlowDamage -= damageDealt;
        // }
        // return totalDamageDealt;
    }
    private bool EnemiesLeftInWave(){
        // after the last Enemy in the wave is killed, the state is instantly changed to WAVEOVER
        // since the state is in QUEUE when DealOverFlow() is called (because it can only be called from an Action.NormalExecute)
        // when the state is not QUEUE anymore that means that the last enemy has died and there are no more enemies left in the wave
        return this.state == BattleState.QUEUE;
    }
#endregion

#region Ability Helper Functions
    public bool CheckForHit(int accuracy){                 /// !!!!!!!!! Also used for checking crits! (same functionality)
        int ran = Random.Range(1, 101);
        return ran <= accuracy;
    }
    public int CalcUnitDamage(Unit U){ 

        return Random.Range(U.attackMin, U.attackMax+1); 
    }
    public int GetAbilityIndexByString(string givenActionName){
        Action ActionFromPlayerAbilityList = this.Player.Abilities.Find(action => action.name == givenActionName);

        if(ActionFromPlayerAbilityList != null) return ActionFromPlayerAbilityList.abilityIndex;
        else {
            Debug.LogError("Ability Name not found in Player Abilities");
            return -1;
        }
    }
    public virtual int AbilityDecay(Action ActionIn){
        if(this.disableAbilityDecay) return ActionIn.damage;

        int damageAfterAbilityDecay = GetDamageAfterAbilityDecay(ActionIn);
        UpdateAbilityDecayArray(ActionIn);

        return damageAfterAbilityDecay;
    }
    private int GetDamageAfterAbilityDecay(Action cAction){

        float decayMultiplicatorIncreasePerLevel = (this.abilityDecayMultiplierMax - this.abilityDecayMultiplierMin)/this.abilityDecayLevelCount;

        int cActionCurrentAbilityDecayLevel = this.AbilityDecayArray[cAction.abilityIndex];

        float decayActionMultiplicator = this.abilityDecayMultiplierMin+(decayMultiplicatorIncreasePerLevel*cActionCurrentAbilityDecayLevel);

        return (int)Mathf.Round(cAction.damage*decayActionMultiplicator);
    }
    private void UpdateAbilityDecayArray(Action ActionIn){
        int[] decayVals = GetComboDecayValues();

        int decayLossForThisAction = decayVals[ActionIn.comboLevel-1];
        this.AbilityDecayArray[ActionIn.abilityIndex] = (int)Mathf.Clamp(this.AbilityDecayArray[ActionIn.abilityIndex] - decayLossForThisAction, 0.0f, float.MaxValue);

        // all actions with a lower combolevel then ActionIn get +1 onto their ability decay level
        foreach(Action A in Player.Abilities){
            if(A.abilityIndex != ActionIn.abilityIndex && A.comboLevel <= ActionIn.comboLevel) {
                AbilityDecayArray[A.abilityIndex] = (int)Mathf.Clamp(AbilityDecayArray[A.abilityIndex]+1, 0.0f, this.abilityDecayLevelCount);
            }
        }
    }
    private int[] GetComboDecayValues(){

        // counting how many abilities are of combolevel 1 - 5
        int[] cAttacks = {0,0,0,0,0}; 
        foreach(Action A in Player.Abilities) cAttacks[A.comboLevel-1]++;

        // getting decay loss for actions of every comboLevel based on how many actions of that combo level are available to the player
        int[] res = {0,0,0,0,0};
        for(int i=0;i<res.Length;i++){
            res[i] = cAttacks[i];
            // if actions of higher comboLevel cant be executed with current ActionQueue ComboLevel: -1 on the value
            if(i+1 >= ActionQueue.comboLevel) res[i]--;
        } 

        return res;
    }
#endregion

#region Spell Elements Functions
    private SpellElement CombineElements(List<SpellElement> BaseElements){      // TODO
        // the ability setup functions call this with a list of none elements, which do not need to be combined
        if(BaseElements.Contains(SpellElement.none)) return SpellElement.none;

        List<SpellElement> MaxAmountElementsList = GetMaxAmountElements(BaseElements);
        //for testing
        //this.log2 = MaxAmountElementsList;

        if(MaxAmountElementsList.Count == 1) return MaxAmountElementsList[0];

        SpellElement CombinedElement = GetHighestLevelMaxAmountElement(MaxAmountElementsList);

        return CombinedElement;
    }
    public int ElementWeakTo(SpellElement AttackElement, SpellElement DefendElement){
        if(AttackElement == SpellElement.none) return 0;
        if(DefendElement == SpellElement.none) return -1;

        int attackEleIndex = (int)AttackElement;
        int defendEleIndex = (int)DefendElement;

        if(defendEleIndex > 3) Debug.LogError("Defending element can only be of the 4 base elements");
        if(attackEleIndex < 0 || defendEleIndex < 0) Debug.LogError("Something went wrong!");

        return this.ElementWeaknessMatrix[attackEleIndex, defendEleIndex];
    }
    private List<SpellElement> GetMaxAmountElements(List<SpellElement> BaseElements){
        // returns a list of all max amount elements, meaning all elements where there are the most of

        // counting all elements in baseElements
        int elementTypesCount = SpellElement.GetNames(typeof(SpellElement)).Length;
        // -1 for SpellElement.none
        elementTypesCount--;

        int[] elementCounts = new int[elementTypesCount];
        foreach(SpellElement Ele in BaseElements) elementCounts[(int)Ele]++;
        // for testing
        this.log1 = elementCounts;

        // add all elements of the maximum amount to the MaxAmountElementsList and return;

        List<SpellElement> MaxAmountElementsList = new List<SpellElement>();

        int maxAmountOfElements = elementCounts.Max();

        foreach(SpellElement element in BaseElements){
            if(elementCounts[(int)element] == maxAmountOfElements && !MaxAmountElementsList.Contains(element)){
                MaxAmountElementsList.Add(element);
            }
        }

        /*for(int elementCountsIndex = 0; elementCountsIndex < elementCounts.Length; elementCountsIndex++){       // <-- use BaseElements[i] instead?
            if(elementCounts[elementCountsIndex] == maxAmountOfElements){
                int maxAmountElementEnumIndex = elementCountsIndex;
                SpellElement maxAmountElement = (SpellElement)maxAmountElementEnumIndex;
                MaxAmountElementsList.Add(maxAmountElement);
            }
        }*/

        return MaxAmountElementsList;
    }
    private SpellElement GetHighestLevelMaxAmountElement(List<SpellElement> MaxAmountElementsList){
        // next priority: find out how many elements are of which level, which of these levels have the most ELements, and which of these highest amount element levels is the highest level
        // return the first element in the list that has that level (Exmpl. FIRE, ARCANUM, ICE, PLODE, MENUM -> ARCANUM) 

        // find out how many elements of which elementlevel are in the array
        int[] elementLevelCounts = {0,0,0,0};
        foreach(SpellElement Element in MaxAmountElementsList) elementLevelCounts[GetElementLevel(Element)]++;

        // find the the highest elementlevel of the max amount of elements of one level

        int maxAmount = 0;
        int highestLevelMaxAmountElementLevel = 0;

        for(int countsIndex = 0; countsIndex < elementLevelCounts.Length; countsIndex++){
            if(elementLevelCounts[countsIndex] > maxAmount) {
                maxAmount = elementLevelCounts[countsIndex];
                highestLevelMaxAmountElementLevel = countsIndex;
            }
            else if(elementLevelCounts[countsIndex] == maxAmount && countsIndex > highestLevelMaxAmountElementLevel){
                highestLevelMaxAmountElementLevel = countsIndex;
            }
        }

        // find the first Element that has that element level and return
        foreach(SpellElement Element in MaxAmountElementsList){
            if(GetElementLevel(Element) == highestLevelMaxAmountElementLevel) return Element;
        }
        return SpellElement.none;
    }
    private int GetElementLevel(SpellElement Element){
        int level = 0;

        switch(Element){
            case SpellElement.FIRE:
            case SpellElement.WATER:
            case SpellElement.AIR:
            case SpellElement.EARTH:
                level = 1;
            break;
            
            case SpellElement.REZA:
            case SpellElement.ICE:
            case SpellElement.PLODE:
            case SpellElement.ROTA:
                level = 2;
            break;

            case SpellElement.ARCANUM:
            case SpellElement.MENUM:
            case SpellElement.GEOS:
            case SpellElement.CHAOS:
                level = 3;
            break;
        }

        return level;
    }
#endregion
    
    public bool IndexInBoundsOfList(int index, int listSize){

        return index < listSize && index >= 0;
    }
    
    public IEnumerator CrawlText(string txt, Text textField, float maxCrawlTime){
        // this function is one of the first I ve ever written and it has worked since. So I'm not even going to touch that right now(28.09.22)
        stopTextCrawl = false;

        float timePerSign = maxCrawlTime/txt.Length;
        timePerSign = Mathf.Round(timePerSign*1000);
        timePerSign /= 1000;

        textField.text = "";

        for(int i = 0; i < txt.Length; i++){
            if(!stopTextCrawl){
                textField.text += txt[i];
                yield return new WaitForSeconds(timePerSign);
            }
        }

        stopTextCrawl = false;

        yield return null;
    }

#region shelved functionality
    /*private void AddWeaponMoveListToPlayerAbilities(){
        List<Action> MoveList = this.Player.Weapon.GetCompleteMoveList();

        int currentAbilityIndex = 0;

        foreach(Action A in MoveList) {
            if(A.comboLevel > 1) {
                ActionQueue.AvailableComboActionsAsStrings.Add(A.comboList);
            }

            A.abilityIndex = currentAbilityIndex;
            currentAbilityIndex++;
            this.Player.Abilities.Add(A);
        }
    }*/
    /*private void SetNextEnemy() {
        if(this.enemyWaveIndex < this.EnemyList.Count -1) this.NextEnemy = this.EnemyList[this.enemyWaveIndex+1];
        else this.NextEnemy = null;
    }*/
    /*private void IncreaseActionScore(int amount){
        actionScore += amount;
        if(actionScore > maxActionScore) actionScore = maxActionScore;
        else if(actionScore < 0 ) {
            actionScore = 0;
            int stage = (int)(actionScore/5);
            ResultHandler.UpdateMultiplier();
        }
        else {
            int stage = (int)(actionScore/5);
            ResultHandler.UpdateMultiplier();
        }
    }*/
    /*private void AddSpellDecay(int AbilityIndex){
        
        currentSpellDecay++;
        if(currentSpellDecay > 2) currentSpellDecay = 2;

        lastAbilityDecayIndex = AbilityIndex;
        //Debug.Log("Ability Decay: "+ Player.Abilities[AbilityIndex].spellName+"|"+currentSpellDecay);
    }*/
    /*private void ResetSpellDecay(){
        AbilityDecayReminder[0] = new Stack<int>();
        AbilityDecayReminder[1] = new Stack<int>();  

        lastAbilityDecayIndex = -1;
        currentSpellDecay = 0;
    }*/
    /*private bool UseHealItem(){
        if(Player.healItemCount > 0){
            Item I = Player.Items.Find(I => I.type == ItemType.heal);
            if(I == null) Debug.Log("An heal item went out of count!");
            Player.healthPoints += I.value;
            // Should be fixed in BattleHUD.DepleteHp() -- if(Player.healthPoints > Player.maxHealthPoints) Player.healthPoints = Player.maxHealthPoints;          // if healed to full you will still lose hp due to the hp depletion being slower than this.
            Player.Items.Remove(I);
            itemsUsedCount++;
            totalItemCost += I.useCost;
            Player.healItemCount--;

            Action newAction = new Action();
            newAction.id = "InfoText";
            //newAction.textColor = itemColor;
            ActionQueue.IntersectNewAction(newAction);
            return true;
        }
        else return false;
    }*/
    /*private void QueueAttack(Action A){
        int baseDamage = CalcUnitDamage(Player);

        //int damageSpamDecay = (int)Mathf.Round((baseDamage*0.2f)*currentSpellDecay);

        int damage = baseDamage; // - damageSpamDecay;

        A.id = "BasicAttack";
        A.cover = "ATTACK";
        A.ap = Player.attackCost;
        //CurrentAction.textColor = playerAttackColor;

        ActionQueue.AddAction(A);                   // defined in ActionQueue.cs
    }
    private async Task ExecuteAttack(Action Attack){
        Player.spentActionPoints -= Player.attackCost; 

        movesUsedCount++;

        if(!CheckForHit(Player.accuracy)){
            await Enemy.MissedAttack();
            Player.currentActionPoints -= Player.attackCost;
            return;
        }

        int damage = CalcUnitDamage(Player);

        if(CheckForHit(Player.crit)){
            damage = (int)Mathf.Round(damage*critMultiplier);
            Enemy.Crit();
            //newAction.textColor = playerCritColor;
        }

        Player.Weapon.DamageToDurability(Enemy.damageToWeapons);
        damage = CheckAbilityDecay(damage, Attack.abilityIndex);
        await Enemy.DealDamage(damage);
    }
    private void QueueSpell(Action A){
        A.id = "CastSpell";
        A.cover = A.Ability.spellName.ToUpper();
        A.ap = A.Ability.apCost;
        A.mana = A.Ability.manaCost;
        A.damage = A.Ability.damage;
        A.accuracy = A.Ability.accuracy;
        A.crit = A.Ability.crit;
        //CurrentAction.textColor = playerAttackColor;
        A.element = A.Ability.element;

        ActionQueue.AddAction(A);
    }
    private async Task ExecuteSpell(Action Spell){
        Player.spentActionPoints -= Spell.ap; 

        movesUsedCount++;
        List<Task> tasksTmp;

        if(Spell.mana > Player.mana){
            tasksTmp = new List<Task>();
            tasksTmp.Add(Enemy.MissedAttack());
            tasksTmp.Add(Player.DepleteMana(Player.mana));
            await Task.WhenAll(tasksTmp);
            Player.currentActionPoints -= Spell.ap;
            return;
        }

        if(!CheckForHit(Spell.accuracy)){
            //newAction.textColor = playerMissColor;
            await Enemy.MissedAttack();
            Player.currentActionPoints -= Spell.ap;
            return;
        }

        int weakness = ElementWeakTo(Enemy.element, Spell.element);
        // returns 1 if less damage
        // returns 0 if neutral
        // returns -1 if more damage

        if(weakness == 1) Spell.damage = (int)Mathf.Round(Spell.damage*spellWeakMultiplier);

        if(weakness == -1) Spell.damage = (int)Mathf.Round(Spell.damage*spellStrongMultiplier);

        if(CheckForHit(Spell.crit)){
            Spell.damage = (int)Mathf.Round(Spell.damage*critMultiplier);
            Enemy.Crit();
            //newAction.textColor = playerCritColor;
        }
        Spell.damage = CheckAbilityDecay(Spell.damage, Spell.abilityIndex);
        tasksTmp = new List<Task>();
        tasksTmp.Add(Player.DepleteMana(Spell.mana));
        tasksTmp.Add(Enemy.DealDamage(Spell.damage));
        await Task.WhenAll(tasksTmp);
    }
    public int AbilityDecay(int baseDmg, int abilityIndex){
        if(disableAbilityDecay) return baseDmg;

        float l = (abilityDecayMultiplierMax - abilityDecayMultiplierMin)/abilityDecayLevelCount;
        float mult = abilityDecayMultiplierMin+l*AbilityDecayArray[abilityIndex];

        int res = (int)Mathf.Round(baseDmg*mult);
        
        if(AbilityDecayArray[abilityIndex]-abilityDecayLevelLoss > 0){ // prev.: >= 0
            AbilityDecayArray[abilityIndex] -= abilityDecayLevelLoss;
        }
        for(int i=0;i<AbilityDecayArray.Length;i++){
            if(i!=abilityIndex) {
                AbilityDecayArray[i] += abilityDecayLevelGain;
                if(AbilityDecayArray[i] > abilityDecayLevelCount) AbilityDecayArray[i] = abilityDecayLevelCount;
            }
        }
        return res;
    }
    private List<int> GetLowestDecayIndexes(Action cA){
        List<int> tmp = new List<int>();
        List<int> res = new List<int>();
        for(int i=0;i<cA.comboLevel;i++) {
            tmp.Add(AbilityDecayArray[i]);
            res.Add(i);
        }
        for(int i=0;i<tmp.Count;i++){
            int compare1I = i;
            int compare2I = i+1;
            bool switched = false;
            while(compare2I<tmp.Count && tmp[compare1I] > tmp[compare2I]){
                int tmpI = tmp[compare2I];
                int tmpVal = res[compare2I];

                tmp[compare2I] = tmp[compare1I];
                res[compare2I] = res[compare1I];

                tmp[compare1I] = tmpI;
                tmp[compare1I] = tmpVal;

                compare1I++;
                compare2I++;
                switched = true;
            }
            if(switched) i = -1;
        }

        
        // for testing
        List<int> log3_l = new List<int>();
        foreach(int i in res) log3_l.Add(i);
        this.log3 = log3_l;


        foreach(Action A in Player.Abilities){
            if(A.comboLevel >= cA.comboLevel){
                int resIndex = 0;
                while(resIndex < res.Count && AbilityDecayArray[A.abilityIndex] >= AbilityDecayArray[res[resIndex]]) resIndex++;

                if(resIndex < cA.comboLevel) res[resIndex] = A.abilityIndex;
            }
        }

        return res;
    }*/
#endregion
} // EOF