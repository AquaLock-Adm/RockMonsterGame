using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacter : Unit
{
    [SerializeField] private Text NameText;
    [SerializeField] private Slider HpSlider;
    [SerializeField] private Slider ManaSlider;

    private BattleSystem BattleSystem;

    [Header("Player Stats:")]

    [SerializeField] private Weapon Weapon;
    [SerializeField] public Armor Armor;

    private int maxMana;
    private int mana;
    [SerializeField] private int manaRegenTickTime_ms = 50;
    private float manaRegenPerTick = 0.0f;
    private float manaRegenerated = 0.0f;

    [SerializeField] private int lifeDrainTickTime_ms = 50;
    private float lifeDrainPerTick = 1.0f;
    private float lifeDrained = 0.0f;

    [SerializeField] private int dotTickTime_ms = 50;

    public bool deathTriggered = false;

#region overrides
    public async override Task Death(){
        if(!this.deathTriggered) return;
        this.healthPoints = 0;
        UpdateHUDElements();

        BattleSystem.PlayerDies();
        await Task.Yield();
    }
#endregion

    public void BattleSetup(BattleSystem BS){
        if(this.Weapon != null) {
            GameObject Weapon_GO = Instantiate(this.Weapon.gameObject);
            Weapon_GO.GetComponent<Weapon>().CopyFrom(this.Weapon);
            this.Weapon = Weapon_GO.GetComponent<Weapon>();

            this.Weapon.Init(this);
        }else Debug.Log("Weapon is not set");
        if(this.Armor != null) {
            GameObject Armor_GO = Instantiate(this.Armor.gameObject);
            Armor_GO.GetComponent<Armor>().CopyFrom(this.Armor);
            this.Armor = Armor_GO.GetComponent<Armor>();

            this.Armor.Init(this);
        }else Debug.Log("Armor is not set");

        this.BattleSystem = BS;
        SetupHUDElements();
        this.NameText.text = this.unitName;

        // PrintStatus();

        // ManaRegenLoop();
        HealthDepleteLoop();
        if(this.Weapon.actionsPerRound > 5) EnemyDOTLoop();
        BattleUpdateLoop();
    }

    public void Heal(int healAmount){
        this.Armor.Repair(healAmount);
        this.healthPoints = this.Armor.healthPoints;
        if(this.deathTriggered) {
            this.deathTriggered = false;
            HealthDepleteLoop();
        }
        UpdateHUDElements();
    }

    public void DealDamage(int damage){
        this.healthPoints = (int)Mathf.Max(0f, (float)( this.healthPoints - damage ));

        if(this.healthPoints <= 0) {
            this.deathTriggered = true;
        }
    }

    public void CopyFrom(PlayerCharacter P_IN){
        this.depletionTime = P_IN.depletionTime;
        this.pointsPerTick = P_IN.pointsPerTick;
        this.tickRate = P_IN.tickRate;
        this.textShowTime = P_IN.textShowTime;
        
        this.unitName = P_IN.unitName;
        
        this.healthPoints = P_IN.healthPoints;
        this.maxHealthPoints = P_IN.maxHealthPoints;
        
        this.Weapon = P_IN.Weapon;
        this.Armor = P_IN.Armor;

        this.maxMana = P_IN.maxMana;
        this.mana = P_IN.maxMana;
    }

    public void PrintStatus(){
        Debug.Log("Status Player "+ this.unitName+":");
        string s = "\nHUD References:\n"
                +  "NameText Set: "+ (this.NameText != null).ToString()+"\n"
                // +  "ApText Set: " + (this.ApText != null).ToString()+"\n"
                +  "HpSlider Set: " + (this.HpSlider != null).ToString()+"\n"
                +  "ManaSlider Set: " + (this.ManaSlider != null).ToString()+"\n"
                +  "BattleSystem Set: " + (this.BattleSystem != null).ToString()+"\n\n"

                +  "Player Stats:\n"
                +  "Weapon: " + this.Weapon.weaponName+"\n"
                +  "Armor: " + this.Armor.armorName+"\n\n"

                +  "Health: " + this.Armor.healthPoints.ToString()+"/"+this.Armor.maxHealthPoints.ToString()+"\n\n"

                +  "MaxAtk: " + this.Weapon.attackMax.ToString()+"\n"
                +  "MinAtk: " + this.Weapon.attackMin.ToString()+"\n\n"

                +  "MaxMana: " + this.maxMana.ToString()+"\n"
                +  "Mana: " + this.mana.ToString()+"\n";
        Debug.Log(s);
        PrintAbilities();
    }

    public void PrintAbilities(){
        string s = "\nAbilities:\n\n";
        if(this.Weapon.Abilities.Count == 0){
            s += "EMPTY";
        } else {
            foreach(Action A in this.Weapon.Abilities){
                s += A.ToString()+"\n";
            } 
        }
        Debug.Log(s);
    }

    private async void BattleUpdateLoop(){
        while(!this.deathTriggered){
            if(this.maxMana > 0) CalcManaRegenPerTick();
            UpdateHUDElements();
            UpdateArmorStats();
            await Task.Yield();
        }
    }

    private async void ManaRegenLoop(){
        if(this.maxMana <= 0) return;
        CalcManaRegenPerTick();
        await Task.Delay(this.manaRegenTickTime_ms);
        while(this.healthPoints > 0){
            if(BattleSystem.state == BattleState.PLAYERTURN || BattleSystem.state == BattleState.QUEUE){
                this.manaRegenerated += this.manaRegenPerTick;
                int manaRegen = 0;
                if(this.manaRegenerated >= 1.0f){
                    manaRegen = (int)this.manaRegenerated;
                    this.manaRegenerated -= (float)manaRegen;
                }
                this.mana = (int)Mathf.Min(this.maxMana, this.mana + manaRegen);
                await Task.Delay(this.manaRegenTickTime_ms);
            }else await Task.Yield();
        }
    }

    private void CalcManaRegenPerTick(){
        this.manaRegenPerTick = (float)((float)this.Armor.manaRegen_s / (1000.0f/(float)this.manaRegenTickTime_ms));
        if(BattleSystem.ActionQueue.comboLevel >= 5) this.manaRegenPerTick *= 2;
    }

    private async void HealthDepleteLoop(){
        while(this.healthPoints > 0 && Application.isPlaying){
            if(BattleSystem.state == BattleState.PLAYERTURN){

                CalcLifeDrainPerTick();
    
                if(BattleSystem.ActionQueue.comboLevel >= 5) this.lifeDrained += this.lifeDrainPerTick*2;
                else this.lifeDrained += this.lifeDrainPerTick;

                int lifeLoss = 0;
                if(this.lifeDrained >= 1.0f){
                    lifeLoss = (int)this.lifeDrained;
                    this.lifeDrained -= (float)lifeLoss;
                }
                this.healthPoints -= lifeLoss;

                if(this.healthPoints <= 0) {
                    this.deathTriggered = true;
                    await Death();
                    continue;
                }

                await Task.Delay(this.lifeDrainTickTime_ms);
            }else await Task.Yield();
        }
    }

    private async void EnemyDOTLoop(){
        if(this.Weapon.enemyDamagePerSecond <= 0f) return;
        float dotPerTick = this.Weapon.enemyDamagePerSecond / ( 1000.0f/(float)this.dotTickTime_ms ); 

        await Task.Delay(this.dotTickTime_ms);
        float cDOT = 0f;
        while(this.healthPoints > 0){
            if(BattleSystem.ActionQueue.comboLevel > 5
                && (BattleSystem.state == BattleState.PLAYERTURN || BattleSystem.state == BattleState.QUEUE)){
                cDOT += dotPerTick;
                if(cDOT >= 1f){
                    int dot = (int)cDOT;
                    cDOT -= (float)dot;
                    if(BattleSystem.Enemy != null){
                        BattleSystem.Enemy.DealWeaponDOT(dot);
                    }
                }
                await Task.Delay(this.dotTickTime_ms);
            }else await Task.Yield();
        }
    }

    private void CalcLifeDrainPerTick(){
        this.lifeDrainPerTick = (float)(this.Weapon.healthCostPerSecond / (1000.0f/(float)this.lifeDrainTickTime_ms)); 
    }

    private void SetupHUDElements() {
        this.NameText = this.BattleSystem.PlayerNameText;
        this.HpSlider = this.BattleSystem.PlayerHpSlider;
        this.ManaSlider = this.BattleSystem.PlayerManaSlider;
    }

    private void UpdateHUDElements(){
        if(!Application.isPlaying) return;
        this.HpSlider.value = this.healthPoints;
        this.HpSlider.maxValue = this.maxHealthPoints;

        this.ManaSlider.value = this.mana;
        this.ManaSlider.maxValue = this.maxMana;
    }

    private void UpdateArmorStats(){
        this.Armor.healthPoints = this.healthPoints;
        this.Armor.mana = this.mana;
    }

#region Getter - Setter
    public int GetAttackMin(){
        return this.Weapon.attackMin;
    }

    public int GetAttackMax(){
        return this.Weapon.attackMax;
    }

    public int GetLifeSteal(){
        return this.Weapon.lifeSteal;
    }

    public int GetActionsPerRound(){
        return this.Weapon.actionsPerRound;
    }

    public Weapon GetWeapon(){
        return this.Weapon;
    }

    public void SetWeapon(Weapon W){
        this.Weapon = W;
    }

    public Armor GetArmor(){
        return this.Armor;
    }

    public void SetArmor(Armor A){
        this.Armor = A;
        this.healthPoints = A.healthPoints;
        this.maxHealthPoints = A.maxHealthPoints;
        this.mana = A.mana;
        this.maxMana = A.maxMana;
    }

    public List<Action> GetAbilityList(){
        return this.Weapon.Abilities;
    }

    public List<Action> GetBlockAbilities(){
        return this.Weapon.Blocks;
    }
#endregion
}