using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// avg0
public enum SpellElement {
    // level 1
    WATER, 
    AIR, 
    FIRE, 
    EARTH,
    // level 2
    REZA,
    ICE,
    PLODE,
    ROTA,
    // level 3
    ARCANUM,
    MENUM,
    GEOS,
    CHAOS,

    none
}

// ac0
public abstract class Action
{
    // avg1
    public BattleSystem BattleSystem;
    public GameHandler GameHandler;
    public PlayerCharacter Player;
    public Enemy Enemy;
    // avg1

    // avg2
    public string name = "";
    public string cover = "";
    // avg2

    // avg3
    public int abilityIndex = -1;

    public int totalTime = 1000; // <-- NBS

    public int knockBack = 100; // <-- NBS

    // avg5
    public Queue<Effect> StartEffects = new Queue<Effect>();

    // avg6
    public int comboLevel = 1;
    public float[] comboDamageMults = {1.0f, 1.75f, 2.35f, 2.65f, 3.0f};
    public string comboList = "";
    // avg6

    // avg7
    public SpellElement element = SpellElement.none;
    public List<Action> Elements = new List<Action>();
    public int elemWeaknessBonus = 0;
    public float elemWeaknessBonusFactor = 0.0f; // + x0 bonus damage
    // avg7

    // avg9
    public int manaCost = 0;
    public int manaGain = 0;
    // avg9

    // avg10
    public int damage = 0;
    public int baseDamage = 0;
    public int maxBaseDamage = 0;
    public int minBaseDamage = 0;
    public int bonusEffectDamage = 0;
    // avg10

    // avg11
    public int hpGain = 0;
    public int hpGainPercent = 0; // 0% life steal
    // avg11

    // avg12
    public int accuracy = 100;

    // avg13
    public int crit = 0;
    public float bonusCritFactor = 0.0f;
    // avg13

    // avg14
    public int damageToWeapon = 0;
    public bool damageToWeaponSet = false;
    // avg14

    // avg15
    public int bonusCreditOnKill = 0;
    public int dropChanceIncrease = 0;
    // avg15

    // avg16
    // public bool resetEnemyAttackTimer = false;

    // avg17
    public int executeThreshhold = 0; // executes under 0% maxhealth

    // avg18
    // public bool aoeON = false;
    // public int aoeUnitCount = 0;
    // avg18

    // avg19
    // public bool overFlowON = false;
    // public int overFlowDamage = 0;
    // avg19

    // avg20
    // public bool multiHitON = false;
    // public int multiHitc = 0; // count of multi hits
    // avg20

    // avg21
    public bool uniqueEffect = false;
    public UniqueEffect UniqueEffect;
    // avg21

    // avg22
    public bool alreadyChecked = false;
    public bool enemySet = false;
    public bool executedCrit = false;
    // avg22

    // avg23
    private bool stopPermanent = false;
    private bool pausePermanent = false;
    // avg23

    // avg24
    private bool useGameHandler = true;
    public bool printThisAction = false;

