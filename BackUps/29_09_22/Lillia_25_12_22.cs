using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Lillia : Weapon
{
    private int scytheBonusManaCost = 7;
    
    public override void SetWeaponType(){
        
        this.Type = WeaponType.SCYTHE;
    }

    public override void InitAbilityList(){
        this.AbilityList = new List<Action>();

        this.AbilityList.Add(new Light());
        this.AbilityList.Add(new Heavy());
        this.AbilityList.Add(new Special());
    }

    public override List<Action> GetCompleteMoveList(){
        List<Action> Actions_l = new List<Action>();

        Actions_l.Add(new Light());
        Actions_l.Add(new Heavy());
        Actions_l.Add(new Special());
        Actions_l.Add(new Pull());
        Actions_l.Add(new Swirl());
        Actions_l.Add(new SpinJump());
        Actions_l.Add(new WideSwipe());
        Actions_l.Add(new Launch());
        Actions_l.Add(new ChargeSlash());
        Actions_l.Add(new FurySlash());
        Actions_l.Add(new TheDecapitator());
        Actions_l.Add(new DeadMansGambit());
        Actions_l.Add(new LilliasFlourish());

        foreach(Action A in Actions_l) A.manaCost += scytheBonusManaCost;

        return Actions_l;
    }

    public override Action CombineActions(string comboList, List<Action> Actions_l){
        Action res;
        switch(comboList){
            case "LS":
                res = new Pull();
            break;

            case "LL":
                res = new Swirl();
            break;

            case "SS":
                res = new SpinJump();
            break;

            case "HH":
                res = new WideSwipe();
            break;

            case "LHS":
                res = new Launch();
            break;

            case "SLL":
                res = new ChargeSlash();
            break;

            case "LLL":
                res = new FurySlash();
            break;

            case "HSSH":
                res = new TheDecapitator();
            break;

            case "HLLS":
                res = new DeadMansGambit();
            break;

            case "SLSHH":
                res = new LilliasFlourish();
            break;

            default:
                Debug.Log("Combo Attack not found!");
                res = null;
            break;
        }
        if(res != null) res.manaCost += this.scytheBonusManaCost;
        return res;
    }

    public override List<Action> GetMovesToTest(){
        List<Action> Actions_l = new List<Action>();

        //Actions_l.Add(new Pull());
        //Actions_l.Add(new Swirl());
        //Actions_l.Add(new SpinJump());
        //Actions_l.Add(new WideSwipe());
        //Actions_l.Add(new Launch());
        //Actions_l.Add(new ChargeSlash());
        //Actions_l.Add(new FurySlash());
        //Actions_l.Add(new TheDecapitator());
        //Actions_l.Add(new DeadMansGambit());
        //Actions_l.Add(new LilliasFlourish());

        foreach(Action A in Actions_l) {
            A.manaCost += scytheBonusManaCost;
            A.element = SpellElement.FIRE;
        }

        return Actions_l;
    }

    private class Pull : Action
    {
        public Pull(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Pull";
            this.cover = "Pll";

            this.comboLevel = 2;
            this.combinationCost = 2;
            this.comboList = "LS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            this.BaseApCalculation();

            this.manaCost += 8;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new ManaSteal(23, this));
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Pull();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class Swirl : Action
    {
        public Swirl(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Swirl";
            this.cover = "Swrl";

            this.comboLevel = 2;
            this.combinationCost = 2;
            this.comboList = "LL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            this.BaseApCalculation();
            
            this.manaCost += 12;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new ApGainPlus(4, this));
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Swirl();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class SpinJump : Action
    {
        public SpinJump(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Spin Jump";
            this.cover = "SpJmp";

            this.comboLevel = 2;
            this.combinationCost = 5;
            this.comboList = "SS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            this.BaseApCalculation();
            
            this.manaCost += 11;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new DecreasedDamageToWeapon(0.5f, this));
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new SpinJump();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class WideSwipe : Action
    {
        public WideSwipe(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Wide Swipe";
            this.cover = "WdSw";

            this.comboLevel = 2;
            this.combinationCost = 3;
            this.comboList = "HH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            this.BaseApCalculation();
            
            this.manaCost += 18;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new AOE(2, this));
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new WideSwipe();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class Launch : Action
    {
        public Launch(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Launch";
            this.cover = "Lnch";

            this.comboLevel = 3;
            this.combinationCost = 6;
            this.comboList = "LHS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            this.BaseApCalculation();
            
            this.manaCost += 25;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new ResetEnemyAttackTimer(this));
            this.StartEffects.Enqueue(new BonusHpDamage(1.5f, this));
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Launch();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class ChargeSlash : Action
    {
        public ChargeSlash(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Charge Slash";
            this.cover = "ChSl";

            this.comboLevel = 3;
            this.combinationCost = 6;
            this.comboList = "SLL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            this.BaseApCalculation();
            
            this.manaCost += 20;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new MaxBaseDamage(this));
            this.StartEffects.Enqueue(new BonusAccuracy(10, this));
        }

        public override async Task Passive(){
            await Task.Yield();
        }  

        public override Action Copy(){
            Action A =  new ChargeSlash();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class FurySlash : Action
    {
        public FurySlash(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Fury Slash";
            this.cover = "FrSl";

            this.comboLevel = 3;
            this.combinationCost = 10;
            this.comboList = "LLL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            this.BaseApCalculation();
            
            this.damage = (int)Mathf.Round(this.baseDamage/4); // multihit effect
            this.baseDamage = this.damage;
            this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage/4);
            this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage/4);

            this.manaCost += 22;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new MultiHit(4, this));
            this.StartEffects.Enqueue(new BonusCritChance(10, this));
        }

        public override async Task Passive(){
            await Task.Yield();
        }  

        public override Action Copy(){
            Action A =  new FurySlash();
            A.abilityIndex = this.abilityIndex;
            return A;
        } 
    }

    private class TheDecapitator : Action
    {
        public TheDecapitator(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "The Decapitator";
            this.cover = "DeCap";

            this.comboLevel = 4;
            this.combinationCost = 10;
            this.comboList = "HSSH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            this.BaseApCalculation();
            
            this.damage = (int)Mathf.Round(this.baseDamage/2); // dmg reduce because of execute effect
            this.baseDamage = damage;
            this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage/2);
            this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage/2);

            this.manaCost += 36;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new TheDecapitator_E(this)); // IMPORTANT!: unique effects at top
            
            this.StartEffects.Enqueue(new ExecuteEffect(39, this));
            this.StartEffects.Enqueue(new BonusCreditOnKill(3.3f, this));
            this.StartEffects.Enqueue(new ManaSteal(62, this));
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new TheDecapitator();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class TheDecapitator_E : UniqueEffect
        {
            private int manaLoss = 40;
            public TheDecapitator_E(Action A){
                this.Action = A;
                this.id = "DeCap_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : Manaloss on miss or failed execute

                int damageDealt = await this.Action.NormalExecute();

                if(damageDealt <= 0) {
                    Debug.Log("Failed Execute dd: "+ damageDealt.ToString());
                    await this.Action.Player.DepleteMana(this.manaLoss);}
                else if(this.Action.Enemy.healthPoints > 0) {
                    Debug.Log("Failed Execute enemy alive: "+ this.Action.Enemy.healthPoints.ToString());
                    await this.Action.Player.DepleteMana(this.manaLoss);}

                return damageDealt;
            }
        }  
    }

    private class DeadMansGambit : Action
    {
        public DeadMansGambit(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Dead Mans Gambit";
            this.cover = "DMG";

            this.comboLevel = 4;
            this.combinationCost = 9;
            this.comboList = "HLLS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            this.BaseApCalculation();
            
            this.manaCost += 33;
            this.crit = this.Player.crit;


            this.SetAccuracy();
            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new DeadMansGambit_E(this)); // IMPORTANT!: unique effects at top
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new DeadMansGambit();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class DeadMansGambit_E : UniqueEffect
        {
            private float healthLossf = 0.25f;
            private float damageIncreasef = 2.0f;
            private int accuracy = 66;

            public DeadMansGambit_E(Action A){
                this.Action = A;
                this.id = "DMG_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : massive damage but decreased accuracy and self harm on miss

                if(this.Action.accuracy > this.accuracy) this.Action.accuracy = this.accuracy;
                
                this.Action.damage = (int)Mathf.Round(this.Action.damage * this.damageIncreasef);

                int damageDealt = await this.Action.NormalExecute();

                if(damageDealt <= 0) await this.Action.Player.DealDamage((int)Mathf.Round(this.Action.damage*this.healthLossf));

                return damageDealt;
            }
        }
    }

    private class LilliasFlourish : Action
    {
        public LilliasFlourish(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Lillias Flourish";
            this.cover = "Lillia";

            this.comboLevel = 5;
            this.combinationCost = 14;
            this.comboList = "SLSHH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            this.BaseApCalculation();
            
            this.damage = (int)Mathf.Round(this.baseDamage/4); // multihit
            this.baseDamage = this.damage;
            this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage/4);
            this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage/4);

            this.manaCost += 50;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new LilliasFlourish_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new AOE(4, this));
            this.StartEffects.Enqueue(new MultiHit(4, this));
            this.StartEffects.Enqueue(new BonusCreditOnKill(2.5f, this));
            return;
        }

        public override async Task Passive(){

            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new LilliasFlourish();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class LilliasFlourish_E : UniqueEffect
        {
            private float damage2WeaponIncreasef = 3.5f;
            private float lifeStealf = 0.25f;

            public LilliasFlourish_E(Action A){
                this.Action = A;
                this.id = "Lillia_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions // TODO!
                // define unique effect here!

                // Unique Effect : % of dmg dealt gained as hp + massive durability damage

                int d2wpnOrig = this.Action.Enemy.damageToWeapons;
                this.Action.Enemy.damageToWeapons = (int)Mathf.Round(this.Action.Enemy.damageToWeapons*this.damage2WeaponIncreasef);

                int damageDealt = await this.Action.NormalExecute();

                this.Action.Enemy.damageToWeapons = d2wpnOrig;

                // heal for damageDealt
                int heal = (int)Mathf.Round(damageDealt * this.lifeStealf);
                Debug.Log("Flourish Heal: "+heal.ToString());
                this.Action.Player.Armor.Repair(heal);

                return damageDealt;
            }
        }
    }
}