/*
    Hauptstück des gesamten Battle ablaufs. ALLE prozesse die gestartet werden kommen entweder hier vorbei oder werden größtenteils auch von hier gestartet.
    Viele wichtige daten und referenzen werden hier gehalten und können per referenz auch von hier geholt werden.
*/

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    [SerializeField] private int monsterWaveSize;
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
    public GameObject PlayerPrefab;
    public GameObject[] EnemyPrefabs;
    [SerializeField] public PlayerCharacter Player;
    [SerializeField] public Enemy Enemy;
    [SerializeField] public Enemy NextEnemy;
    public List<Enemy> EnemyList = new List<Enemy>();
    // bsvg5

    public List<Action> TestAbilities = new List<Action>(); // bsvg6

    public int maxAp = 99; // bsvg7

    [Header("System Counters")]
    // bsvg8
    public int enemyDefeatCount = 0;
    public int enemyWaveIndex = 0;
    
    [SerializeField] public int earnedCredits = 0;
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

    [Header("Former BattleHUD")]
    // bsvg9
    [SerializeField] private int StandartApGainTextShowTime = 1000; // in ms

    [SerializeField] public Text PlayerNameText;
    [SerializeField] public Text PlayerApText;
    [SerializeField] public Text PlayerApGainText;
    [SerializeField] public Slider PlayerHpSlider;
    [SerializeField] public Slider PlayerShieldSlider;
    [SerializeField] public Slider PlayerManaSlider;

    [Header("")]
    [SerializeField] public Text EnemyNameText;
    [SerializeField] public Text EnemyLevelText;
    [SerializeField] public Slider EnemyHpSlider;
    [SerializeField] public Slider EnemyShieldSlider;
    // bsvg9


    public Text KillCountText; // bsvg10


    [Header("Other System References")]
    // bsvg11
    public Importer Importer;

    //public BattleHUD BattleHUD;
    public MenuHandler MenuHandler;
    public ActionQueue ActionQueue;
    public PauseMenu PauseMenu;
    public ResultHandler ResultHandler;
    public DialogueHandler DialogueHandler;
    public WaveScript WaveScript;

    public GameObject MainMenu;
    // bsvg11

    [Header("Battle Multipliers")]
    // bsvg12
    [SerializeField] public float critMultiplier = 1.5f;
    [SerializeField] public float heavyAttackMultiplier = 1.2f;
    [SerializeField] public float specialAttackMultiplier = 1.1f;
    [SerializeField] public float ultAbilityDamageMultiplier = 1.1f;


    [SerializeField] public float attackIntoShieldMultiplier = 0.9f;

    [SerializeField] public float spellStrongMultiplier = 1.5f;
    [SerializeField] public float spellWeakMultiplier = 0.75f;
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

    [Header("MSG Colors")]
    // bsvg14
    public Color gainColor;
    public Color playerAttackColor;
    public Color enemyAttackColor;
    public Color playerMissColor;
    public Color enemyMissColor;
    public Color playerCritColor;
    public Color enemyCritColor;
    public Color itemColor;
    public Color standartColor;
    // bsvg14

    [Header("Element Matrix")]
    private int[,] ElementWeaknessMatrix = new int[,] // bsvg15
                                            {
                                            { 0,  1,  0, -1},
                                            {-1,  0,  1,  0},
                                            { 0, -1,  0,  1},
                                            { 1,  0, -1,  0}
                                            };

    [Header("For Testing")]
    // bsvg16
    public bool disableAbilityDecay = false;

    public bool disableHeat = false;

    public bool disableTestCombo = false;

    [SerializeField] public bool disableApCost = false;

    [SerializeField] private int[] log1;  // decay values for lv1-5 combo abilities
    //[SerializeField] public List<Action> log2; // empty
    //[SerializeField] private List<int> log3; // empty
    // bsvg16