    public abstract void Setup();
    public abstract void SetDamageToWeapon();
    public abstract void SetEffects();
    public abstract Action Copy();
    public abstract Task Passive();

#region SetUp Functions
    public void SetGameHandler(){

        this.GameHandler = GameObject.Find("Game Handler").GetComponent<GameHandler>();
        this.BattleSystem = this.GameHandler.BattleSystem;
    }
    public void SetBattleSystem(BattleSystem BS){

        this.BattleSystem = BS;
    }
    public async Task SetBattleUnits(){
        this.Player = this.BattleSystem.Player;

        while(this.BattleSystem.Enemy == null) await Task.Yield();
        
        this.Enemy = this.BattleSystem.Enemy;
    }
    public void BaseDamageCalculation(){
        if(this.GameHandler != null) this.useGameHandler = true;
        else this.useGameHandler = false;

        int elementsI = 0;
        for(int stringI = 0; stringI < this.comboList.Length; stringI++){
            switch(this.comboList[stringI]){
                case 'L':
                    if(this.useGameHandler) this.baseDamage += this.GameHandler.CalcUnitDamage(this.Player);
                    else this.baseDamage += this.BattleSystem.CalcUnitDamage(this.Player);
                    this.maxBaseDamage += this.Player.attackMax;
                    this.minBaseDamage += this.Player.attackMin;
                break;

                case 'H':
                    float hAttackMult; 
                    if(this.useGameHandler) {
                        hAttackMult = this.GameHandler.heavyAttackMultiplier;
                        this.baseDamage += (int)Mathf.Round(this.GameHandler.CalcUnitDamage(this.Player)*hAttackMult);
                    }
                    else {
                        hAttackMult = this.BattleSystem.heavyAttackMultiplier;
                        this.baseDamage += (int)Mathf.Round(this.BattleSystem.CalcUnitDamage(this.Player)*hAttackMult);
                    }
                    this.maxBaseDamage += (int)Mathf.Round(this.Player.attackMax * hAttackMult);
                    this.minBaseDamage += (int)Mathf.Round(this.Player.attackMin * hAttackMult);
                break;

                case 'S':
                    float sAttackMult; 
                    if(this.useGameHandler) {
                        sAttackMult = this.GameHandler.specialAttackMultiplier;
                        this.baseDamage += (int)Mathf.Round(this.GameHandler.CalcUnitDamage(this.Player)*sAttackMult);
                    }
                    else {
                        sAttackMult = this.BattleSystem.specialAttackMultiplier;
                        this.baseDamage += (int)Mathf.Round(this.BattleSystem.CalcUnitDamage(this.Player)*sAttackMult);
                    }
                    this.maxBaseDamage += (int)Mathf.Round(this.Player.attackMax * sAttackMult);
                    this.minBaseDamage += (int)Mathf.Round(this.Player.attackMin * sAttackMult);
                break;

                case 'E':
                    if(this.useGameHandler){
                        if(elementsI < this.Elements.Count){
                            this.baseDamage += this.Elements[elementsI].baseDamage;
                            this.maxBaseDamage += this.Elements[elementsI].baseDamage;
                            this.minBaseDamage += this.Elements[elementsI].baseDamage;
                            elementsI++;
                        }else Debug.Log("Error: Not enough elements given!");
                    }else Debug.LogError("NOT IMPLEMENTED YET");
                break;

                default:
                    Debug.Log("ERROR");
                break;
            }
        }

        this.baseDamage = (int)Mathf.Round(this.baseDamage * this.comboDamageMults[this.comboLevel-1]);
        this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage * this.comboDamageMults[this.comboLevel-1]);
        this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage * this.comboDamageMults[this.comboLevel-1]);

        this.damage = this.baseDamage;
    }
    public void SetAccuracy(){
        
        this.accuracy = this.Player.Weapon.GetComboAccuracy(this.comboLevel);
    }
#endregion

    public async Task<int> NormalExecute(){
        this.SetDamageToWeapon();

        this.ActivateEffects(this.StartEffects);

        // if(this.uniqueEffect && this.UniqueEffect != null) {
        //     this.uniqueEffect = false;
        //     this.damage = this.baseDamage; // reset damage so that repeated effects dont stack damage (i.e. BonusShieldDamage)
        //     return await this.UniqueEffect.ActivateUniqueEffect();
        // }

        this.CheckMovesUsedCount();
        this.CheckDamageToWeapon();
        this.CheckAbilityDecay();
        this.CheckElementalWeakness();
        this.CheckCrit();

        this.CheckMana(); // para to wait time
        await Task.Delay(this.totalTime);

        this.CheckMiss();

        if(this.damage <= 0) {
            this.Enemy.MissedAttack();
            this.MissedKillEffects();
        }else {
            this.ExecuteAttack();
        }

        this.executedCrit = false;

        return this.damage;
    }

