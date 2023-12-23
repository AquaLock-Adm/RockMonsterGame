using System.Collections.Generic;
using UnityEngine;

public class Maria : Weapon
{
    public override List<WeaponUpgrade> GetUpgradeTable(){
        List<WeaponUpgrade> table = new List<WeaponUpgrade>();
                                // (baseMinAtk, baseMaxAtk, APR, lifeDrain, EnemyDOT, UpgradeCost)
        table.Add(new WeaponUpgrade(   5,   9, 3, 0.8f , 0.0f,   900)); //(+ 200 (+50 *x))
        table.Add(new WeaponUpgrade(   9,  13, 3, 0.84f, 0.0f,  1100));
        table.Add(new WeaponUpgrade(  15,  19, 3, 0.88f, 0.0f,  1350));
        table.Add(new WeaponUpgrade(  23,  27, 3, 0.92f, 0.0f,  1650));
        table.Add(new WeaponUpgrade(  33,  34, 3, 0.96f, 0.0f,  5000));

        table.Add(new WeaponUpgrade(  35,  43, 4, 1.1f , 0.0f,  5250)); //(+400 (+150 *x))
        table.Add(new WeaponUpgrade(  47,  55, 4, 1.14f, 0.0f,  5650));
        table.Add(new WeaponUpgrade(  61,  69, 4, 1.18f, 0.0f,  6200));
        table.Add(new WeaponUpgrade(  77,  85, 4, 1.22f, 0.0f,  6900));
        table.Add(new WeaponUpgrade(  95, 103, 4, 1.26f, 0.0f, 20000));

        table.Add(new WeaponUpgrade( 100, 112, 5, 1.4f , 0.0f, 20350)); //(+800 (+450 *x))
        table.Add(new WeaponUpgrade( 120, 132, 5, 1.44f, 0.0f, 21150));
        table.Add(new WeaponUpgrade( 142, 154, 5, 1.48f, 0.0f, 22400));
        table.Add(new WeaponUpgrade( 166, 178, 5, 1.52f, 0.0f, 24100));
        table.Add(new WeaponUpgrade( 192, 204, 5, 1.56f, 0.0f, 70000));

        table.Add(new WeaponUpgrade( 180, 196, 6, 2.6f , 0.5f, 71150)); //(+2400 (+1250 *x))
        table.Add(new WeaponUpgrade( 208, 224, 6, 2.65f, 0.7f, 73550));
        table.Add(new WeaponUpgrade( 238, 254, 6, 2.7f , 0.9f, 77200));
        table.Add(new WeaponUpgrade( 270, 286, 6, 2.75f, 1.1f, 82100));
        table.Add(new WeaponUpgrade( 304, 320, 6, 2.8f , 1.3f,200000));

        table.Add(new WeaponUpgrade( 660, 692, 7, 4.0f , 2.5f,     0)); //(== LV21)

        return table;
    }

    public override void InitAbilities(){
        this.Abilities = new List<Action>();
        List<Action> AbilityList = new List<Action>();
        int abilityIndex_c = 0;
        
        // if(!Player.BattleSystem.useSetAbilities){
        //     AbilityList = GetCompleteMoveList();
        // }else{
        //     AbilityList = Player.SetAbilities;
        // }

        AbilityList = GetCompleteMoveList();

        foreach(Action A in AbilityList){
            A.abilityIndex = abilityIndex_c;
            abilityIndex_c++;
            this.Abilities.Add(A);
        }
    }

    public override List<Action> GetCompleteMoveList(){
        List<Action> Actions_l = new List<Action>();
        // Level 1
        Actions_l.Add(new Light(this.Player));
        Actions_l.Add(new Heavy(this.Player));
        Actions_l.Add(new Special(this.Player));
        // Level 3
        Actions_l.Add(new Combo_A(this.Player));
        Actions_l.Add(new Combo_B(this.Player));
        Actions_l.Add(new Combo_C(this.Player));
        Actions_l.Add(new Combo_D(this.Player));
        Actions_l.Add(new Combo_E(this.Player));
        Actions_l.Add(new Combo_F(this.Player));
        // Level 4
        Actions_l.Add(new Combo_A_2(this.Player));
        // Level 5
        Actions_l.Add(new Combo_A_3(this.Player));
        // Level 6
        Actions_l.Add(new Combo_A_4(this.Player));
        // Level 7
        Actions_l.Add(new Combo_A_5(this.Player));

        // WARNING! Sort entries by ComboLevel for Loadout Menu (I was too lazy to sort it there)

        return Actions_l;
    }