#region Unity Functions
    private void Awake(){
        this.state = BattleState.SETUP;
        SetUpEverything();
        PauseMenu.ShowPauseMenu(true);
        // the beginning pause menu unlocks the Enemy attack loop and calls PlayerTurn()
    }
    private void OnApplicationQuit(){
        
        ClearEnemyList();
    }
#endregion

    private void SetUpEverything(){

        ImportStartSettingsFromImporter();
        SetUpSystemComponents();
        if(this.useWaveScript) SetUpWaveScript();
        SetUpBattleParticipants();
        if(this.useDialogueScript) SetUpDialogueScript();
    }
    private void ImportStartSettingsFromImporter(){
        Importer = GameObject.Find("Importer").GetComponent<Importer>();

        this.entryCost = Importer.entryCost;
        this.monsterWaveSize = Importer.monsterWaveSize;
        this.moveCost = Importer.moveCost;
        this.deathCost = Importer.deathCost;

        this.PlayerPrefab = Importer.PlayerPrefab;
        this.EnemyPrefabs = Importer.EnemyPrefabs;
    }

#region SetUpSystemComponents Functions
    private void SetUpSystemComponents(){
        SetBattleSystemsComponentReferenzes();
        SetUpComponentsInnerReferenzes();
    }
    private void SetBattleSystemsComponentReferenzes(){
        SetMenuHandlerReferenz();
        SetActionQueueReferenz();
        SetPauseMenuReferenz();
        SetResultHandlerReferenz();
        SetDialogueHandlerReferenz();
    }
    private void SetUpComponentsInnerReferenzes(){
        SetUpMenuHandlerInnerReferenzes();
        SetUpActionQueueInnerReferenzes();
        SetUpPauseMenuInnerReferenzes();
        SetUpResultHandlerInnerReferenzes();
        SetUpDialogueHandlerInnerReferenzes();
    }
    private void SetMenuHandlerReferenz(){
        GameObject MenuHandlerGameObject = GetSystemComponentGameObject("MenuHandler");
        this.MenuHandler = MenuHandlerGameObject.GetComponent<MenuHandler>();
    }
    private void SetActionQueueReferenz(){
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
    private void SetUpMenuHandlerInnerReferenzes(){
        this.MenuHandler.BattleSystem = this;
        this.MenuHandler.ActionQueue = this.ActionQueue;
        this.MenuHandler.PauseMenu = this.PauseMenu;
        this.MenuHandler.DialogueHandler = this.DialogueHandler;  
    }
    private void SetUpActionQueueInnerReferenzes(){
        
        this.ActionQueue.BattleSystem = this; 
    }
    private void SetUpPauseMenuInnerReferenzes(){

        this.PauseMenu.BattleSystem = this;
    }
    private void SetUpResultHandlerInnerReferenzes(){

        this.ResultHandler.BattleSystem = this;
    }
    private void SetUpDialogueHandlerInnerReferenzes(){
        this.DialogueHandler.BattleSystem = this;
        this.DialogueHandler.ActionQueue = this.ActionQueue; 
    }
    private GameObject GetSystemComponentGameObject(string componentName){
        
        return GameObject.Find(componentName);
    }
#endregion

    private void SetUpWaveScript(){
        // spawn wavescript
        this.WaveScript = Instantiate(Importer.WaveScriptPrefab).GetComponent<WaveScript>();
        this.WaveScript.waveScriptName = Importer.waveScriptName;
        this.WaveScript.EnemyPrefabs = this.EnemyPrefabs;
        this.WaveScript.Activate();
    }

    private void SetUpBattleParticipants(){
        AddEnemiesToEnemyList();
        SetCurrentUnits();
    }

#region AddEnemiesToEnemyList Functions
    private void AddEnemiesToEnemyList(){
        if(this.useWaveScript && !CurrentWaveIsLastWaveInScript()){
            SetUpEnemyListWithWaveScript();
        }
        else{
            SetUpEnemyListWithRandomWave();
        }
    }
    private bool CurrentWaveIsLastWaveInScript(){
        
        return this.wavesClearedCount+1 <= WaveScript.waveCount;
    }
    private void SetUpEnemyListWithWaveScript(){
        this.EnemyList = WaveScript.NextWave();
        foreach (Enemy E in this.EnemyList){
            E.Setup(this);
        } 
    }
    private void SetUpEnemyListWithRandomWave(){
        List<GameObject> RandomizedEnemiesToBeSpawned = GetRandomizedEnemyPrefabList();

        foreach (GameObject EnemyGameObject in RandomizedEnemiesToBeSpawned){
            GameObject InstantiatedEnemyGameObject = Instantiate(EnemyGameObject);
            InstantiatedEnemyGameObject.GetComponent<Enemy>().Setup(this);
            this.EnemyList.Add(InstantiatedEnemyGameObject.GetComponent<Enemy>());
        }
    }
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

        for(int enemyIndex = 0; enemyIndex < this.EnemyPrefabs.Length; enemyIndex++){
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
    private void SetCurrentUnits(){
        SetUpCurrentPlayer();
        SetUpCurrentEnemy();
    }
    private void SetUpCurrentPlayer(){
        // Marcel.object 
        // Marcel.Player.weapon = marcel.weapon
        // -||- armor
        /*
        switch(marcel.playername){
            case "Hoover":
                Player = new Hoover();

            case "Whatever":
            .....
        }
        */
        this.PlayerApGainText.text = "";
        GameObject P = Instantiate(PlayerPrefab);
        this.Player = P.GetComponent<PlayerCharacter>();
        this.Player.Setup(this);

        SetUpPlayerAbilities();
        SetUpPlayerItems();
    }
    private void SetUpCurrentEnemy(){
        this.enemyWaveIndex = 0;
        this.Enemy = this.EnemyList[0];
        this.Enemy.Hide(false);
        this.Enemy.StartAttack();
    }
    private void SetUpPlayerAbilities(){
        this.TestAbilities = this.Player.Weapon.GetMovesToTest();

        // NOTE: Element fähigkeit IMMER als erstes, weil der MenuHandler darauf aufbaut

        this.Player.Elements.Add(SpellElement.FIRE);        // <-- not completely implemented yet/Also: Scythe???
        //this.TestAbilities.Add(A);

        if(!this.disableTestCombo){
            SetUpTestCombo();
            Debug.Log(this.TestAbilities.Count.ToString()+" abilities in TestCombo.");
        }

        AddWeaponMoveListToPlayerAbilities();

        ActionQueue.firstComboAbilityIndex = GetFirstComboAbilityIndex();

        Debug.Log(this.Player.Abilities.Count.ToString()+" player abilities");
        InitiateAbilityDecayArray();
    }                                               // TODO
    private void SetUpPlayerItems(){
        /*Item I = new Item();
        I.type = ItemType.heal;
        I.value = 300;
        I.useCost = 460;

        Player.Items.Add(I);*/

        Item R = new Item();
        R.type = ItemType.repair;
        R.value = 2000;
        R.useCost = 250;

        this.Player.Items.Add(R);

        this.Player.CountAllItems();
    }
    private void SetUpTestCombo(){
        Action A = new TestCombo();
        A.abilityIndex = 1;
        this.Player.Abilities[1] = A;
    }
    private void AddWeaponMoveListToPlayerAbilities(){
        List<Action> MoveList = this.Player.Weapon.GetMoveList();

        int currentAbilityIndex = 0;

        foreach(Action A in MoveList) {
            if(A.comboLevel > 1) {
                ActionQueue.AvailableComboActionsAsStrings.Add(A.comboList);
            }

            A.abilityIndex = currentAbilityIndex;
            currentAbilityIndex++;
            this.Player.Abilities.Add(A);
        }
    }
    private int GetFirstComboAbilityIndex(){
        int resultIndex = 0;
        foreach(Action A in this.Player.Abilities){
            if(A.comboLevel < 2) resultIndex++;
            else break;
        }
        return resultIndex;
    }
    private void InitiateAbilityDecayArray(){
        this.AbilityDecayArray = new int[Player.Abilities.Count];
        foreach(Action A in this.Player.Abilities){
            this.AbilityDecayArray[A.abilityIndex] = this.abilityDecayLevelCount;
        }
    }
#endregion

    private void SetUpDialogueScript(){
        DialogueHandler.dialogueScriptName = Importer.dialogueScriptName;
        DialogueHandler.ActivateDialogueScript();
    }

#region Round Start Functions
    public async void NextWave(){
        this.state = BattleState.START;
        ClearEnemyList();
        AddEnemiesToEnemyList();
        SetUpCurrentEnemy();
        this.Player.actionPointGain += this.Player.startActionPoints;

        await PlayerTurn();
        this.Enemy.StartAttack();
    }
    // PauseMenu.ContinueGame() calls PlayerTurn() at end of BattleState.SETUP(see above)
    public async Task PlayerTurn(){
        this.state = BattleState.START;
        await GainAp(this.Player.actionPointGain + this.Player.roundStartActionPointGain);
        this.Player.ResetActionPointGain();
        this.state = BattleState.PLAYERTURN;
    }
    public async Task GainAp(int apGain, int totalTime = -1){
        if(totalTime < 0) totalTime = this.StandartApGainTextShowTime;

        if(ApGainWouldOverFlowMaxAp(apGain)) apGain = this.maxAp - this.Player.currentActionPoints;
        
        this.PlayerApGainText.text = "+"+apGain.ToString();

        int restTime = await WaitBeforeIncrease(totalTime);

        int[] depletionFactors = CalculateApGainDepletionFactors(apGain, restTime);
        
        showApGainText(apGain, depletionFactors);

        await IncreaseApWithDepletionFactors(apGain, depletionFactors);
    }
    private bool ApGainWouldOverFlowMaxAp(int apGain){

        return this.Player.currentActionPoints+apGain > this.maxAp;
    }
    private async void showApGainText(int apGain, int[] depletionFactors){

        int tickRate = depletionFactors[0]; // see CalculateApGainDepletionFactors()
        int pointsPerTick = depletionFactors[1];
        int totalTime = depletionFactors[2];

        int totalApGain = apGain;
        int timePassed = 0;

        while(totalApGain >= 0 && this.Player.currentActionPoints <= maxAp){
            await Task.Delay(tickRate);
            totalApGain -= pointsPerTick;
            this.PlayerApGainText.text = "+"+totalApGain.ToString();
            timePassed += tickRate;
        }

        if(totalApGain != 0) this.PlayerApGainText.text = "+0";

        await WaitRemainingTime(totalTime, timePassed);

        this.PlayerApGainText.text = "";
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
        int tickRate = depletionFactors[0];             // see CalculateApGainDepletionFactors()
        int pointsPerTick = depletionFactors[1];
        int totalTime = depletionFactors[2];

        int totalCurrentApGain = 0;
        int timePassed = 0;

        while(totalCurrentApGain < apGain && this.Player.currentActionPoints <= this.maxAp){
            this.Player.currentActionPoints += pointsPerTick;
            totalCurrentApGain += pointsPerTick;
            await Task.Delay(tickRate);
            timePassed += tickRate;
        }

        // if the ap gain didnt exactly hit the expected ap gain adjust accordingly
        if(totalCurrentApGain != apGain) this.Player.currentActionPoints -= apGain - totalCurrentApGain;
        await WaitRemainingTime(totalTime, timePassed);
    }
    private async Task WaitRemainingTime(int totalTime, int timePassed){
        int timeWait = (int)Mathf.Clamp(totalTime - timePassed, 0.0f, float.MaxValue);

        await Task.Delay(timeWait);
    }
    private void ClearEnemyList(){
        foreach (Unit U in this.EnemyList){
            GameObject GO = U.gameObject;
            Destroy(GO);
        }

        this.EnemyList = new List<Enemy>();
    }
#endregion

#region Input Functions during Player Turn 
    // all public functions in this region are called from MenuHandler.CheckInputs()
    public void CastAttack(int attackIndex){
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
    public void CastSpell(int eleIndex){
        if(!IndexInBoundsOfList(eleIndex, this.Player.Elements.Count)){
            Debug.LogError("Spell index is not in Bounds of Player.Elements");
            return;
        }

        Action A = new Element(Player.Elements[eleIndex]);
        // see SetUpPlayerAbilities - Action.Element is always at Player.Abilities[0]
        A.abilityIndex = 0; 
        CastAbility(A); 
    }
    public void CastComboAbility(Action CastAction){
        if(PlayerHasSufficientAp(this.Player.spentActionPoints + CastAction.combinationCost)){
            RemoveActionsFromComboList(CastAction.comboLevel);

            CastAction.abilityIndex = GetAbilityIndexByString(CastAction.name);

            ActionQueue.AddAction(CastAction);

            this.Player.spentActionPoints += CastAction.apCost;
            if(disableApCost) this.Player.spentActionPoints -= CastAction.apCost;
        }

        MenuHandler.LoadMainMenu();
    }
    public void CancelLastAction(bool gainApBack = true){

        if(ActionQueue.Actions.Count > 0) {

            // Bug: eine ult form ability wird gecancelt
            CheckUltFormAbilityCanceled();

            Action A = ActionQueue.RemoveLastAction();                 // defined in ActionQueue.cs
            if(gainApBack) this.Player.spentActionPoints -= A.apCost;
        }
    }
    public async void PassRound(){
        await ActionQueue.ExecuteAllActions();
        if(!ActionQueue.stopQueue) {
            await PlayerTurn();
        }
        else ActionQueue.stopQueue = false;
    }
    private void CastAbility(Action A){
        if(CheckAddActionToComboListOnly(A)) return;

        if(PlayerHasSufficientAp(Player.spentActionPoints + A.apCost)){
            ActionQueue.AddAction(A);

            this.Player.spentActionPoints += A.apCost;
            if(disableApCost) this.Player.spentActionPoints -= A.apCost;
        }

        MenuHandler.LoadMainMenu();
    }
    private bool AttackIsComboAbility(int abilityIndex){

        return this.Player.Abilities[abilityIndex].comboLevel > 1;
    }
    private bool PlayerHasSufficientAp(int apCost){

        return this.Player.currentActionPoints >= apCost;
    }
    private void RemoveActionsFromComboList(int comboLevel){
        int countRemoveActions = (int)Mathf.Clamp(comboLevel - ActionQueue.comboListOverFlow,1.0f, (float)comboLevel);

        // can use CancelLastAction() with gainApBack = true, because apCost of combo abilities contains ap Costs of recipe Actions
        for(int i = 0; i < countRemoveActions; i++) CancelLastAction();

        Debug.Log("Removed: "+countRemoveActions.ToString());

        
        ActionQueue.ClearComboList();
    }
    private void CheckUltFormAbilityCanceled(){
        Action ActionToBeCanceled = ActionQueue.Actions[ActionQueue.Actions.Count-1];
        if(ActionToBeCanceled.comboLevel > ActionQueue.comboLevel) ActionQueue.comboLevel = ActionToBeCanceled.comboLevel;
    }
    private bool CheckAddActionToComboListOnly(Action A){
        // if the action queue is full but the combo queue isnt, abilities still can get added but only to the combo queue
        if(ActionQueue.MaxActionsQueued()){
            return true;
        }else if(ActionQueue.ActionQueueFull()){
            ActionQueue.AddAction(A);
            return true;
        }else return false;
    }
#endregion

#region Enemy Dies Functions
    public void EnemyDies(){
        UpdateEnemyKillRewards();
        this.enemyDefeatCount++;
        CheckEnemyItemDrop();
        SetNewEnemy();
    }
    private void UpdateEnemyKillRewards(){
        this.earnedCredits += this.Enemy.killPrice;
        this.earnedExp += this.Enemy.killExp;
        ResultHandler.UpdateScore(this.earnedCredits);

        //int heal = (int)Mathf.Round((Player.maxHealthPoints-Player.healthPoints)*0.7f);
        //BattleHUD.UpdateHp(-heal, Player);
    }
    private void CheckEnemyItemDrop(){
        if(CheckForHit(this.Enemy.itemDropChance)) {
            Debug.Log("Item dropped here");

            Item R = new Item();
            R.type = ItemType.repair;
            R.value = 2000;
            R.useCost = 250;

            this.Player.Items.Add(R);

            this.Player.CountAllItems();
        }
    }
    private void SetNewEnemy(){
        do {
            this.enemyWaveIndex++;

            UpdateKillCount();

            if (this.enemyWaveIndex == this.monsterWaveSize){  // if end of enemy array get next wave
                ActionQueue.StopQueue();
                this.Enemy.Hide(true);
                Destroy(this.Enemy.gameObject);
                WaveOver();
                return;
            }
            Destroy(this.Enemy.gameObject);
            this.Enemy = this.EnemyList[enemyWaveIndex];
        }while(this.Enemy.healthPoints <= 0); // find alive enemy

        this.Enemy.StartAttack();
        this.Enemy.Hide(false);
    }
    private void WaveOver(){

        this.state = BattleState.WAVEOVER;
        this.wavesClearedCount++;

        ResetPlayerActionPoints();
        SetBackActionQueueComboLevel();
        InitiateAbilityDecayArray();

        PauseMenu.ShowPauseMenu(true);
    }
    private void UpdateKillCount(){
        
        this.KillCountText.text = this.enemyDefeatCount.ToString();
    }
    private void ResetPlayerActionPoints(){
        this.Player.spentActionPoints = 0;
        this.Player.currentActionPoints = 0;
    }
    private void SetBackActionQueueComboLevel(){
        if(ActionQueue.comboLevel >= 2) ActionQueue.SetComboLevel(2);
        else ActionQueue.SetComboLevel(1);
    }
#endregion

    public void PlayerDies(){
        this.state = BattleState.PLAYERDIED;
        ActionQueue.stopQueue = true;
        ActionQueue.ClearActionQueue(); // not really necessary
        PauseMenu.ShowPauseMenu(true);
    }

    public void End(){ 

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public async Task EnemyAttack(Enemy AttackingEnemy){

        if(!CheckForHit(AttackingEnemy.accuracy)){
            await this.Player.MissedAttack();
            return;
        }

        int damage = CalcUnitDamage(AttackingEnemy);

        if(CheckForHit(AttackingEnemy.crit)){
            damage = (int)Mathf.Round(damage*critMultiplier);
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
        Debug.Log("AOE("+unitCount.ToString()+") called");
        int damageDealt = 0;
        // saving original data for later
        int origDamage = AOEAction.baseDamage;
        bool oMH = AOEAction.multiHitON;

        // loop over unitCount units or all remaining units in the current wave, depending on which one is less
        for(int currentUnitIndex = (int)Mathf.Min((float)unitCount, (float)(this.monsterWaveSize - this.enemyWaveIndex)); currentUnitIndex > 0; currentUnitIndex-- ){
            // getting the Enemy
            Enemy Ec = this.EnemyList[this.enemyWaveIndex + currentUnitIndex-1];

            // setting new data to the Action
            AOEAction.Enemy = Ec;
            AOEAction.enemySet = true;
            AOEAction.damage = origDamage;
            AOEAction.multiHitON = oMH;

            // executing the new Action, a lot of steps get skipped after the first NormalExecute() -> see Action.NormalExecute()
            damageDealt += await AOEAction.NormalExecute();
        }
        return damageDealt;
    }
    public async Task<int> MultiHit(Action MultiHitAction, int hitCount){ 
        Debug.Log("MultiHit("+hitCount.ToString()+") called");

        int damageDealt = 0;
        // saving original damage for later
        int oDamage = MultiHitAction.damage;

        // deal all hits as long as the targeted enemy lives
        for(int currentHitCount = 0; currentHitCount < hitCount && MultiHitAction.Enemy.healthPoints > 0; currentHitCount++){
            // set damage and check MultiHitAction.enemySet for the case that the multihit doesnt target the main enemy (AOE+multihit)
            MultiHitAction.damage = oDamage;
            MultiHitAction.enemySet = true;
            damageDealt += await MultiHitAction.NormalExecute();
        }
        return damageDealt;
    }
    public async Task<int> DealOverFlow(int overFlowDamage){
        int totalDamageDealt = 0;

        // while there is still damage left to deal and still enemies left to deal it to
        while(overFlowDamage > 0 && EnemiesLeftInWave()){
            
            int damageDealt = await this.Enemy.DealDamage(overFlowDamage);

            totalDamageDealt += damageDealt;
            overFlowDamage -= damageDealt;
        }
        return totalDamageDealt;
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
    private int GetAbilityIndexByString(string givenActionName){
        Action ActionFromPlayerAbilityList = this.Player.Abilities.Find(action => action.name == givenActionName);

        if(ActionFromPlayerAbilityList != null) return ActionFromPlayerAbilityList.abilityIndex;
        else {
            Debug.LogError("Ability Name not found in Player Abilities");
            return -1;
        }
    }
    public int AbilityDecay(Action ActionIn){
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
        // for testing
        //this.log1 = decayVals;

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
    public SpellElement CombineElements( List<SpellElement> BaseElements){      // TODO

        return BaseElements[0];
    }
    public int ElementWeakTo(SpellElement AttackElement, SpellElement DefendElement){
        if(AttackElement == SpellElement.none) return 0;
        if(DefendElement == SpellElement.none) return -1;

        int attackEleIndex = GetElementIndex(AttackElement);
        int defendEleIndex = GetElementIndex(DefendElement);

        if(attackEleIndex < 0 || defendEleIndex < 0) Debug.LogError("Something went wrong!");

        return this.ElementWeaknessMatrix[attackEleIndex, defendEleIndex];
    }
    private int GetElementIndex(SpellElement Input){
        int index = -1;

        switch(Input){
            case SpellElement.WATER:
                index = 0;
            break;

            case SpellElement.FIRE:
                index = 1;
            break;

            case SpellElement.AIR:
                index = 2;
            break;

            case SpellElement.EARTH:
                index = 3;
            break;

            default:
                Debug.Log("Nani ?  Unknown Element?");
            break;
        }

        return index;
    }
#endregion
    
    private bool IndexInBoundsOfList(int index, int listSize){

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
    /*public bool UseHealItem(){
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
    /*public void QueueAttack(Action A){
        int baseDamage = CalcUnitDamage(Player);

        //int damageSpamDecay = (int)Mathf.Round((baseDamage*0.2f)*currentSpellDecay);

        int damage = baseDamage; // - damageSpamDecay;

        A.id = "BasicAttack";
        A.cover = "ATTACK";
        A.ap = Player.attackCost;
        //CurrentAction.textColor = playerAttackColor;

        ActionQueue.AddAction(A);                   // defined in ActionQueue.cs
    }
    public async Task ExecuteAttack(Action Attack){
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
    public async Task ExecuteSpell(Action Spell){
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