#region Effect Functions
    private void ActivateEffects(Queue<Effect> EffectQ){
        int effectsCount = EffectQ.Count;
        for(int activatedCount = 0; activatedCount < effectsCount; activatedCount++){
            Effect E = EffectQ.Dequeue();
            Debug.Log(E.id + " activated");
            E.ActivateEffect();
        }
        //Debug.Log("Activated "+effectsCount.ToString()+" effects on action");
    }
    private void MissedKillEffects(){
        this.Enemy.killPrice -= this.bonusCreditOnKill;
        this.Enemy.itemDropChance -= this.dropChanceIncrease;
    }
#endregion


#region Normal Execute Check Functions
    public void CheckMovesUsedCount(){

        this.BattleSystem.movesUsedCount++;
    }
    public void CheckMana(){
        // Set variable Mana deplete time here
        this.Player.DepleteMana(this.manaCost);

        if(this.manaCost > this.Player.mana){
            this.damage = 0;
        }
    }
    public void CheckDamageToWeapon(){

        this.Player.Weapon.DamageToDurability(this.damageToWeapon);
    }
    public void CheckAbilityDecay(){

        this.damage = this.BattleSystem.AbilityDecay(this);//(this.damage, this.abilityIndex);
    }
    public void CheckMiss(){
        if(!this.BattleSystem.CheckForHit(this.accuracy)){
            Debug.Log("Miss");
            this.damage = 0;
        }
    }
    public void CheckElementalWeakness(){
        int weakness = this.BattleSystem.ElementWeakTo(this.element, this.Enemy.element);
        // returns 1 if less damage
        // returns 0 if neutral
        // returns -1 if more damage

        float cElemWeakf = 1.0f;
        float cElemWeakBonusf = 0.0f;
        int cElemBonus = 0;

        if(weakness == 1) {
            cElemWeakf = this.BattleSystem.spellWeakMultiplier;
            cElemBonus = -this.elemWeaknessBonus;
        }

        if(weakness == -1) {
            cElemWeakf = this.BattleSystem.spellStrongMultiplier;
            cElemWeakBonusf = this.elemWeaknessBonusFactor;
            cElemBonus = this.elemWeaknessBonus;
        }

        this.damage = (int)Mathf.Round(this.damage*(cElemWeakf+cElemWeakBonusf));
        this.damage += cElemBonus;
    }
    public void CheckCrit(){
        if(this.BattleSystem.CheckForHit(this.crit)){
            Debug.Log("Crit");
            this.damage = (int)Mathf.Round(this.damage*(this.BattleSystem.critMultiplier+this.bonusCritFactor+this.Player.Weapon.bonusCritDamagef));
            this.Enemy.Crit();
            this.executedCrit = true;
            //newAction.textColor = playerCritColor;
        }
    }
#endregion

#region Execute Attack Functions
    public async void ExecuteAttack(){
        int damageDealt = 0;

        if(CheckExecuteThreshhold()) {
            // PrintAction();
            GetManaGain();
            SetExtraKillBoni();

            damageDealt += await this.Enemy.ExecuteUnit();
            this.bonusEffectDamage = 0;
        }

        else {
            // PrintAction();
            GetManaGain();
            SetExtraKillBoni();
            this.Enemy.KnockBack(this.knockBack);
            damageDealt += await this.Enemy.DealDamage(this.damage);

            if(this.Enemy.healthPoints > 0) MissedKillEffects();

            this.bonusEffectDamage = 0;
        }
    }
    private bool CheckExecuteThreshhold(){
        if(!ExecuteThreshholdSet()) return false;
        int EnemyHpP = (int)Mathf.Round(((float)this.Enemy.healthPoints/this.Enemy.maxHealthPoints) * 100);

        if(EnemyHpP <= this.executeThreshhold) return true;
        else return false;
    }
    private bool ExecuteThreshholdSet(){

        return this.executeThreshhold > 0;
    }
    private void GetManaGain(){
        this.Player.DepleteMana(-this.manaGain);
        this.manaGain = 0;
    }
    private void SetExtraKillBoni(){
        // get removed in MissedKillEffects() should the Attack not kill
        this.Enemy.killPrice += this.bonusCreditOnKill;
        this.Enemy.itemDropChance += this.dropChanceIncrease;
    }
