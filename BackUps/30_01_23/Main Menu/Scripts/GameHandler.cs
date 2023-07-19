using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] public MenuHandler MenuHandler;

    [Header("Menu Data")]
    [Header("")]

    [Header("Character")]
    [SerializeField] public PlayerCharacter Player;
    [SerializeField] public Weapon PlayerWeapon;
    [SerializeField] public Armor PlayerArmor;
    // [SerializeField] public GameObject PlayerWeaponPrefab;
    // [SerializeField] public GameObject PlayerArmorPrefab;

    [Header("Character Proficiency")]
    [SerializeField] public int playerProficiencyLevel;
    [SerializeField] public int playerTotalExperience;
    [SerializeField] public int playerExperienceToNextLevel;

    [SerializeField] public int playerSwordExp;
    [SerializeField] public int playerScytheExp;
    [SerializeField] public int playerAxeExp;
    [SerializeField] public int playerDaggerExp;

    public Dungeon CurrentDungeon;
    public bool newRunAvailable = false;

    public List<Dungeon> AvailableDungeonsList;

    [Header("Inventory")]
    [SerializeField] public List<GameObject> WeaponPrefabList;
    [SerializeField] public GameObject NULL_WeaponPrefab;
    [SerializeField] public List<Weapon> InventoryWeapons;
    [SerializeField] public List<GameObject> ArmorPrefabList;
    [SerializeField] public GameObject NULL_ArmorPrefab;
    [SerializeField] public List<Armor> InventoryArmor;
    [SerializeField] public List<Action> PlayerAbilityList;

    [SerializeField] public int totalCreditsEarned;

    public int maxProficiencyLevel = 100;

    public BattleSystem BattleSystem;

    [Header("Battle Multipliers")]
    [SerializeField] public float critMultiplier = 1.5f;
    [SerializeField] public float heavyAttackMultiplier = 1.2f;
    [SerializeField] public float specialAttackMultiplier = 1.1f;
    [SerializeField] public float ultAbilityDamageMultiplier = 1.1f;

    [SerializeField] public float attackIntoShieldMultiplier = 0.9f;

    [SerializeField] public float spellStrongMultiplier = 1.5f;
    [SerializeField] public float spellWeakMultiplier = 0.75f;

    private static GameHandler GameHandlerInstance;

    void Awake(){
        DontDestroyOnLoad(this.gameObject);

        if(GameHandlerInstance == null){
            GameHandlerInstance = this;
        } else {
            Destroy(this.gameObject);
            return;
        }

        // TestLevelLimitCalc();
        // TestAccuracyCalcualtion();
        // TestAbilityDecay();
        // TestComboHeat();

        if(SceneManager.GetActiveScene().name == "Main Menu Scene"){
            // For testing
            if(this.CurrentDungeon == null) this.CurrentDungeon = this.AvailableDungeonsList[0];
            if(this.BattleSystem == null){
                this.gameObject.AddComponent<BattleSystem>();
                this.BattleSystem = this.gameObject.GetComponent<BattleSystem>();
            }

            this.MenuHandler = GameObject.Find("Menu Handler").GetComponent<MenuHandler>();

            if(SaveDataAvailable()){
                Debug.Log("Loading Main Menu");
                this.MenuHandler.LoadMainMenu(this.MenuHandler.StartMenu);
            }
        }
    }

    public bool SaveDataAvailable(){

        return this.Player != null;
    }

    public void ReturnFromBattleScene(){
        UpdateDataFromNewRun();
        this.newRunAvailable = false;

        this.MenuHandler = GameObject.Find("Menu Handler").GetComponent<MenuHandler>();
        this.MenuHandler.LoadMainMenu(this.MenuHandler.StartMenu);
    }

    private void UpdateDataFromNewRun(){
        DungeonRun NewRun = this.CurrentDungeon.LastRun;

        this.totalCreditsEarned += NewRun.creditsEarned;

        switch(this.Player.Weapon.Type){
            case WeaponType.SWORD:
                this.playerSwordExp += NewRun.expEarned;
                break;

            case WeaponType.SCYTHE:
                this.playerScytheExp += NewRun.expEarned;
                break;

            case WeaponType.AXE:
                this.playerAxeExp += NewRun.expEarned;
                break;

            case WeaponType.DAGGER:
                this.playerDaggerExp += NewRun.expEarned;
                break;
        }
        this.playerTotalExperience += NewRun.expEarned;

        UpdatePlayerProficiency();
    }

    public void SetNewPlayerCharacter(PlayerCharacter NewPlayer){
        this.Player = NewPlayer;
        this.PlayerWeapon = null;
        this.PlayerArmor = null;

        this.PlayerAbilityList = new List<Action>();

        SetNewPlayerProficiency();
        UpdatePlayerProficiency();
        LoadNewInventoryLists();

        // TEMP
        SetPlayerItems();
    }

    public void SetNewWeapon(Weapon NewWeapon){
        this.PlayerWeapon = NewWeapon;
        this.Player.Weapon = this.PlayerWeapon;
        UpdatePlayerProficiency();

        this.PlayerWeapon.Setup(this.Player);
        this.PlayerWeapon.InitAbilityList();

        this.Player.Abilities = this.PlayerWeapon.AbilityList;
        // this.PlayerAbilityList = W.AbilityList;
        this.PlayerAbilityList = this.PlayerWeapon.GetCompleteMoveList();
    }

    private void InitWeaponAbility(){
        foreach(Weapon W in this.InventoryWeapons){
            W.InitAbilityList();
        }
    }

    public void SetNewArmor(Armor NewArmor){
        this.PlayerArmor = NewArmor;
        this.PlayerArmor.Setup(this.Player);
    }

    private void SetNewPlayerProficiency(){
        this.playerSwordExp = this.Player.baseSwordExp;
        this.playerScytheExp = this.Player.baseScytheExp;
        this.playerAxeExp = this.Player.baseAxeExp;
        this.playerDaggerExp = this.Player.baseDaggerExp;
    }

    public void UpdatePlayerProficiency(){
        this.playerProficiencyLevel = 0;
        this.playerExperienceToNextLevel = 0;

        if(this.PlayerWeapon != null){
            UpdateTotalExperience();
            int expLeft = this.playerTotalExperience;
            int levelLimit = 0;

            while(expLeft >= levelLimit && this.playerProficiencyLevel < this.maxProficiencyLevel){
                expLeft -= levelLimit;
                this.playerProficiencyLevel++;
                levelLimit = GetLevelLimit(this.playerProficiencyLevel);
            }

            this.playerExperienceToNextLevel = expLeft;
        }

        this.Player.proficiencyLevel = this.playerProficiencyLevel;
    }

    private void LoadNewInventoryLists(){
        this.InventoryArmor = new List<Armor>();
        foreach(GameObject A_GO in this.ArmorPrefabList){
            this.InventoryArmor.Add(A_GO.GetComponent<Armor>());
        }

        this.InventoryWeapons = new List<Weapon>();
        foreach(GameObject W_GO in this.WeaponPrefabList){
            Weapon W = W_GO.GetComponent<Weapon>();
            switch(W.Type){
                case WeaponType.SWORD:
                    if(this.Player.baseSwordExp >= 0){
                        this.InventoryWeapons.Add(W);
                    }
                    break;

                case WeaponType.SCYTHE:
                    if(this.Player.baseScytheExp >= 0){
                        this.InventoryWeapons.Add(W);
                    }
                    break;

                case WeaponType.AXE:
                    if(this.Player.baseAxeExp >= 0){
                        this.InventoryWeapons.Add(W);
                    }
                    break;

                case WeaponType.DAGGER:
                    if(this.Player.baseDaggerExp >= 0){
                        this.InventoryWeapons.Add(W);
                    }
                    break;
            }
        }
    }

    private void UpdatePlayerAbilityList(){

    }

    private void UpdateTotalExperience(){
        switch(this.PlayerWeapon.Type){
            case WeaponType.SWORD:
                this.playerTotalExperience = this.playerSwordExp;
            break;

            case WeaponType.SCYTHE:
                this.playerTotalExperience = this.playerScytheExp;
            break;

            case WeaponType.DAGGER:
                this.playerTotalExperience = this.playerDaggerExp;
            break;

            case WeaponType.AXE:
                this.playerTotalExperience = this.playerAxeExp;
            break;
        }
    } 

    public void SetNewDungeon(Dungeon NewDungeon){
        this.CurrentDungeon = NewDungeon;
    }

    public string TimeToString(int time){
        // TEMP
        return "00:00.00";
    }

    public void StartDungeon(){
        // PrintStatus();
        // ReadyPlayer();
        SceneManager.LoadScene("Battle Scene");
    } // TODO

    private void ReadyPlayer(){
        if(this.PlayerWeapon == null){
            Debug.Log("No weapon, setting NULLWeapon");
        }

        if(this.PlayerArmor == null){
            Debug.Log("No armor, setting NULLArmor");
        }
    } // TODO

    public void ResetData(){

        Debug.Log("ResetData() called");
    } // TODO

    public int CalcUnitDamage(Unit U){ 

        return Random.Range(U.attackMin, U.attackMax+1); 
    }

    public void PrintStatus(){
        Debug.Log("Printing Status: \n");
        this.Player.PrintStatus();
        this.PlayerWeapon.PrintStatus();
        this.PlayerArmor.PrintStatus();
        this.CurrentDungeon.PrintStatus();
    }

    // TEMP
    private void SetPlayerItems(){

        Item R = new Item();
        R.type = ItemType.repair;
        R.value = 2000;
        R.useCost = 250;

        this.Player.Items.Add(R);
        this.Player.CountAllItems();
    } // TODO

    public int GetLevelLimit(int currentLevel){
        if(currentLevel == 0) return 0;
        if(currentLevel > this.maxProficiencyLevel) Debug.LogError("Level Limit is "+this.maxProficiencyLevel.ToString()+"!");

        int decaDigit = (int)Mathf.Ceil(currentLevel/10);
        float exp = 1.25f + 0.25f * decaDigit;
        int res = (int)Mathf.Round((250*(decaDigit+1)) + Mathf.Pow(currentLevel,exp));

        return res;
    }

    private void TestLevelLimitCalc(){
        int amountLevelsChecked = 99;
        int expToLvl100 = 0;
        string s = "Checking till Level "+ amountLevelsChecked.ToString()+"\n"
                  +"Calculated Limits:\n";
        for(int c_level = 0; c_level <= amountLevelsChecked; c_level++){
            if(c_level%10 == 0) s += "\n";
            int limit = GetLevelLimit(c_level);
            expToLvl100+=limit;
            s += "Level: " + c_level.ToString()+" | exp: "+ limit.ToString()+"\n";
        }
        Debug.Log(s);
        Debug.Log(expToLvl100);
    }

    private void TestAccuracyCalcualtion(){
        int level1AccuracyBase = 75;
        Debug.Log("Testing Accuracy Calculation:");
        string s =  "";

        for(int cLvl = 1; cLvl < 6; cLvl++){
            int baseAccuracy = level1AccuracyBase - (10*(cLvl-1))-1; // i.e. [75,65,55,45,35] for [1,2.3,4,5]
            int levelOfMaxAccuracy = (int)Mathf.Clamp((cLvl-1)*30, 0.0f, float.MaxValue);   // ie [20,40,60,80,100], maxAccuracy == 95% as to not make bonus acc effect useless
            
            float accPerLvl = (95.0f-(float)baseAccuracy)/(float)levelOfMaxAccuracy;
            s += "Combo Level: "+cLvl.ToString()+"\n"
                +"Level of MaxAccuracy: "+levelOfMaxAccuracy.ToString()+"\n"
                +"Accuracy Per Level: "+ accPerLvl.ToString()+"\n\n";

            for(int p_lvl = 0; p_lvl < 101; p_lvl+=5){
                if(p_lvl == 0) p_lvl++;

                int acc = (int)Mathf.Clamp(Mathf.Round(baseAccuracy+(p_lvl*accPerLvl)), 0.0f, 95.0f);

                s += "P Level: "+p_lvl.ToString()+" | Accuracy: "+acc.ToString()+"\n";


                if(p_lvl == 1) p_lvl--;
            }
            s += "\n\n";
        }

        Debug.Log(s);
    }

    private void TestAbilityDecay(){
        int abilityDecayLevelCount = 10;
        int actionQueueComboLevel = 2;

        int[] AbilityAllocationArray = {1, 1, 0, 0, 0}; // ie how many abilities of combolevel 1-5
        int abilityCount = 0;
        int highestComboLevel = 0;
        for(int i = 0; i < AbilityAllocationArray.Length; i++){
            abilityCount += AbilityAllocationArray[i];
            if(AbilityAllocationArray[i] > 0) highestComboLevel = i+1;
        }

        if(actionQueueComboLevel > highestComboLevel) {
            Debug.LogError("Have to adjust action queue combo level to allocated actions");
            return;
        }



        List<Action> TestAbilities = GetTestAbilityList(abilityCount, AbilityAllocationArray);

        int[] AbilityDecayArray = InitiateAbilityDecayArray(abilityCount, TestAbilities, abilityDecayLevelCount);

        Debug.Log("After Init: ");
        PrintTestDecayArray(AbilityDecayArray);

        int test_index = 0;
        Action TestAction = TestAbilities[test_index];
        UpdateAbilityDecayArray(TestAction, AbilityDecayArray, TestAbilities, abilityDecayLevelCount, actionQueueComboLevel);
        Debug.Log("After Updating with ability of index "+test_index.ToString());
        PrintTestDecayArray(AbilityDecayArray);

        test_index = 0;
        TestAction = TestAbilities[test_index];
        UpdateAbilityDecayArray(TestAction, AbilityDecayArray, TestAbilities, abilityDecayLevelCount, actionQueueComboLevel);
        Debug.Log("After Updating with ability of index "+test_index.ToString());
        PrintTestDecayArray(AbilityDecayArray);

        // test_index = 0;
        // TestAction = TestAbilities[test_index];
        // UpdateAbilityDecayArray(TestAction, AbilityDecayArray, TestAbilities, abilityDecayLevelCount, actionQueueComboLevel);
        // Debug.Log("After Updating with ability of index "+test_index.ToString());
        // PrintTestDecayArray(AbilityDecayArray);

        // test_index = 0;
        // TestAction = TestAbilities[test_index];
        // UpdateAbilityDecayArray(TestAction, AbilityDecayArray, TestAbilities, abilityDecayLevelCount, actionQueueComboLevel);
        // Debug.Log("After Updating with ability of index "+test_index.ToString());
        // PrintTestDecayArray(AbilityDecayArray);

        // test_index = 0;
        // TestAction = TestAbilities[test_index];
        // UpdateAbilityDecayArray(TestAction, AbilityDecayArray, TestAbilities, abilityDecayLevelCount, actionQueueComboLevel);
        // Debug.Log("After Updating with ability of index "+test_index.ToString());
        // PrintTestDecayArray(AbilityDecayArray);

    }

    private List<Action> GetTestAbilityList(int abilityCount, int[] AbilityAllocationArray){
        List<Action> TestAbilities = new List<Action>();
        int c_abilityIndex = 0;
        for(int comboLevel = 1; comboLevel < 6; comboLevel++){
            for(int i = 0; i < AbilityAllocationArray[comboLevel-1]; i++){
                Action Test_Action = new PlaceHolderAction(comboLevel);
                Test_Action.abilityIndex = c_abilityIndex;
                c_abilityIndex++;
                TestAbilities.Add(Test_Action);
            }  
        }
        return TestAbilities;
    }

    private int[] InitiateAbilityDecayArray(int abilityCount, List<Action> AbilityList, int abilityDecayLevelCount){
        int[] AbilityDecayArray = new int[abilityCount];
        foreach(Action A in AbilityList){
            AbilityDecayArray[A.abilityIndex] = abilityDecayLevelCount;
        }
        return AbilityDecayArray;
    }

    private void PrintTestDecayArray(int[] AbilityDecayArray){
        string s = "Decay Values: \n";

        foreach(int decay_val in AbilityDecayArray){
            s += decay_val.ToString() + " ";
        }

        Debug.Log(s);
    }
    
    private void UpdateAbilityDecayArray(Action TestAction, int[] AbilityDecayArray, List<Action> AbilityList, int abilityDecayLevelCount, int AQ_Level){
        int[] decayVals = GetComboDecayValues(AbilityList, AQ_Level);

        int decayLossForThisAction = decayVals[TestAction.comboLevel-1];
        AbilityDecayArray[TestAction.abilityIndex] = (int)Mathf.Clamp(AbilityDecayArray[TestAction.abilityIndex] - decayLossForThisAction, 0.0f, float.MaxValue);

        // all actions with a lower combolevel then ActionIn get +1 onto their ability decay level
        foreach(Action A in AbilityList){
            if(A.abilityIndex != TestAction.abilityIndex && A.comboLevel <= TestAction.comboLevel) {
                AbilityDecayArray[A.abilityIndex] = (int)Mathf.Clamp(AbilityDecayArray[A.abilityIndex]+1, 0.0f, abilityDecayLevelCount);
            }
        }
    }

    private int[] GetComboDecayValues(List<Action> AbilityList, int AQ_ComboLevel){

        // counting how many abilities are of combolevel 1 - 5
        int[] cAttacks = {0,0,0,0,0}; 
        foreach(Action A in AbilityList) cAttacks[A.comboLevel-1]++;

        // getting decay loss for actions of every comboLevel based on how many actions of that combo level are available to the player
        int[] res = {0,0,0,0,0};
        for(int i=0;i<res.Length;i++){
            res[i] = cAttacks[i];
            // if actions of higher comboLevel cant be executed with current ActionQueue ComboLevel: -1 on the value
            if(i+1 >= AQ_ComboLevel) res[i]--;
        } 

        return res;
    }

    private void TestComboHeat(){
    }

