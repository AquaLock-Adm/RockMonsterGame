using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Karmina : Weapon
{
    public override void SetWeaponType(){
        
        this.Type = WeaponType.AXE;
    }

    public override void InitAbilityList(){
        this.AbilityList = new List<Action>();

        this.AbilityList.Add(new Light()); // Replaces Element
        this.AbilityList.Add(new Heavy());
        this.AbilityList.Add(new Special());
    }

    public override List<Action> GetCompleteMoveList(){
        List<Action> Actions_l = new List<Action>();
        
        Actions_l.Add(new Light());
        Actions_l.Add(new Heavy());
        Actions_l.Add(new Special());
        Actions_l.Add(new ElementalInfuse());
        Actions_l.Add(new BruteSlash());
        Actions_l.Add(new Swipe());
        Actions_l.Add(new Combustion());
        Actions_l.Add(new Stomp());
        Actions_l.Add(new AxeThrow());
        Actions_l.Add(new ManaSurge());
        Actions_l.Add(new BrainDeath());
        Actions_l.Add(new BarbariansRaid());
        Actions_l.Add(new KaminasDivider());

        return Actions_l;
    }

    public override Action CombineActions(string comboList, List<Action> Actions_l){
        Action res;
        switch(comboList){
            case "LH":
                res = new ElementalInfuse();
            break;

            case "HH":
                res = new BruteSlash();
            break;

            case "HS":
                res = new Swipe();
            break;

            case "LL":
                res = new Combustion();
            break;

            case "HHH":
                res = new Stomp();
            break;

            case "LHS":
                res = new AxeThrow();
            break;

            case "HSH":
                res = new ManaSurge();
            break;

            case "HHHH":
                res = new BrainDeath();
            break;

            case "LHSS":
                res = new BarbariansRaid();
            break;

            case "HHLSS":
                res = new KaminasDivider();
            break;

            default:
                Debug.Log("Combo Attack not found!");
                res = null;
            break;
        }
        return res;
    }

    public override List<Action> GetMovesToTest(){
        List<Action> Actions_l = new List<Action>();
        
        //Actions_l.Add(new ElementalInfuse(new Element(SpellElement.FIRE)));
        //Actions_l.Add(new BruteSlash());
        //Actions_l.Add(new Swipe());
        //Actions_l.Add(new Combustion(new Element(SpellElement.FIRE), new Element(SpellElement.FIRE)));
        //Actions_l.Add(new Stomp());
        //Actions_l.Add(new AxeThrow(new Element(SpellElement.FIRE)));
        //Actions_l.Add(new ManaSurge());
        //Actions_l.Add(new BrainDeath());
        //Actions_l.Add(new BarbariansRaid(new Element(SpellElement.FIRE)));
        //Actions_l.Add(new KaminasDivider(new Element(SpellElement.FIRE)));

        return Actions_l;
    }

    private class ElementalInfuse : Action
    {
        public ElementalInfuse(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Elemental Infuse";
            this.cover = "E-Inf";

            this.comboLevel = 2;
            this.comboList = "LH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            
            this.manaCost += 8;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new ElementalInfuse();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class BruteSlash : Action
    {
        public BruteSlash(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Brute Slash";
            this.cover = "Br-Sl";

            this.comboLevel = 2;
            this.comboList = "HH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            
            this.manaCost += 8;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            // this.StartEffects.Enqueue(new ApGainPlus(3, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new BruteSlash();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class Swipe : Action
    {
        public Swipe(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Swipe";
            this.cover = "Swp";

            this.comboLevel = 2;
            this.comboList = "HS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            
            this.manaCost += 8;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            // this.StartEffects.Enqueue(new ResetEnemyAttackTimer(this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Swipe();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class Combustion : Action
    {
        public Combustion(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Combustion";
            this.cover = "Comb";

            this.comboLevel = 2;
            this.comboList = "LL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            
            this.manaCost += 10;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = 0; // special case
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new ExtraElementalDamage(70, 0.3f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Combustion();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class Stomp : Action
    {
        public Stomp(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Stomp";
            this.cover = "Stmp";

            this.comboLevel = 3;
            this.comboList = "HHH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            
            this.manaCost += 15;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            // this.StartEffects.Enqueue(new ApGainPlus(4, this));
            this.StartEffects.Enqueue(new BonusShieldDamage(1.3f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Stomp();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class AxeThrow : Action
    {
        public AxeThrow(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Axe Throw";
            this.cover = "AxeTh";

            this.comboLevel = 3;
            this.comboList = "LHS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            
            this.manaCost += 16;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            // this.StartEffects.Enqueue(new ResetEnemyAttackTimer(this));
            this.StartEffects.Enqueue(new LowHpDamageBonus(0, this.baseDamage*3, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new AxeThrow();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class ManaSurge : Action
    {
        public ManaSurge(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Mana Surge";
            this.cover = "MaSur";

            this.comboLevel = 3;
            this.comboList = "HSH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            
            this.damage = (int)Mathf.Round(this.baseDamage/2); // aoe effect
            this.baseDamage = this.damage;
            this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage/2);
            this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage/2);
            
            this.manaCost += 14;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new ManaSteal(28, this));
            // this.StartEffects.Enqueue(new AOE(2, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new ManaSurge();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class BrainDeath : Action
    {
        public BrainDeath(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Brain Death";
            this.cover = "Brain";

            this.comboLevel = 4;
            this.comboList = "HHHH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            

            this.damage = (int)Mathf.Round(this.baseDamage/6); // dmg lowered because of execute effect
            this.baseDamage = damage;
            this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage/6);
            this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage/6);

            this.manaCost += 32;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            // this.StartEffects.Enqueue(new BrainDeath_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new BonusCritDamage(0.5f, this));
            this.StartEffects.Enqueue(new ExecuteEffect(20, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new BrainDeath();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        // private class BrainDeath_E : UniqueEffect
        // {
        //     private int attackCount = 4;
        //     private int critChanceIncrease = 15;
        //     private int accuracyDecrease = 10;

        //     public BrainDeath_E(Action A){
        //         this.Action = A;
        //         this.id = "Brain_E";
        //     }

        //     public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
        //         // define unique effect here!

        //         // Unique Effect : multiple hits, increasing crit chance and decreasing accuracy

        //         this.Action.CheckMovesUsedCount();
        //         if(!await this.Action.CheckMana()) return 0;
        //         this.Action.CheckDamageToWeapon();
        //         this.Action.CheckAbilityDecay();
        //         this.Action.PrintAction();
        //         this.Action.alreadyChecked = true;

        //         int damageDealt = 0;
        //         int origDamage = this.Action.damage;

        //         for(int attackn = 0; attackn < this.attackCount && this.Action.Enemy.healthPoints > 0; attackn++){
        //             this.Action.damage = origDamage;
        //             this.Action.accuracy = (int)Mathf.Clamp(this.Action.accuracy - accuracyDecrease, 0.0f, float.MaxValue);
        //             this.Action.crit += this.critChanceIncrease;
        //             this.Action.enemySet = true;

        //             damageDealt += await this.Action.NormalExecute();

        //         }

        //         return damageDealt;
        //     }
        // }
    }

    private class BarbariansRaid : Action
    {
        public BarbariansRaid(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Barbarians Raid";
            this.cover = "Barbar";

            this.comboLevel = 4;
            this.comboList = "LHSS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            

            this.manaCost += 30;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new BarbariansRaid_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new DropChanceOnKill(25, this));
            this.StartEffects.Enqueue(new BonusCreditOnKill(4.0f, this));
            this.StartEffects.Enqueue(new DecreasedDamageToWeapon(0.6f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new BarbariansRaid();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class BarbariansRaid_E : UniqueEffect
        {
            private int shieldGain = 25;
            private float damage2WeaponDecreasef = 0.5f;

            public BarbariansRaid_E(Action A){
                this.Action = A;
                this.id = "BarBar_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : decreased damage to durability + permanent shield gain(for this dungeon)

                int d2wpnOrig = this.Action.Enemy.damageToWeapons;
                this.Action.Enemy.damageToWeapons = (int)Mathf.Round(this.Action.Enemy.damageToWeapons*this.damage2WeaponDecreasef);

                int damageDealt = await this.Action.NormalExecute();

                this.Action.Enemy.damageToWeapons = d2wpnOrig;

                if(damageDealt > 0) this.Action.Player.Armor.shield += this.shieldGain;

                return damageDealt;
            }
        }
    }

    private class KaminasDivider : Action
    {
        public KaminasDivider(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Kaminas Divider";
            this.cover = "Kamina";

            this.comboLevel = 5;
            this.comboList = "HHLSS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            

            // no damage nerf for aoe, because ult and such

            this.manaCost += 50;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            // this.StartEffects.Enqueue(new KaminasDivider_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new LowHpDamageBonus(0, this.baseDamage*5, this));
            this.StartEffects.Enqueue(new BonusHpDamage(1.5f, this));
            this.StartEffects.Enqueue(new BonusCreditOnKill(3.0f, this));
            return;
        }

        public override async Task Passive(){

            await Task.Yield();
        }

        public override Action Copy(){
            Action A = new KaminasDivider();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        // private class KaminasDivider_E : UniqueEffect
        // {
        //     // private int aoec = 3;

        //     public KaminasDivider_E(Action A){
        //         this.Action = A;
        //         this.id = "Kamina_E";
        //     }

        //     public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions // TODO!
        //         // define unique effect here!

        //         // Unique Effect : aoe overflow damage
        //         int damageDealt = 0;
        //         this.Action.CheckMovesUsedCount();
        //         if(!await this.Action.CheckMana()) return 0;
        //         this.Action.CheckDamageToWeapon();
        //         this.Action.CheckAbilityDecay();

        //         if(!await this.Action.CheckMiss()) return 0;
        //         this.Action.CheckElementalWeakness();
        //         this.Action.CheckCrit();

        //         this.Action.PrintAction();
        //         this.Action.Player.DepleteMana(-this.Action.manaGain);
                
        //         this.Action.Enemy.killPrice += this.Action.bonusCreditOnKill;
        //         this.Action.Enemy.itemDropChance += this.Action.dropChanceIncrease;

        //         int damageShield = this.Action.damage;
        //         if(this.Action.Enemy.shield > 0) damageShield = (int)Mathf.Round(damageShield * this.Action.Enemy.attackIntoShieldMultiplier);

        //         damageDealt += await this.Action.Enemy.DealDamage(this.Action.damage);
        //         int overFlowDamage = damageShield - damageDealt;

        //         if(this.Action.Enemy.healthPoints <= 0){

        //             Action A = new Light();

        //             A.manaCost = 0;

        //             A.baseDamage = overFlowDamage;
        //             A.damage = A.baseDamage;
        //             A.minBaseDamage = A.baseDamage;
        //             A.maxBaseDamage = A.baseDamage;

        //             A.element = this.Action.element;

        //             A.alreadyChecked = true;

        //             // A.StartEffects.Enqueue(new AOE(this.aoec,A));
        //             A.StartEffects.Enqueue(new BonusCreditOnKill(3.0f,A));
        //             damageDealt += await A.NormalExecute();
        //         }

        //         return damageDealt;
        //     }
        // }
    }
}
