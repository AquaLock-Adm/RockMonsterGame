using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PlayerState { WAITING, START, PLAYERTURN, QUEUE, ATTACKRUSH, DEAD }

public class PlayerCharacter : Unit
{
    protected PlayerBattleResourceHandler ResourceHandler;
    protected PlayerControl Controls;
    protected PlayerActionHandler ActionHandler;

    public PlayerState state = PlayerState.WAITING;
    public bool defendModeActive = false;
    public BattleSystem BattleSystem;

    [Header("HUD")]
    private GameObject ActionHUD;
    protected List<TextMeshProUGUI> MenuTexts = new List<TextMeshProUGUI>();
    protected List<GameObject> ReferencesForActionHandler = new List<GameObject>();

    [Header("Player Stats:")]

    [SerializeField] private Weapon Weapon;
    [SerializeField] private Armor Armor;

    public bool deathTriggered = false;
    private bool heatChargeAvailable = false;

    [SerializeField] private int baseBattleSpeed = 5;
    [SerializeField] private int battleSpeed = 5;


    #region overrides
    public async override Task Death()
    {
        if (!this.deathTriggered) return;
        this.healthPoints = 0;
        this.ResourceHandler.UpdateHUDElements();


        this.ActionHandler.StopQueue();
        state = PlayerState.DEAD;

        BattleSystem.PlayerDies();
        await Task.Yield();
    }
    #endregion



    #region BattleSetup
    public void BattleSetup(BattleSystem BS, GameObject ActionHUD, int weaponStartLevel = 1)
    {
        if (this.Weapon != null)
        {
            if (weaponStartLevel > 1) Weapon.UpgradeWeaponToLevel(weaponStartLevel);
            if (Weapon.actionsPerRound >= 5) this.heatChargeAvailable = true;
        }
        else Debug.LogError("Weapon is not set");

        if (this.Armor == null) Debug.LogError("Armor is not set");

        this.BattleSystem = BS;
        this.ActionHUD = ActionHUD;
        this.state = PlayerState.WAITING;

        SetupHUDReferences();
        SetupBattleComponents();
    }

    private void SetupHUDReferences()
    {
        SetupMenuTexts();
        SetupReferencesForActionHandler();
    }

    private void SetupMenuTexts()
    {
        this.MenuTexts = new List<TextMeshProUGUI>();
        int textsFound = 0;

        foreach (Transform Button_T in this.ActionHUD.transform.Find("MainMenu"))
        {
            TextMeshProUGUI t = Button_T.Find("Button Text").gameObject.GetComponent<TextMeshProUGUI>();
            if (t != null) textsFound++;

            MenuTexts.Add(Button_T.Find("Button Text").gameObject.GetComponent<TextMeshProUGUI>());
        }

        if (textsFound < 4)
        {
            Debug.LogError("Not enough Button Texts found! " + textsFound.ToString() + "/4");
        }
    }

    private void SetupReferencesForActionHandler()
    {
        this.ReferencesForActionHandler = new List<GameObject>();

        // NOTE: The order of these GO's is important! Ref: PlayerActionHandler.SetupHUDReferences()
        if (BattleSystem.ActionBoxPrefab != null)
        {
            this.ReferencesForActionHandler.Add(BattleSystem.ActionBoxPrefab);
        }
        else Debug.LogError("ActionBoxPrefab Missing!");

        this.ReferencesForActionHandler.Add(this.ActionHUD.transform.Find("Combo Level Text").gameObject);
        this.ReferencesForActionHandler.Add(this.ActionHUD.transform.Find("Action Text").gameObject);
        this.ReferencesForActionHandler.Add(this.ActionHUD.transform.Find("Heat Progress Bar").gameObject);
        this.ReferencesForActionHandler.Add(this.ActionHUD.transform.Find("Action Boxes").gameObject);
        this.ReferencesForActionHandler.Add(this.ActionHUD.transform.Find("Attack Rush Bar").gameObject);
        this.ReferencesForActionHandler.Add(this.ActionHUD.transform.Find("Attack Rush Bar/Remain Box").gameObject);
        this.ReferencesForActionHandler.Add(this.ActionHUD.transform.Find("Attack Rush Bar/Decrease Box").gameObject);
    }

    protected virtual void SetupBattleComponents()
    {
        // Reminder: Setup Actionshandler before Player control so the start menu knows that the heatcharge is done on apr < 5
        this.ActionHandler = this.gameObject.AddComponent<PlayerActionHandler>();
        this.ActionHandler.Setup(this, this.ReferencesForActionHandler);

        this.ResourceHandler = this.gameObject.AddComponent<PlayerBattleResourceHandler>();
        this.ResourceHandler.Setup(this, BattleSystem.PlayerNameText, BattleSystem.PlayerHpSlider);

        this.Controls = this.gameObject.AddComponent<PlayerControl>();
        this.Controls.Setup(this, BattleSystem.InputDarkFilter, this.MenuTexts);
    } // Changed in: Player_Tutorial.cs
    #endregion