#endregion

    public void StopPermanentEffect(){

        this.stopPermanent = true;
    }

    public void PausePermanentEffect(bool on){

        this.pausePermanent = on;
    }

    public async void PermanentEffect(){
        while(!stopPermanent){
            if(!pausePermanent){
                await Passive();
            }else await Task.Yield();
        }
    }

    public void PrintAction(){
        string s = "------------------------------\n"+
                    "Name: "+this.name+"\n"+
                    "Cover: "+this.cover+"\n"+
                    "Ability Index: "+this.abilityIndex.ToString()+"\n\n";

        if(this.BattleSystem != null) s += "BattleSystem: Set\n";
        else s += "BattleSystem: Not Set\n";

        if(this.Player != null) s += "Player: "+this.Player.unitName+"\n";
        else s += "Player: Not Set\n";

        if(this.Enemy != null) s += "Enemy: "+this.Enemy.unitName+"\n\n";
        else s += "Enemy: Not Set\n\n";

        s +=      "Combo Level: "+this.comboLevel.ToString()+"\n"+
                    "Combo List: "+this.comboList+"\n\n"+

                    "Mana Cost: -"+this.manaCost.ToString()+"\n"+
                    "Mana Gain: +"+this.manaGain.ToString()+"\n\n"+

                    "Damage = (Base * (elemWeakf + elemWeakbonusf) + elemBonus + BonusEffectDmg) * (critf + bonusCritf + Weapon.bonusCritf)\n"+
                    "Base: "+this.minBaseDamage.ToString()+" <-> "+this.maxBaseDamage.ToString()+"\n"+
                    "Current Base: "+this.baseDamage.ToString()+"\n";

                    float cElemWeakf = 1.0f;
                    if(this.element != SpellElement.none) cElemWeakf = this.GameHandler.spellStrongMultiplier;
                    s += "Max Damage = "+((int)Mathf.Round(
                        (this.maxBaseDamage*(cElemWeakf+this.elemWeaknessBonusFactor)+this.elemWeaknessBonus+this.bonusEffectDamage)
                        *(this.GameHandler.critMultiplier+this.bonusCritFactor+this.Player.Weapon.bonusCritDamagef))).ToString() +"\n";

                    cElemWeakf = 1.0f;
                    float cElemWeakBonusf = 0.0f;
                    int cElemBonus = 0;

                    int weakness = this.BattleSystem.ElementWeakTo(this.element, this.Enemy.element);
                    if(weakness == 1)  {
                        cElemWeakf = this.GameHandler.spellWeakMultiplier;
                        cElemBonus = -this.elemWeaknessBonus;
                    }
                    if(weakness == -1) {
                        cElemWeakf = this.GameHandler.spellStrongMultiplier;
                        cElemWeakBonusf = this.elemWeaknessBonusFactor;
                        cElemBonus = this.elemWeaknessBonus;
                    }

                    float cCritf = 1.0f;
                    float cBonusCritf = 0.0f;
                    float cWeaponCritf = 0.0f;

                    if(this.executedCrit){
                        this.executedCrit = false;
                        cCritf = this.GameHandler.critMultiplier;
                        cBonusCritf = this.bonusCritFactor;
                        cWeaponCritf = this.Player.Weapon.bonusCritDamagef;
                    }

                    s += "Current Damage = ("+this.baseDamage.ToString()+" * ("+cElemWeakf.ToString()+" + "+cElemWeakBonusf.ToString()+") + "+cElemBonus.ToString()+" + "+
                                    this.bonusEffectDamage.ToString()+") * ("+cCritf.ToString()+" + "+cBonusCritf.ToString()+" + "+cWeaponCritf.ToString()+")\n"+
                    
                    "Current Damage: "+this.damage.ToString()+"\n"+
                    "Effect Bonus Damage: "+this.bonusEffectDamage.ToString()+"\n"+
                    "Execute Threshhold: <="+this.executeThreshhold.ToString()+"%\n\n"+

                    "Element: "+this.element.ToString().ToLower()+"\n"+
                    "ElemWeakbonusf: x"+this.elemWeaknessBonusFactor.ToString()+"\n"+
                    "ElemBonusDmg: "+this.elemWeaknessBonus.ToString()+"\n\n"+

                    "Accuracy: "+this.accuracy.ToString()+"%\n"+
                    "Crit Chance: "+this.crit.ToString()+"%\n"+
                    "Bonus Crit Damagef: x"+this.bonusCritFactor.ToString()+"\n"+
                    "Weapon Bonus Crit Damagef: x"+this.Player.Weapon.bonusCritDamagef.ToString()+"\n\n"+

                    "Damage To Weapons: "+this.damageToWeapon.ToString()+"\n\n"+

                    "Bonus Credit: "+this.bonusCreditOnKill.ToString()+"cd\n"+
                    "Bonus Drop Chance: "+this.dropChanceIncrease.ToString()+"%\n\n"+
                    "Effects: "+GetListOfStartEffects()+"\n\n"+
                    "------------------------------\n";

        Debug.Log(s);
    }

    public override string ToString(){
        
        return this.name;
    }

    private string GetListOfStartEffects(){
        string s = "";
        foreach(Effect E in this.StartEffects) s += E.ToString()+" ";
        return s;
    }
}// Action Class