    public override List<Action> GetStandartAbilitiesList(){
        List<Action> Actions_l = new List<Action>();
        // Level 1
        Actions_l.Add(new Light(this.Player));
        Actions_l.Add(new Heavy(this.Player));
        Actions_l.Add(new Special(this.Player));
        // Level 3
        Actions_l.Add(new Combo_A(this.Player));
        Actions_l.Add(new Combo_B(this.Player));
        Actions_l.Add(new Combo_C(this.Player));

        // WARNING! Sort entries by ComboLevel for Loadout Menu (I was too lazy to sort it there)

        return Actions_l;
    }

    public override Action CombineActions(string comboList, List<Action> Actions_l){
        Action res;
        switch(comboList){
            case "LHS":
                res = new Combo_A(this.Player);
            break;
            case "LHSS":
                res = new Combo_A_2(this.Player);
            break;
            case "LHSSH":
                res = new Combo_A_3(this.Player);
            break;
            case "LHSSHL":
            
                res = new Combo_A_4(this.Player);
            break;
            case "LHSSHLH":
                res = new Combo_A_5(this.Player);
            break;

            case "LSH":
                res = new Combo_B(this.Player);
            break;

            case "SHL":
                res = new Combo_C(this.Player);
            break;

            case "SSH":
                res = new Combo_D(this.Player);
            break;

            case "LHH":
                res = new Combo_E(this.Player);
            break;

            case "HLL":
                res = new Combo_F(this.Player);
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

    public override Action GetAttackRushFinisher(int finisherLevel){
        Action res = new Light(this.Player);
        if(finisherLevel <= 0){
            Debug.LogError("Error: can't get attack rush finisher under level 1!");
            return res;
        }

        switch(finisherLevel){
            case 1:
                res = new Heavy(this.Player);
            break;

            default:
                res = new MariaFinisherA(this.Player);
            break;
        }

        return res;
    }

    private class MariaFinisherA : Action
    {
        public MariaFinisherA(PlayerCharacter Player){
            this.name = "Finisher A";
            this.cover = "F_A";
            this.totalTime = 700;
            this.Player = Player;
            this.BaseDamageCalculation(20, 1.7f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            Debug.Log("Maria Finisher A queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            Debug.Log("Maria Finisher A dequeued! Queue length: "+ AQ.Actions.Count);
        }

        public override Action Copy(){
            Action A =  new MariaFinisherA(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        } 
    }

    private class Combo_A : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_A(PlayerCharacter Player){
            this.name = "Combo A";
            this.cover = "Com A";
            this.comboLevel = 3;
            this.comboString = "LHS";
            this.AbilityType = AbilityType.SPECIAL;
            this.totalTime = this.heavyAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(30, 1.4f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_A_L(this.Player));
            ComboActionParts.Add(new Combo_A_H(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_A(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_A Class

    private class Combo_A_2 : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_A_2(PlayerCharacter Player){
            this.name = "Combo A2";
            this.cover = "Com A2";
            this.comboLevel = 4;
            this.comboString = "LHSS";
            this.AbilityType = AbilityType.SPECIAL;
            this.totalTime = this.heavyAttackStdTime+200;
            this.Player = Player;
            this.BaseDamageCalculation(76, 1.4f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_A_L(this.Player));
            ComboActionParts.Add(new Combo_A_H(this.Player));
            ComboActionParts.Add(new Combo_A_S(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_A_2(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_A_2 Class

    private class Combo_A_3 : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_A_3(PlayerCharacter Player){
            this.name = "Combo A3";
            this.cover = "Com A3";
            this.comboLevel = 5;
            this.comboString = "LHSSH";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime+400;
            this.Player = Player;
            this.BaseDamageCalculation(115, 1.4f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_A_L(this.Player));
            ComboActionParts.Add(new Combo_A_H(this.Player));
            ComboActionParts.Add(new Combo_A_S(this.Player));
            ComboActionParts.Add(new Combo_A_S(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_A_3(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_A_3 Class

    private class Combo_A_4 : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_A_4(PlayerCharacter Player){
            this.name = "Combo A4";
            this.cover = "Com A4";
            this.comboLevel = 6;
            this.comboString = "LHSSHL";
            this.AbilityType = AbilityType.LIGHT;
            this.totalTime = this.heavyAttackStdTime+600;
            this.Player = Player;
            this.BaseDamageCalculation(138, 1.5f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_A_L(this.Player));
            ComboActionParts.Add(new Combo_A_H(this.Player));
            ComboActionParts.Add(new Combo_A_S(this.Player));
            ComboActionParts.Add(new Combo_A_S(this.Player));
            ComboActionParts.Add(new Combo_A_H(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_A_4(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_A_4 Class

    private class Combo_A_5 : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_A_5(PlayerCharacter Player){
            this.name = "Combo A5";
            this.cover = "Com A5";
            this.comboLevel = 7;
            this.comboString = "LHSSHLH";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime + 800;
            this.Player = Player;
            this.BaseDamageCalculation(190, 1.5f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_A_L(this.Player));
            ComboActionParts.Add(new Combo_A_H(this.Player));
            ComboActionParts.Add(new Combo_A_S(this.Player));
            ComboActionParts.Add(new Combo_A_S(this.Player));
            ComboActionParts.Add(new Combo_A_H(this.Player));
            ComboActionParts.Add(new Combo_A_L(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_A_5(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_A_5 Class

    private class Combo_A_L : Action
    {
        public Combo_A_L(PlayerCharacter Player){
            this.name = "Combo A-L";
            this.cover = "Com A";
            this.comboLevel = 3;
            this.comboString = "L";
            this.AbilityType = AbilityType.LIGHT;
            this.totalTime = this.lightAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_A_L(this.Player);
            return A;
        }
    } // End of Combo_A_L

    private class Combo_A_H : Action
    {
        public Combo_A_H(PlayerCharacter Player){
            this.name = "Combo A-H";
            this.cover = "Com A";
            this.comboLevel = 3;
            this.comboString = "H";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0, this.heavyStdDamageMult);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_A_H(this.Player);
            return A;
        }
    } // End of Combo_A_H

    private class Combo_A_S : Action
    {
        public Combo_A_S(PlayerCharacter Player){
            this.name = "Combo A-S";
            this.cover = "Com A";
            this.comboLevel = 3;
            this.comboString = "S";
            this.AbilityType = AbilityType.SPECIAL;
            this.totalTime = this.specialAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0, this.specialStdDamageMult);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_A_S(this.Player);
            return A;
        }
    } // End of Combo_A_S




    private class Combo_B : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_B(PlayerCharacter Player){
            this.name = "Combo B";
            this.cover = "Com B";
            this.comboLevel = 3;
            this.comboString = "LSH";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(35, 1.35f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_B_L(this.Player));
            ComboActionParts.Add(new Combo_B_S(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_B(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_B Class

    private class Combo_B_L : Action
    {
        public Combo_B_L(PlayerCharacter Player){
            this.name = "Combo B-L";
            this.cover = "Com B";
            this.comboLevel = 3;
            this.comboString = "L";
            this.AbilityType = AbilityType.LIGHT;
            this.totalTime = this.lightAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_B_L(this.Player);
            return A;
        }
    } // End of Combo_B_L

    private class Combo_B_S : Action
    {
        public Combo_B_S(PlayerCharacter Player){
            this.name = "Combo B-S";
            this.cover = "Com B";
            this.comboLevel = 3;
            this.comboString = "S";
            this.AbilityType = AbilityType.SPECIAL;
            this.totalTime = this.specialAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0, this.specialStdDamageMult);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_B_S(this.Player);
            return A;
        }
    } // End of Combo_B_S



    private class Combo_C : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_C(PlayerCharacter Player){
            this.name = "Combo C";
            this.cover = "Com C";
            this.comboLevel = 3;
            this.comboString = "SHL";
            this.AbilityType = AbilityType.LIGHT;
            this.totalTime = this.lightAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(35, 1.4f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_C_S(this.Player));
            ComboActionParts.Add(new Combo_C_H(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_C(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_C Class

    private class Combo_C_H : Action
    {
        public Combo_C_H(PlayerCharacter Player){
            this.name = "Combo C-H";
            this.cover = "Com C";
            this.comboLevel = 3;
            this.comboString = "H";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0, this.heavyStdDamageMult);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_C_H(this.Player);
            return A;
        }
    } // End of Combo_C_H

    private class Combo_C_S : Action
    {
        public Combo_C_S(PlayerCharacter Player){
            this.name = "Combo C-S";
            this.cover = "Com C";
            this.comboLevel = 3;
            this.comboString = "S";
            this.AbilityType = AbilityType.SPECIAL;
            this.totalTime = this.specialAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0, this.specialStdDamageMult);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_C_S(this.Player);
            return A;
        }
    } // End of Combo_C_S



    private class Combo_D : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_D(PlayerCharacter Player){
            this.name = "Combo D";
            this.cover = "Com D";
            this.comboLevel = 3;
            this.comboString = "SSH";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(25, 1.35f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_D_S(this.Player));
            ComboActionParts.Add(new Combo_D_S(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_D(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_D Class

    private class Combo_D_S : Action
    {
        public Combo_D_S(PlayerCharacter Player){
            this.name = "Combo D-S";
            this.cover = "Com D";
            this.comboLevel = 3;
            this.comboString = "S";
            this.AbilityType = AbilityType.SPECIAL;
            this.totalTime = this.specialAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0, this.specialStdDamageMult);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_D_S(this.Player);
            return A;
        }
    } // End of Combo_D_S

    private class Combo_D_H : Action
    {
        public Combo_D_H(PlayerCharacter Player){
            this.name = "Combo D-H";
            this.cover = "Com D";
            this.comboLevel = 3;
            this.comboString = "H";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0, this.heavyStdDamageMult);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_D_H(this.Player);
            return A;
        }
    } // End of Combo_D_H



    private class Combo_E : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_E(PlayerCharacter Player){
            this.name = "Combo E";
            this.cover = "Com E";
            this.comboLevel = 3;
            this.comboString = "LHH";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(25, 1.35f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_E_L(this.Player));
            ComboActionParts.Add(new Combo_E_H(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_E(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_E Class

    private class Combo_E_L : Action
    {
        public Combo_E_L(PlayerCharacter Player){
            this.name = "Combo E-L";
            this.cover = "Com E";
            this.comboLevel = 3;
            this.comboString = "L";
            this.AbilityType = AbilityType.LIGHT;
            this.totalTime = this.lightAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_E_L(this.Player);
            return A;
        }
    } // End of Combo_E_L

    private class Combo_E_H : Action
    {
        public Combo_E_H(PlayerCharacter Player){
            this.name = "Combo E-H";
            this.cover = "Com E";
            this.comboLevel = 3;
            this.comboString = "H";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0, this.heavyStdDamageMult);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_E_H(this.Player);
            return A;
        }
    } // End of Combo_E_H



    private class Combo_F : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Combo_F(PlayerCharacter Player){
            this.name = "Combo F";
            this.cover = "Com F";
            this.comboLevel = 3;
            this.comboString = "HLL";
            this.AbilityType = AbilityType.LIGHT;
            this.totalTime = this.lightAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(25, 1.1f);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Combo_F_H(this.Player));
            ComboActionParts.Add(new Combo_F_L(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillPlayerActionHandlerWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillPlayerActionHandlerWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Combo_F(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }
    } // End of Combo_F Class

    private class Combo_F_L : Action
    {
        public Combo_F_L(PlayerCharacter Player){
            this.name = "Combo F-L";
            this.cover = "Com F";
            this.comboLevel = 3;
            this.comboString = "L";
            this.AbilityType = AbilityType.LIGHT;
            this.totalTime = this.lightAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_F_L(this.Player);
            return A;
        }
    } // End of Combo_F_L

    private class Combo_F_H : Action
    {
        public Combo_F_H(PlayerCharacter Player){
            this.name = "Combo F-H";
            this.cover = "Com F";
            this.comboLevel = 3;
            this.comboString = "H";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = this.heavyAttackStdTime;
            this.Player = Player;
            this.BaseDamageCalculation(0, this.heavyStdDamageMult);
        }

        public override void QueueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
            return;
        }

        public override void DequeueAction(PlayerActionHandler AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
            return;
        }

        public override Action Copy(){
            Action A =  new Combo_F_H(this.Player);
            return A;
        }
    } // End of Combo_F_H
} // End of Maria Class