    #region MenuSetup
    public void MenuSetup(Weapon PlayerWeapon, Armor PlayerArmor)
    {
        this.Weapon = PlayerWeapon;
        this.Armor = PlayerArmor;
        PlayerWeapon.Init(this);
        PlayerArmor.Init(this);
    }
    #endregion

    public void CopyFrom(PlayerCharacter P_IN)
    {

        this.unitName = P_IN.unitName;

        this.healthPoints = P_IN.healthPoints;
        this.maxHealthPoints = P_IN.maxHealthPoints;

        this.Weapon = P_IN.Weapon;
        this.Armor = P_IN.Armor;
    }

    public void PrintStatus()
    {
        Debug.Log("Status Player " + this.unitName + ":");
        string s = "\nHUD References:\n"
                + "BattleSystem Set: " + (this.BattleSystem != null).ToString() + "\n\n"

                + "Player Stats:\n"
                + "Weapon: " + this.Weapon.weaponName + "\n"
                + "Armor: " + this.Armor.armorName + "\n\n"

                + "Health: " + this.Armor.healthPoints.ToString() + "/" + this.Armor.maxHealthPoints.ToString() + "\n\n"

                + "MaxAtk: " + this.Weapon.attackMax.ToString() + "\n"
                + "MinAtk: " + this.Weapon.attackMin.ToString() + "\n";
        Debug.Log(s);
        PrintAbilities();
    }

    public void PrintAbilities()
    {
        string s = "\nAbilities:\n\n";
        if (this.Weapon.Abilities.Count == 0)
        {
            s += "EMPTY";
        }
        else
        {
            foreach (Action A in this.Weapon.Abilities)
            {
                s += A.ToString() + "\n";
            }
        }
        Debug.Log(s);
    }

    public void NextWave()
    {
        this.ActionHandler.CheckHeatChargeAvailability();
        this.ActionHandler.attackRushUsed = false;
        this.Controls.LoadMainMenu();
    }

    public void PassRound()
    {
    // private int baseBattleSpeed;
    // private int battleSpeed;
        // use only vals 1-10

        int enemySpeedVal = 3;

        if(this.battleSpeed >= enemySpeedVal){
            Debug.Log("Would be Attack Turn.\n"+battleSpeed.ToString()+","+enemySpeedVal.ToString());
            this.battleSpeed -= enemySpeedVal;
        }else{
            Debug.Log("Would be Defend Turn.\n"+battleSpeed.ToString()+","+enemySpeedVal.ToString());
            this.battleSpeed += this.baseBattleSpeed;
        }

        this.ActionHandler.PassRound();
    }

    public virtual void SwitchBattleModes()
    {
        this.defendModeActive = !this.defendModeActive;

        BattleSystem.Enemy.SwitchModes(this.defendModeActive);
        this.ActionHandler.SwitchModes(this.defendModeActive);
        this.Controls.LoadMainMenu();
    } // Changed in: Player_Tutorial.cs

    public int GetAbilityIndexByString(string givenActionName)
    {
        Action ActionFromPlayerAbilityList = this.Weapon.Abilities.Find(action => action.name == givenActionName);

        if (ActionFromPlayerAbilityList != null) return ActionFromPlayerAbilityList.abilityIndex;
        else
        {
            Debug.LogError("Ability Name not found in Player Abilities: " + givenActionName);
            return -1;
        }
    }

    public void BattleEnd(){
        ResourceHandler.BattleEnd();
        Controls.BattleEnd();
        ActionHandler.BattleEnd();
        ResetHealth();
    }

    private void ResetHealth(){
        this.Armor.healthPoints = this.Armor.maxHealthPoints;
        this.healthPoints = this.maxHealthPoints;
    }



    #region Resource Handler Functions
    public void DealDamage(int damage)
    {
        this.ResourceHandler.DealDamage(damage);
    }
    public void Heal(int heal)
    {
        this.ResourceHandler.Heal(heal);
    }
    #endregion