#region After Battle Functions
    public void SetNewRun(DungeonRun NewRun){
        this.newRunAvailable = true;
        this.CurrentDungeon.AddNewRun(NewRun);
    } // TODO
#endregion


// #region Action Heat Functions
//     private void AddHeat(Action CurrentAction){
//         int heatGain = CalculateHeatGainFromAction(CurrentAction);

//         this.heatGainThisRound += heatGain;
//         this.currentHeat += heatGain;
//     }
//     private void RoundEndHeat(int actionsExecutedCount){
//         if(IsFullComboExecuted(actionsExecutedCount)) this.currentHeat += GetFullComboHeatGain();
//         CalculateNewComboLevel();
//         RoundEndHeatDecrease();
//         CalculateNewComboLevel();
//         this.heatGainThisRound = 0;
//     }
//     private int CalculateHeatGainFromAction(Action A){
//         float heatMultiplicator = ((float)BattleSystem.abilityDecayLevelCount/100)*BattleSystem.AbilityDecayArray[A.abilityIndex];
//         //Debug.Log("Heat: +"+((int)Mathf.Round(heatValues[A.comboLevel-1] * heatMultiplicator)).ToString());

//         int heatGain = (int)Mathf.Round(heatValues[A.comboLevel-1] * heatMultiplicator);
//         return heatGain;
//     }
//     private bool IsFullComboExecuted(int actionsExecutedCount){

//         return actionsExecutedCount == this.maxMovesQueued;
//     }
//     private int GetFullComboHeatGain(){

//         return (int)Mathf.Round(((float)this.heatGainThisRound * this.fullComboHeatf) - this.heatGainThisRound);
//     }
//     private void CalculateNewComboLevel(){
//         this.comboLevel = 0;
//         while(this.currentHeat >= this.heatLimits[this.comboLevel] && this.comboLevel+1 <= this.maxComboLv) this.comboLevel++;

//         ComboLevelText.text = "Lv."+comboLevel.ToString();
//         //Debug.Log("ComboLv: "+this.comboLevel.ToString());
//     }
//     private void RoundEndHeatDecrease(){
//         int oldHeat = this.currentHeat;

//         this.currentHeat -= (int)Mathf.Round((this.currentHeat - this.heatLimits[this.comboLevel-1])*this.roundEndHeatf);
//         if(this.currentHeat>0)this.currentHeat--;
//         //Debug.Log("CurrentHeat = "+oldHeat.ToString()+" - "+(oldHeat - this.currentHeat).ToString());
//     }
// #endregion
}