// ac1
public class Light : Action
{
    public Light(){
        this.name = "Light Attack";
        this.cover = "Light";
        this.comboLevel = 1;
        this.totalTime = 300;
        this.knockBack = 20;
        this.comboList = "L";
        this.Setup();
        this.StopPermanentEffect();
    }

    public override void Setup(){
        this.SetGameHandler();
        this.Player = this.GameHandler.Player;
        this.BaseDamageCalculation();

        this.manaCost = 0;
        this.crit = this.Player.crit;
        this.SetAccuracy();

        this.SetEffects();
    }

    public override void SetDamageToWeapon(){

        this.damageToWeapon = this.Enemy.damageToWeapons;
    }

    public override void SetEffects(){
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new Light();
        A.abilityIndex = this.abilityIndex;
        return A;
    } 
}

// ac2
public class Heavy : Action
{
    public Heavy(){
        this.name = "Heavy Attack";
        this.cover = "Heavy";
        this.comboLevel = 1;
        this.comboList = "H";

        this.Setup();
        this.StopPermanentEffect();
    }

    public override void Setup(){
        this.SetGameHandler();
        this.Player = this.GameHandler.Player;
        this.BaseDamageCalculation();

        this.manaCost = 0;
        this.crit = this.Player.crit;
        this.SetAccuracy();

        this.SetEffects();
    }

    public override void SetDamageToWeapon(){
        this.damageToWeapon = this.Enemy.damageToWeapons;
    }

