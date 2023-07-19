using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Harpies : Weapon
{
    public override void SetWeaponType(){
        
        this.Type = WeaponType.DAGGER;
    }

    public override void InitAbilityList(){
        this.AbilityList = new List<Action>();

        this.AbilityList.Add(new Light());
        this.AbilityList.Add(new Heavy()); // replaced Element
        this.AbilityList.Add(new Special());
    }
    
    public override List<Action> GetCompleteMoveList(){
        List<Action> Actions_l = new List<Action>();

        Actions_l.Add(new Light());
        Actions_l.Add(new Heavy());
        Actions_l.Add(new Special());
        Actions_l.Add(new DoubleStab());
        Actions_l.Add(new ElementalInfuse());
        Actions_l.Add(new VitalStab());
        Actions_l.Add(new KnifeThrow());
        Actions_l.Add(new DualSurge());
        Actions_l.Add(new Seeker());
        Actions_l.Add(new Sprint());
        Actions_l.Add(new SoulStealer());
        Actions_l.Add(new HeartPiercer());
        Actions_l.Add(new HarpiesLifeEnder());

        return Actions_l;
    }

    public override Action CombineActions(string comboList, List<Action> Actions_l){
        Action res;
        switch(comboList){
            case "LL":
                res = new DoubleStab();
            break;

            case "HS":
                res = new ElementalInfuse();
            break;

            case "SL":
                res = new VitalStab();
            break;

            case "SS":
                res = new KnifeThrow();
            break;

            case "SLS":
                res = new DualSurge();
            break;

            case "LSL":
                res = new Seeker();
            break;

            case "LHL":
                res = new Sprint();
            break;

            case "HHLS":
                res = new SoulStealer();
            break;

            case "SLSL":
                res = new HeartPiercer();
            break;

            case "LHHHL":
                res = new HarpiesLifeEnder();
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

        //Actions_l.Add(new DoubleStab());
        //Actions_l.Add(new ElementalInfuse(new Element(SpellElement.FIRE)));
        //Actions_l.Add(new VitalStab());
        //Actions_l.Add(new KnifeThrow());
        //Actions_l.Add(new DualSurge());
        //Actions_l.Add(new Seeker());
        //Actions_l.Add(new Sprint(new Element(SpellElement.FIRE)));
        //Actions_l.Add(new SoulStealer(new Element(SpellElement.FIRE), new Element(SpellElement.FIRE)));
        //Actions_l.Add(new HeartPiercer());
        //Actions_l.Add(new HarpiesLifeEnder(new Element(SpellElement.FIRE), new Element(SpellElement.FIRE), new Element(SpellElement.FIRE)));

        return Actions_l;
    }

    private class DoubleStab : Action
    {
        public DoubleStab(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Double Stab";
            this.cover = "DbStb";

            this.comboLevel = 2;
            this.comboList = "LL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            
            this.damage = (int)Mathf.Round(this.baseDamage/3); // multihit
            this.baseDamage = this.damage;
            this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage/3);
            this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage/3);
            
            this.manaCost = 8;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            // this.StartEffects.Enqueue(new MultiHit(3, this));
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

    private class ElementalInfuse : Action
    {
        public ElementalInfuse(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Elemental Infuse";
            this.cover = "E-Inf";

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

    private class VitalStab : Action
    {
        public VitalStab(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Vital Stab";
            this.cover = "VitStb";

            this.comboLevel = 2;
            this.comboList = "SL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            this.manaCost = 9;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new BonusCritChance(10, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new VitalStab();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class KnifeThrow : Action
    {
        public KnifeThrow(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Knife Throw";
            this.cover = "KnThr";

            this.comboLevel = 2;
            this.comboList = "SS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();

            this.manaCost = 8;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new ExecuteDamage(0, this.baseDamage*3, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new KnifeThrow();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class DualSurge : Action
    {
        public DualSurge(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Dual Surge";
            this.cover = "DlSur";

            this.comboLevel = 3;
            this.comboList = "SLS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            this.manaCost = 21;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new ManaSteal(25, this));
            // this.StartEffects.Enqueue(new ApGainPlus(5, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new DualSurge();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class Seeker : Action
    {
        public Seeker(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Seeker";
            this.cover = "Seek";

            this.comboLevel = 3;
            this.comboList = "LSL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            this.manaCost = 23;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new Seeker_E(this)); // IMPORTANT!: unique effects at top
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Seeker();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class Seeker_E : UniqueEffect
        {
            private float increasf = 0.05f;
            public Seeker_E(Action A){
                this.Action = A;
                this.id = "Seek_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : permanent bonusCritDamageGain

                int damageDealt = await this.Action.NormalExecute();

                if(damageDealt > 0) this.Action.Player.Weapon.bonusCritDamagef += this.increasf;

                return damageDealt;
            }
        }
    } 

    private class Sprint : Action
    {
        public Sprint(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Sprint";
            this.cover = "Sprn";

            this.comboLevel = 3;
            this.comboList = "LHL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            this.damage = (int)Mathf.Round(this.baseDamage/4); // multihit
            this.baseDamage = this.damage;
            this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage/4);
            this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage/4);

            this.manaCost += 16;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            // this.StartEffects.Enqueue(new MultiHit(4, this));
            this.StartEffects.Enqueue(new BonusShieldDamage(1.4f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Sprint();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class SoulStealer : Action
    {
        public SoulStealer(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Soul Stealer";
            this.cover = "Soul";

            this.comboLevel = 4;
            this.comboList = "HHLS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            this.damage = (int)Mathf.Round(this.baseDamage/4);  // multihit
            this.baseDamage = this.damage;
            this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage/4);
            this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage/4);

            this.manaCost += 39;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new SoulStealer_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new ExtraElementalDamage(90, 0.6f, this));
            this.StartEffects.Enqueue(new BonusCritChance(15, this));
            // this.StartEffects.Enqueue(new MultiHit(4, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new SoulStealer();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class SoulStealer_E : UniqueEffect
        {
            public SoulStealer_E(Action A){
                this.Action = A;
                this.id = "Soul_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : deletes Enemy Element

                int damageDealt = await this.Action.NormalExecute();

                if(damageDealt > 0) this.Action.Enemy.element = SpellElement.none;

                return damageDealt;
            }
        }
    }

    private class HeartPiercer : Action
    {
        public HeartPiercer(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Heart Piercer";
            this.cover = "HrtP";

            this.comboLevel = 4;
            this.comboList = "SLSL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            this.manaCost += 32;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new MaxBaseDamage(this)); // NOTE: should be at the top everytime
            
            this.StartEffects.Enqueue(new HeartPiercer_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new ExecuteEffect(35, this));
            this.StartEffects.Enqueue(new ExecuteDamage(0,this.baseDamage*4, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new HeartPiercer();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class HeartPiercer_E : UniqueEffect
        {
            public HeartPiercer_E(Action A){
                this.Action = A;
                this.id = "HrtP_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : durability regain

                int d2wpnOrig = this.Action.Enemy.damageToWeapons;
                this.Action.Enemy.damageToWeapons = 0;

                int damageDealt = await this.Action.NormalExecute();

                this.Action.Enemy.damageToWeapons = d2wpnOrig;
                
                if(damageDealt > 0){
                    Weapon W = this.Action.Player.Weapon;
                    W.Repair((int)Mathf.Round((W.maxDurability - W.durability)/2));
                }

                return damageDealt;
            }
        }
    }

    private class HarpiesLifeEnder : Action
    {
        public HarpiesLifeEnder(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Harpies Life Ender";
            this.cover = "Harpy";

            this.comboLevel = 5;
            this.comboList = "LHHHL";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            
            this.manaCost += 50;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new HarpiesLifeEnder_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new BonusCritDamage(0.5f, this));
            this.StartEffects.Enqueue(new ExtraElementalDamage(555, 0.5f, this));
            this.StartEffects.Enqueue(new BonusCreditOnKill(4.0f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new HarpiesLifeEnder();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class HarpiesLifeEnder_E : UniqueEffect
        {
            private int damagePerBonus = 50; // bonus damage per 0.01f bonus crit damage

            public HarpiesLifeEnder_E(Action A){
                this.Action = A;
                this.id = "Harpy_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : resets bonuscritdamage for extra damage
                Weapon W = this.Action.Player.Weapon;
                float increaseC = (W.bonusCritDamagef - W.bonusCritDamagefbase)*100;
                this.Action.damage = (int)Mathf.Round(this.Action.damage+(increaseC*this.damagePerBonus));
                W.bonusCritDamagef = W.bonusCritDamagefbase;

                int damageDealt = await this.Action.NormalExecute();
                
                return damageDealt;
            }
        }
    }  
}