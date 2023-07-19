using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Maria : Weapon
{
    public override void SetWeaponType(){
        
        this.Type = WeaponType.SWORD;
    }

    public override void InitAbilityList(){
        this.AbilityList = new List<Action>();

        this.AbilityList.Add(new Light());
        this.AbilityList.Add(new Heavy());
        this.AbilityList.Add(new Special()); // replaced Element
    }

    public override List<Action> GetCompleteMoveList(){
        List<Action> Actions_l = new List<Action>();

        Actions_l.Add(new Light());
        Actions_l.Add(new Heavy());
        Actions_l.Add(new Special());
        Actions_l.Add(new ShieldBreak());
        Actions_l.Add(new ElementalInfuse());
        Actions_l.Add(new JumpAttack());
        Actions_l.Add(new Execute());
        Actions_l.Add(new SpinAttack());
        Actions_l.Add(new Lunge());
        Actions_l.Add(new MagicBoom());
        Actions_l.Add(new WyvernSlayer());
        Actions_l.Add(new PaladinsBane());
        Actions_l.Add(new MariasWrath());

        return Actions_l;
    }

    public override Action CombineActions(string comboList, List<Action> Actions_l){
        Action res;
        switch(comboList){
            case "LH":
                res = new ShieldBreak();
            break;

            case "SL":
                res = new ElementalInfuse();
            break;

            case "HL":
                res = new JumpAttack();
            break;

            case "HH":
                res = new Execute();
            break;

            case "HLL":
                res = new SpinAttack();
            break;

            case "LSH":
                res = new Lunge();
            break;

            case "HSS":
                res = new MagicBoom();
            break;

            case "SSSH":
                res = new WyvernSlayer();
            break;

            case "LHLH":
                res = new PaladinsBane();
            break;

            case "HLSSH":
                res = new MariasWrath();
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

        //Actions_l.Add(new ShieldBreak());
        //Actions_l.Add(new ElementalInfuse(new Element(SpellElement.FIRE)));
        //Actions_l.Add(new JumpAttack());
        //Actions_l.Add(new Execute());
        //Actions_l.Add(new SpinAttack());
        //Actions_l.Add(new Lunge(new Element(SpellElement.FIRE)));
        //Actions_l.Add(new MagicBoom(new Element(SpellElement.FIRE), new Element(SpellElement.FIRE)));
        //Actions_l.Add(new WyvernSlayer(new Element(SpellElement.FIRE), new Element(SpellElement.FIRE), new Element(SpellElement.FIRE)));
        //Actions_l.Add(new PaladinsBane());
        //Actions_l.Add(new MariasWrath(new Element(SpellElement.FIRE), new Element(SpellElement.FIRE)));

        return Actions_l;
    }

    private class ShieldBreak : Action
    {
        public ShieldBreak(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Shield Break";
            this.cover = "ShBreak";

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
            this.StartEffects.Enqueue(new BonusShieldDamage(1.3f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new ShieldBreak();
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
            this.comboList = "SL";

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

    private class JumpAttack : Action
    {
        public JumpAttack(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Jump Attack";
            this.cover = "J-Atk";

            this.comboLevel = 2;
            this.comboList = "HL";

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
            this.StartEffects.Enqueue(new BonusCritDamage(0.4f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new JumpAttack();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class Execute : Action
    {
        public Execute(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Execute";
            this.cover = "Exec";

            this.comboLevel = 2;
            this.comboList = "HH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){

            this.BaseDamageCalculation();
            

            this.damage = (int)Mathf.Round(this.baseDamage/2);    // halved because of execute effect
            this.maxBaseDamage = (int)Mathf.Round(this.maxBaseDamage/2);
            this.minBaseDamage = (int)Mathf.Round(this.minBaseDamage/2);

            this.manaCost = 11;
            this.crit = 0;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new ExecuteEffect(20, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Execute();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class SpinAttack : Action
    {
        public SpinAttack(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Spin Attack";
            this.cover = "Spn-Atk";

            this.comboLevel = 3;
            this.comboList = "HLL";

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
            this.StartEffects.Enqueue(new BonusAccuracy(10, this));
            // this.StartEffects.Enqueue(new ApGainPlus(6, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new SpinAttack();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class Lunge : Action
    {
        public Lunge(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Lunge";
            this.cover = "Lnge";

            this.comboLevel = 3;
            this.comboList = "LSH";

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
            this.StartEffects.Enqueue(new DropChanceOnKill(15, this));
            this.StartEffects.Enqueue(new BonusHpDamage(1.5f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new Lunge();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class MagicBoom : Action
    {
        public MagicBoom(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Magic Boom";
            this.cover = "M-Boom";

            this.comboLevel = 3;
            this.comboList = "HSS";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            

            this.manaCost += 23;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new BonusCreditOnKill(1.5f, this));
            this.StartEffects.Enqueue(new ExtraElementalDamage(200, 0.3f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new MagicBoom();
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    }

    private class WyvernSlayer : Action
    {
        public WyvernSlayer(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Wyvern Slayer";
            this.cover = "WyS";

            this.comboLevel = 4;
            this.comboList = "SSSH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            

            this.manaCost += 39;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new WyvernSlayer_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new ExtraElementalDamage(450, 0.5f, this));
            this.StartEffects.Enqueue(new BonusCritChance(15, this));
            this.StartEffects.Enqueue(new BonusCritDamage(0.5f, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new WyvernSlayer();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class WyvernSlayer_E : UniqueEffect
        {
            public WyvernSlayer_E(Action A){
                this.Action = A;
                this.id = "WyS_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : Ignores Enemy Shield


                int sTmp = this.Action.Enemy.shield;
                this.Action.Enemy.shield = 0;

                int damageDealt = await this.Action.NormalExecute();

                this.Action.Enemy.shield = sTmp;

                return damageDealt;
            }
        }
    }

    private class PaladinsBane : Action
    {
        public PaladinsBane(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Paladins Bane";
            this.cover = "PlB";

            this.comboLevel = 4;
            this.comboList = "LHLH";

            this.Setup();
            this.StopPermanentEffect();
        }

        public override void Setup(){
            this.BaseDamageCalculation();
            

            this.manaCost += 35;
            this.crit = this.Player.crit;
            this.SetAccuracy();

            this.SetEffects();
        }

        public override void SetDamageToWeapon(){
            this.damageToWeapon = (int)Mathf.Round(this.Enemy.damageToWeapons*this.comboDamageMults[this.comboLevel-1]);
        }

        public override void SetEffects(){
            this.StartEffects.Enqueue(new PaladinsBane_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new ManaSteal(92, this));
            this.StartEffects.Enqueue(new BonusShieldDamage(1.7f, this));
            // this.StartEffects.Enqueue(new ResetEnemyAttackTimer(this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A =  new PaladinsBane();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class PaladinsBane_E : UniqueEffect
        {
            public PaladinsBane_E(Action A){
                this.Action = A;
                this.id = "PlB_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : Steals shield

                int damageDealt = await this.Action.NormalExecute();

                if(damageDealt > 0) this.Action.Player.shield = this.Action.Player.maxShield + this.Action.Enemy.shield;

                return damageDealt;
            }
        }
    }

    private class MariasWrath : Action
    {
        public MariasWrath(){
            this.SetGameHandler();
            this.Player = this.GameHandler.Player;
            this.name = "Marias Wrath";
            this.cover = "Maria";

            this.comboLevel = 5;
            this.comboList = "HLSSH";

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
            this.StartEffects.Enqueue(new MariasWrath_E(this)); // IMPORTANT!: unique effects at top

            this.StartEffects.Enqueue(new ExecuteDamage(0, this.baseDamage*4, this));
            this.StartEffects.Enqueue(new BonusCreditOnKill(4.0f, this));
            this.StartEffects.Enqueue(new DropChanceOnKill(70, this));
            return;
        }

        public override async Task Passive(){
            await Task.Yield();
        }

        public override Action Copy(){
            Action A = new MariasWrath();
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class MariasWrath_E : UniqueEffect
        {
            public MariasWrath_E(Action A){
                this.Action = A;
                this.id = "Maria_E";
            }

            public override async Task<int> ActivateUniqueEffect(){ // gets called instead of normal execute functions
                // define unique effect here!

                // Unique Effect : guaranteed hit, 175% dmg


                this.Action.accuracy = int.MaxValue;
                this.Action.damage = (int)Mathf.Round(this.Action.damage*1.75f);

                int damageDealt = await this.Action.NormalExecute();

                return damageDealt;
            }
        }
    }
} // End of Maria Class