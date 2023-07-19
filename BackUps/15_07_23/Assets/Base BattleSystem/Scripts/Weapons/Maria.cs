using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Maria : Weapon
{
    public override void SetWeaponType(){
        
        this.Type = WeaponType.SWORD;
    }

    public override List<WeaponUpgrade> GetUpgradeTable(){
        List<WeaponUpgrade> table = new List<WeaponUpgrade>();
                                // (baseMinAtk, baseMaxAtk, APR, lifeDrain, EnemyDOT, UpgradeCost)
        table.Add(new WeaponUpgrade(   5,   9, 3, 0.8f , 0.0f,   900)); //(+ 200 (+50 *x))
        table.Add(new WeaponUpgrade(   9,  13, 3, 0.85f, 0.0f,  1100));
        table.Add(new WeaponUpgrade(  15,  19, 3, 0.9f , 0.0f,  1350));
        table.Add(new WeaponUpgrade(  23,  27, 3, 0.95f, 0.0f,  1650));
        table.Add(new WeaponUpgrade(  33,  34, 3, 1.0f , 0.0f,  5000));

        table.Add(new WeaponUpgrade(  35,  43, 4, 1.4f , 0.0f,  5250)); //(+400 (+150 *x))
        table.Add(new WeaponUpgrade(  47,  55, 4, 1.45f, 0.0f,  5650));
        table.Add(new WeaponUpgrade(  61,  69, 4, 1.5f , 0.0f,  6200));
        table.Add(new WeaponUpgrade(  77,  85, 4, 1.55f, 0.0f,  6900));
        table.Add(new WeaponUpgrade(  95, 103, 4, 1.6f , 0.0f, 20000));

        table.Add(new WeaponUpgrade( 100, 112, 5, 2.0f , 0.0f, 20350)); //(+800 (+450 *x))
        table.Add(new WeaponUpgrade( 120, 132, 5, 2.05f, 0.0f, 21150));
        table.Add(new WeaponUpgrade( 142, 154, 5, 2.1f , 0.0f, 22400));
        table.Add(new WeaponUpgrade( 166, 178, 5, 2.15f, 0.0f, 24100));
        table.Add(new WeaponUpgrade( 192, 204, 5, 2.2f , 0.0f, 70000));

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

        this.Abilities.Add(new Light(this.Player));
        this.Abilities.Add(new Heavy(this.Player));
        this.Abilities.Add(new Special(this.Player));
        this.Abilities.Add(new Exodus1(this.Player));
        this.Abilities.Add(new Exodus2(this.Player));
        this.Abilities.Add(new Genesis1(this.Player));
        this.Abilities.Add(new Genesis2(this.Player));

        int abilityIndex_c = 0;

        foreach(Action A in this.Abilities){
            A.abilityIndex = abilityIndex_c;
            abilityIndex_c++;
        }
    }

    public override List<Action> GetCompleteMoveList(){
        List<Action> Actions_l = new List<Action>();

        Actions_l.Add(new Light(this.Player));
        Actions_l.Add(new Heavy(this.Player));
        Actions_l.Add(new Special(this.Player));
        Actions_l.Add(new Exodus1(this.Player));
        Actions_l.Add(new Exodus2(this.Player));
        Actions_l.Add(new Genesis1(this.Player));
        Actions_l.Add(new Genesis2(this.Player));

        return Actions_l;
    }

    public override Action CombineActions(string comboList, List<Action> Actions_l){
        Action res;
        switch(comboList){
            case "LHS":
                res = new Exodus1(this.Player);
            break;

            case "LLHHS":
                res = new Exodus2(this.Player);
            break;

            case "LSH":
                res = new Genesis1(this.Player);
            break;

            case "LLSSH":
                res = new Genesis2(this.Player);
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

        public override void QueueAction(ActionQueue AQ){
            Debug.Log("Maria Finisher A queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        }

        public override void DequeueAction(ActionQueue AQ){
            Debug.Log("Maria Finisher A dequeued! Queue length: "+ AQ.Actions.Count);
        }

        public override Action Copy(){
            Action A =  new MariaFinisherA(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        } 
    }

    private class Exodus1 : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Exodus1(PlayerCharacter Player){
            this.name = "Exodus I";
            this.cover = "Ex1";
            this.comboLevel = 3;
            this.comboString = "LHS";
            this.AbilityType = AbilityType.SPECIAL;
            this.totalTime = 500;
            this.Player = Player;
            this.BaseDamageCalculation(20, 1.7f);
        }

        public override void QueueAction(ActionQueue AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Exodus1_partA(this.Player));
            ComboActionParts.Add(new Exodus1_partB(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillActionQueueWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(ActionQueue AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillActionQueueWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Exodus1(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class Exodus1_partA : Action
        {
            public Exodus1_partA(PlayerCharacter Player){
                this.name = "Exodus I A";
                this.cover = "Ex1A";
                this.comboLevel = 3;
                this.comboString = "L";
                this.AbilityType = AbilityType.LIGHT;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Exodus1_partA(this.Player);
                return A;
            }
        } // End of part A

        private class Exodus1_partB : Action
        {
            public Exodus1_partB(PlayerCharacter Player){
                this.name = "Exodus I B";
                this.cover = "Ex1B";
                this.comboLevel = 3;
                this.comboString = "H";
                this.AbilityType = AbilityType.HEAVY;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Exodus1_partB(this.Player);
                return A;
            }
        } // End of part B
    } // End of Exodus 1 Class

    private class Exodus2 : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Exodus2(PlayerCharacter Player){
            this.name = "Exodus II";
            this.cover = "Ex2";
            this.comboLevel = 5;
            this.comboString = "LLHHS";
            this.AbilityType = AbilityType.SPECIAL;
            this.totalTime = 850;
            this.Player = Player;
            this.BaseDamageCalculation(200, 1.8f);
        }

        public override void QueueAction(ActionQueue AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Exodus2_partA(this.Player));
            ComboActionParts.Add(new Exodus2_partB(this.Player));
            ComboActionParts.Add(new Exodus2_partC(this.Player));
            ComboActionParts.Add(new Exodus2_partD(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillActionQueueWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(ActionQueue AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillActionQueueWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Exodus2(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class Exodus2_partA : Action
        {
            public Exodus2_partA(PlayerCharacter Player){
                this.name = "Exodus II A";
                this.cover = "Ex2A";
                this.comboLevel = 5;
                this.comboString = "L";
                this.AbilityType = AbilityType.LIGHT;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Exodus2_partA(this.Player);
                return A;
            }
        } // End of part A

        private class Exodus2_partB : Action
        {
            public Exodus2_partB(PlayerCharacter Player){
                this.name = "Exodus II B";
                this.cover = "Ex2B";
                this.comboLevel = 5;
                this.comboString = "L";
                this.AbilityType = AbilityType.LIGHT;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Exodus2_partB(this.Player);
                return A;
            }
        } // End of part B

        private class Exodus2_partC : Action
        {
            public Exodus2_partC(PlayerCharacter Player){
                this.name = "Exodus II C";
                this.cover = "Ex2C";
                this.comboLevel = 5;
                this.comboString = "H";
                this.AbilityType = AbilityType.HEAVY;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Exodus2_partC(this.Player);
                return A;
            }
        } // End of part C

        private class Exodus2_partD : Action
        {
            public Exodus2_partD(PlayerCharacter Player){
                this.name = "Exodus II B";
                this.cover = "Ex2B";
                this.comboLevel = 5;
                this.comboString = "H";
                this.AbilityType = AbilityType.HEAVY;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Exodus2_partD(this.Player);
                return A;
            }
        } // End of part D
    } // End of Exodus 2 Class

    private class Genesis1 : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Genesis1(PlayerCharacter Player){
            this.name = "Genesis I";
            this.cover = "Gen1";
            this.comboLevel = 3;
            this.comboString = "LSH";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = 500;
            this.Player = Player;
            this.BaseDamageCalculation(35);
        }

        public override void QueueAction(ActionQueue AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Genesis1_partA(this.Player));
            ComboActionParts.Add(new Genesis1_partB(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillActionQueueWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(ActionQueue AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillActionQueueWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Genesis1(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class Genesis1_partA : Action
        {
            public Genesis1_partA(PlayerCharacter Player){
                this.name = "Genesis I A";
                this.cover = "Gen1A";
                this.comboLevel = 3;
                this.comboString = "L";
                this.AbilityType = AbilityType.LIGHT;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Genesis1_partA(this.Player);
                return A;
            }
        } // End of part A

        private class Genesis1_partB : Action
        {
            public Genesis1_partB(PlayerCharacter Player){
                this.name = "Genesis I B";
                this.cover = "Gen1B";
                this.comboLevel = 3;
                this.comboString = "S";
                this.AbilityType = AbilityType.SPECIAL;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Genesis1_partB(this.Player);
                return A;
            }
        } // End of part B
    } // End of Genesis 1 Class

    private class Genesis2 : Action 
    {
        private List<Action> RemovedActionsOnQueue = new List<Action>();

        public Genesis2(PlayerCharacter Player){
            this.name = "Genesis II";
            this.cover = "Gen2";
            this.comboLevel = 5;
            this.comboString = "LLSSH";
            this.AbilityType = AbilityType.HEAVY;
            this.totalTime = 800;
            this.Player = Player;
            this.BaseDamageCalculation(240);
        }

        public override void QueueAction(ActionQueue AQ){
            // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());

            List<Action> ComboActionParts = new List<Action>();
            ComboActionParts.Add(new Genesis2_partA(this.Player));
            ComboActionParts.Add(new Genesis2_partB(this.Player));
            ComboActionParts.Add(new Genesis2_partC(this.Player));
            ComboActionParts.Add(new Genesis2_partD(this.Player));
            ComboActionParts.Add(this);

            this.RemovedActionsOnQueue = FillActionQueueWithComboAbilities(AQ, ComboActionParts);
        }

        public override void DequeueAction(ActionQueue AQ){
            // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);

            RefillActionQueueWithPreviousActions(AQ, this.RemovedActionsOnQueue);
        }

        public override Action Copy(){
            Action A =  new Genesis2(this.Player);
            A.abilityIndex = this.abilityIndex;
            return A;
        }

        private class Genesis2_partA : Action
        {
            public Genesis2_partA(PlayerCharacter Player){
                this.name = "Genesis II A";
                this.cover = "Gen2A";
                this.comboLevel = 5;
                this.comboString = "L";
                this.AbilityType = AbilityType.LIGHT;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Genesis2_partA(this.Player);
                return A;
            }
        } // End of part A

        private class Genesis2_partB : Action
        {
            public Genesis2_partB(PlayerCharacter Player){
                this.name = "Genesis II B";
                this.cover = "Gen2B";
                this.comboLevel = 5;
                this.comboString = "L";
                this.AbilityType = AbilityType.LIGHT;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Genesis2_partB(this.Player);
                return A;
            }
        } // End of part B

        private class Genesis2_partC : Action
        {
            public Genesis2_partC(PlayerCharacter Player){
                this.name = "Genesis II C";
                this.cover = "Gen2A";
                this.comboLevel = 5;
                this.comboString = "S";
                this.AbilityType = AbilityType.SPECIAL;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Genesis2_partC(this.Player);
                return A;
            }
        } // End of part C

        private class Genesis2_partD : Action
        {
            public Genesis2_partD(PlayerCharacter Player){
                this.name = "Genesis II D";
                this.cover = "Gen2D";
                this.comboLevel = 5;
                this.comboString = "S";
                this.AbilityType = AbilityType.SPECIAL;
                this.totalTime = 300;
                this.Player = Player;
                this.BaseDamageCalculation(0);
            }

            public override void QueueAction(ActionQueue AQ){
                // Debug.Log("Exodus I queued! Pos: "+ (AQ.Actions.Count-1).ToString());
                return;
            }

            public override void DequeueAction(ActionQueue AQ){
                // Debug.Log("Exodus I dequeued! Queue length: "+ AQ.Actions.Count);
                return;
            }

            public override Action Copy(){
                Action A =  new Genesis2_partD(this.Player);
                return A;
            }
        } // End of part D
    } // End of Genesis 2 Class
} // End of Maria Class