    public override void SetEffects(){
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new Heavy();
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

// ac3
public class Special : Action
{
    public Special(){
        this.name = "Special Attack";
        this.cover = "Special";
        this.comboLevel = 1;
        this.comboList = "S";

        this.Setup();
        this.StopPermanentEffect();
    }

    public override void Setup(){
        this.SetGameHandler();
        this.Player = this.GameHandler.Player;
        this.BaseDamageCalculation();

        this.manaCost = 0;
        this.crit = this.Player.crit;
        this.SetAccuracy();

        this.SetEffects();
    }

    public override void SetDamageToWeapon(){
        this.damageToWeapon = this.Enemy.damageToWeapons;
    }

    public override void SetEffects(){
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new Special();
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

// acX
public class TestCombo : Action
{
    public TestCombo(){
        this.SetGameHandler();
        this.Player = this.GameHandler.Player;
        this.name = "Test Combo";
        this.cover = "Test";

        this.comboLevel = 1;
        this.comboList = "";

        this.Setup();
        this.StopPermanentEffect();
    }

    public override void Setup(){

        this.BaseDamageCalculation();

        this.manaCost = 0;
        this.crit = this.Player.crit;
        this.SetAccuracy();

        this.SetEffects();
    }

    public override void SetDamageToWeapon(){
        this.damageToWeapon = this.Enemy.damageToWeapons;
    }

    public override void SetEffects(){
        this.StartEffects.Enqueue(new Test_E(this)); // IMPORTANT!: unique effects at top
        /*
        //this.StartEffects.Enqueue(new MaxBaseDamage(this));
        //this.StartEffects.Enqueue(new AOE(1, this)); 
        //this.StartEffects.Enqueue(new MultiHit(1, this)); 
        //this.StartEffects.Enqueue(new OverFlow(this));  
        //this.StartEffects.Enqueue(new ApGainPlus(0, this)); 
        //this.StartEffects.Enqueue(new ManaSteal(0, this)); 
        //this.StartEffects.Enqueue(new BonusShieldDamage(1.0f, this)); 
        //this.StartEffects.Enqueue(new BonusHpDamage(1.0f, this)); 
        //this.StartEffects.Enqueue(new BonusCreditOnKill(1.0f, this)); 
        //this.StartEffects.Enqueue(new ExecuteDamage(0, 0, this)); 
        //this.StartEffects.Enqueue(new ExtraElementalDamage(0, 0.0f, this)); 
        //this.StartEffects.Enqueue(new ResetEnemyAttackTimer(this)); 
        //this.StartEffects.Enqueue(new DecreasedDamageToWeapon(1.0f, this)); 
        //this.StartEffects.Enqueue(new DropChanceOnKill(0, this)); 
        //this.StartEffects.Enqueue(new BonusCritChance(0, this)); 
        //this.StartEffects.Enqueue(new BonusAccuracy(0, this)); 
        //this.StartEffects.Enqueue(new BonusCritDamage(0.0f, this)); 
        //this.StartEffects.Enqueue(new ExecuteEffect(0, this)); 
        //this.StartEffects.Enqueue(new LowHpDamageBonus(0, 0, this)); 
        */
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new TestCombo();
        A.abilityIndex = this.abilityIndex;
        return A;
    }
    // acX_cX
    private class Test_E : UniqueEffect
    {
        public Test_E(Action A){
            this.Action = A;
            this.id = "Test_E";
        }

        public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
            // define unique effect here!

            // Unique Effect : testing all given actions

            Debug.Log("Testing Abilities...");
            this.Action.Enemy.damageTesting = true;
            //this.Action.Enemy.healthPoints = 10000;
            //this.Action.Enemy.element = SpellElement.EARTH;
            this.Action.BattleSystem.disableAbilityDecay = true;

            List<Action> Actions = this.Action.BattleSystem.TestAbilities;

            Debug.Log("Be sure that every Ability that uses an element has an element assigned to it in creation!");
            Debug.Log("Starting Tests");

            foreach(Action A in Actions){
                //A.accuracy = 100;
                int damageDealt = await A.NormalExecute();
                Debug.Log("Damage Dealt: "+ damageDealt.ToString());

                //A.BattleSystem.Player.currentActionPoints = A.BattleSystem.maxAp;
                //A.BattleSystem.Player.mana = A.BattleSystem.Player.maxMana;
            }

            this.Action.Enemy.damageTesting = false;
            this.Action.BattleSystem.disableAbilityDecay = false;
            Debug.Log("Finished");
            return 0;
        }
    }
}

public class PlaceHolderAction : Action
{
    public PlaceHolderAction(int comboLevel){
        this.comboLevel = comboLevel;
        this.StopPermanentEffect();
    }

    public override void Setup(){
        Debug.LogError("No Setup Function for Placeholder Actions");
    }

    public override void SetDamageToWeapon(){
        Debug.LogError("No SetDamageToWeapon Function for Placeholder Actions");
    }

    public override void SetEffects(){
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new PlaceHolderAction(this.comboLevel);
        A.abilityIndex = this.abilityIndex;
        return A;
    } 
}