    #region Action Handler Functions
    public void CastBlock(int abilityIndex)
    {
        this.ActionHandler.CastBlock(abilityIndex);
    }
    public void CastAttack(int abilityIndex)
    {
        this.ActionHandler.CastAttack(abilityIndex);
    }
    public void CancelLastAction()
    {
        this.ActionHandler.CancelLastAction();
    }
    public void StopAttackRushQueue()
    {
        this.ActionHandler.StopAttackRushQueue();
    }
    public void StartAttackRushQueue()
    {
        this.ActionHandler.StartAttackRushQueue();
    }
    public void StartAttackRushHeatDrain()
    {
        this.ActionHandler.StartAttackRushHeatDrain();
    }
    public void CancelAttackRush()
    {
        this.ActionHandler.CancelAttackRush();
    }
    public Action PredictComboAbilityWith(Action ActionToAdd)
    {
        return this.ActionHandler.PredictComboAbilityWith(ActionToAdd);
    }
    public void StartHeatChargeLoop()
    {
        this.ActionHandler.StartHeatChargeLoop();
    }
    public void StopHeatChargeLoop()
    {
        this.ActionHandler.StopHeatChargeLoop();
    }
    public void StartHeatDischargeLoop()
    {
        this.ActionHandler.StartHeatDischargeLoop();
    }
    public void StopHeatDischargeLoop()
    {
        this.ActionHandler.StopHeatDischargeLoop();
    }
    public bool HeatChargeIsDone()
    {
        return this.ActionHandler.heatChargeDone;
    }
    public bool IsInHeatCharge()
    {
        return this.ActionHandler.inHeatCharge;
    }
    public bool IsInHeatDischarge()
    {
        return this.ActionHandler.inHeatDischarge;
    }
    public int GetCurrentActionCount()
    {
        return this.ActionHandler.Actions.Count;
    }
    public int GetCurrentComboLevel()
    {
        return this.ActionHandler.comboLevel;
    }
    public void StopQueue()
    {
        this.ActionHandler.StopQueue();
    }
    public void ResetHeatLevel()
    {
        this.ActionHandler.ResetHeatLevel();
    }
    #endregion

    #region Player Control Functions
    public void EnableAttackRushInputs(bool on)
    {
        this.Controls.EnableAttackRushInputs(on);
    }
    public void BlockAllInputsFor(int time_ms)
    {
        this.Controls.BlockAllInputsFor(time_ms);
    }
    public void BlockPlayerInputs(List<bool> playerInputBlockList)
    {
        this.Controls.BlockPlayerInputs(playerInputBlockList);
    }
    public void LoadMainMenu()
    {
        this.Controls.LoadMainMenu();
    }
    public void LoadRoundOverMenu()
    {
        this.Controls.LoadRoundOverMenu();
    }
    public void LoadComboAbilityMenu()
    {
        this.Controls.LoadComboAbilityMenu();
    }
    #endregion



    #region Getter - Setter
    public int GetWeaponLevel()
    {
        return this.Weapon.weaponLevel;
    }
    
    public int GetAttackMin()
    {
        return this.Weapon.attackMin;
    }

    public int GetAttackMax()
    {
        return this.Weapon.attackMax;
    }

    public float GetLifeDrain()
    {
        return this.Weapon.healthCostPerSecond;
    }

    public int GetLifeSteal() 
    {
        return this.Weapon.lifeSteal;
    }
    
    public float GetEnemyDOT()
    {
        return this.Weapon.enemyDamagePerSecond;
    }

    public int GetWeaponUpgradeCost()
    {
        return this.Weapon.upgradeCost;
    }

    public int GetActionsPerRound()
    {
        return this.Weapon.actionsPerRound;
    }

    public Weapon GetWeapon()
    {
        return this.Weapon;
    }

    public Enemy GetCurrentEnemy()
    {
        return BattleSystem.Enemy;
    }

    public bool IsHeatChargeAvailable()
    {
        return this.heatChargeAvailable;
    }

    public void SetWeapon(Weapon W)
    {
        this.Weapon = W;
    }
    
    public Armor GetArmor()
    {
        return this.Armor;
    }
    
    public int GetArmorLevel()
    {
        return this.Armor.armorLevel;
    }

    public int GetArmorHealth()
    {
        return this.Armor.maxHealthPoints;
    }

    public int GetArmorMana()
    {
        return this.Armor.maxMana;
    }

    public int GetArmorManaRegen()
    {
        return this.Armor.manaRegen_s;
    }

    public int GetArmorUpgradeCost()
    {
        return this.Armor.upgradeCost;
    }

    public void SetArmor(Armor A)
    {
        this.Armor = A;
        this.healthPoints = A.healthPoints;
        this.maxHealthPoints = A.maxHealthPoints;
    }

    public List<Action> GetAbilityList()
    {
        return this.Weapon.Abilities;
    }

    public List<Action> GetBlockAbilities()
    {
        return this.Weapon.Blocks;
    }
    #endregion
}