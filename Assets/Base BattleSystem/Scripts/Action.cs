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

public enum AbilityType {
    LIGHT,
    HEAVY,
    SPECIAL,
    NONE
}

// ac0
public abstract class Action
{
    [Header("Battle Multipliers")]
    [SerializeField] public static float critMultiplier = 1.5f;

    // avg1
    public PlayerCharacter Player;
    public Enemy Enemy;

    public string name = "";
    public string cover = "";
    public int comboLevel = 0;
    public string comboString = "";
    public AbilityType AbilityType = AbilityType.LIGHT;
    public int abilityIndex = -1;
    public int totalTime = 1000;
    public int damage = 0;
    public int baseDamage = 0;

    protected int lightAttackStdTime = 300;
    protected int specialAttackStdTime = 150;
    protected int heavyAttackStdTime = 600;

    protected float heavyStdDamageMult = 1.2f;
    protected float specialStdDamageMult = 0.8f;

    protected int timeBeforeAttackRush = 2000;
    protected float attackRushDamageScaling = 0.25f;
    
    public abstract void QueueAction(PlayerActionHandler AQ); // called when Action is queued into action queue
    public abstract void DequeueAction(PlayerActionHandler AQ); // called when action is dequeued from action queue
    public abstract Action Copy();

#region SetUp Functions
    public void SetEnemy(Enemy Enemy){
        this.Enemy = Enemy;
    }
    public virtual void BaseDamageCalculation(int baseDmg, float damageMultiplicator = 1.0f){
        if(this.Player == null){
            Debug.LogError("Player not set for action "+this.name+"!");
            this.baseDamage = 0;
            return;
        }
        this.baseDamage = baseDmg + Random.Range(this.Player.GetAttackMin(), this.Player.GetAttackMax()+1);
        this.baseDamage = (int)Mathf.Round(this.baseDamage*damageMultiplicator);
        this.damage = this.baseDamage;
    }
    public virtual void TriggerOnHit(){
        // Debug.Log("On-Hit!");
    }
    public virtual void TriggerOnDeath(){
        // Debug.Log("On Death!!");
    }

    // used by combo Actions to change The PlayerActionHandler Action list after being queued
    public List<Action> FillPlayerActionHandlerWithComboAbilities(PlayerActionHandler AQ, List<Action> ComboActionParts){
        List<Action> RemovedActions = new List<Action>();

        if(ComboActionParts.Count > AQ.Actions.Count){
            Debug.LogError("Too many parts in Combo Action!");
            return RemovedActions;
        }
        if(ComboActionParts.Count <= 1){
            Debug.LogError("Too few combo action parts! " + ComboActionParts.Count.ToString());
            return RemovedActions;
        }

        AQ.Actions.RemoveAt(AQ.Actions.Count-1); // without ComboEnder

        for(int c=0; c<ComboActionParts.Count-1;c++){
            Action Rem = AQ.RemoveLastAction();
            // Debug.Log("Removed "+Rem.name);
            RemovedActions.Add(Rem);
        }

        RemovedActions.Reverse();

        for(int i=0; i<ComboActionParts.Count-1;i++){
            AQ.AddAction(ComboActionParts[i]);
        }

        Action ComboEnder = ComboActionParts[ComboActionParts.Count-1];
        AQ.Actions.Add(ComboEnder);
        AQ.UpdateVisualizer();

        return RemovedActions;
    }

    // Counter part to FillPlayerActionHandlerWithComboAbilities()
    public void RefillPlayerActionHandlerWithPreviousActions(PlayerActionHandler AQ, List<Action> RemovedActions){
        if(RemovedActions.Count <= 1){
            Debug.LogError("Illogical amount of Removed Actions! "+RemovedActions.Count.ToString());
            return;
        }

        for(int c=0;c<RemovedActions.Count;c++){
            AQ.RemoveLastAction();
        }

        for(int i=0;i<RemovedActions.Count;i++){
            Action AddA = RemovedActions[i];
            // Debug.Log("Adding "+AddA.name);
            AQ.AddAction(AddA);
        }
    }
#endregion

    public virtual async Task<int> NormalExecute(){
        if(!CheckBattleUnits()) return 0;
        // Debug.Log("Executing: "+this.name);

        await Task.Delay(this.totalTime);
        return this.damage;
    }


#region Normal Execute Check Functions
    protected bool CheckBattleUnits(){
        if(this.Player == null){
            Debug.LogError("Didn't set Player at Action Setup!");
            return false;
        }else if(this.Enemy == null){
            Debug.LogError("Didn't set Enemy before executing action!");
            return false;
        }
        return true;
    }
#endregion

    public override string ToString(){
        
        return this.name;
    }
}// Action Class

// ac1
public class Light : Action
{
    public Light(PlayerCharacter Player){
        this.name = "Light Attack";
        this.cover = "Light";
        this.comboLevel = 1;
        this.comboString = "L";
        this.AbilityType = AbilityType.LIGHT;
        this.totalTime = this.lightAttackStdTime;
        this.Player = Player;
        this.BaseDamageCalculation(0);
    }

    public override void QueueAction(PlayerActionHandler AQ){
        // Debug.Log("Light Attack queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        return;
    }

    public override void DequeueAction(PlayerActionHandler AQ){
        // Debug.Log("Light Attack dequeued! Queue length: "+ AQ.Actions.Count);
        return;
    }

    public override Action Copy(){
        Action A =  new Light(this.Player);
        A.abilityIndex = this.abilityIndex;
        return A;
    } 
}

// ac2
public class Heavy : Action     // slower but twice damage
{
    public Heavy(PlayerCharacter Player){
        this.name = "Heavy Attack";
        this.cover = "Heavy";
        this.comboLevel = 1;
        this.comboString = "H";
        this.AbilityType = AbilityType.HEAVY;
        this.totalTime = this.heavyAttackStdTime;
        this.Player = Player;
        this.BaseDamageCalculation(0, heavyStdDamageMult);
    }

    public override void QueueAction(PlayerActionHandler AQ){
        // Debug.Log("Heavy Attack queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        return;
    }

    public override void DequeueAction(PlayerActionHandler AQ){
        // Debug.Log("Heavy Attack dequeued! Queue length: "+ AQ.Actions.Count);
        return;
    }

    public override Action Copy(){
        Action A =  new Heavy(this.Player);
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

// ac3
public class Special : Action   // slower but more knockback
{
    public Special(PlayerCharacter Player){
        this.name = "Special Attack";
        this.cover = "Special";
        this.comboLevel = 1;
        this.comboString = "S";
        this.AbilityType = AbilityType.SPECIAL;
        this.totalTime = this.specialAttackStdTime;
        this.Player = Player;
        this.BaseDamageCalculation(0, specialStdDamageMult);
    }

    public override void QueueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        return;
    }

    public override void DequeueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack dequeued! Queue length: "+ AQ.Actions.Count);
        return;
    }

    public override Action Copy(){
        Action A =  new Special(this.Player);
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

public class LowBlock : Action   // slower but more knockback
{
    public LowBlock(PlayerCharacter Player){
        this.name = "Low Block";
        this.cover = "Low";
        this.AbilityType = AbilityType.LIGHT;
        this.totalTime = this.lightAttackStdTime;
        this.Player = Player;
    }

    public override void QueueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        return;
    }

    public override void DequeueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack dequeued! Queue length: "+ AQ.Actions.Count);
        return;
    }

    public override Action Copy(){
        Action A =  new LowBlock(this.Player);
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

public class MidBlock : Action   // slower but more knockback
{
    public MidBlock(PlayerCharacter Player){
        this.name = "Mid Block";
        this.cover = "Mid";
        this.AbilityType = AbilityType.HEAVY;
        this.totalTime = this.heavyAttackStdTime;
        this.Player = Player;
    }

    public override void QueueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        return;
    }

    public override void DequeueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack dequeued! Queue length: "+ AQ.Actions.Count);
        return;
    }

    public override Action Copy(){
        Action A =  new MidBlock(this.Player);
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

public class HighBlock : Action   // slower but more knockback
{
    public HighBlock(PlayerCharacter Player){
        this.name = "High Block";
        this.cover = "High";
        this.AbilityType = AbilityType.SPECIAL;
        this.totalTime = this.specialAttackStdTime;
        this.Player = Player;
    }

    public override void QueueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        return;
    }

    public override void DequeueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack dequeued! Queue length: "+ AQ.Actions.Count);
        return;
    }

    public override Action Copy(){
        Action A =  new HighBlock(this.Player);
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

public class NoBlock : Action   // slower but more knockback
{
    public NoBlock(PlayerCharacter Player){
        this.name = "No Block";
        this.cover = "None";
        this.AbilityType = AbilityType.NONE;
        this.totalTime = this.specialAttackStdTime;
        this.Player = Player;
    }

    public override void QueueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        return;
    }

    public override void DequeueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack dequeued! Queue length: "+ AQ.Actions.Count);
        return;
    }

    public override Action Copy(){
        Action A =  new NoBlock(this.Player);
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

// ac3
public class AttackRushAction : Action
{
    public AttackRushAction(PlayerCharacter Player){
        this.name = "Attack Rush";
        this.cover = "RUSH";
        this.totalTime = this.timeBeforeAttackRush;
        this.Player = Player;
    }

    public override void QueueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack queued! Pos: "+ (AQ.Actions.Count-1).ToString());
        return;
    }

    public override void DequeueAction(PlayerActionHandler AQ){
        // Debug.Log("Special Attack dequeued! Queue length: "+ AQ.Actions.Count);
        return;
    }

    public override Action Copy(){
        Action A =  new AttackRushAction(this.Player);
        A.abilityIndex = this.abilityIndex;
        return A;
    }

    public override async Task<int> NormalExecute(){
        if(!CheckBattleUnits()) return 0;
        Player.state = PlayerState.ATTACKRUSH;
        this.Enemy.EnableAttackRush(this.Player.GetAttackMin(), this.Player.GetAttackMax(), this.attackRushDamageScaling);

        await Task.Delay(this.totalTime);
        this.Player.EnableAttackRushInputs(true);
        this.Player.StartAttackRushHeatDrain();

        while(Player.state == PlayerState.ATTACKRUSH){
            await Task.Yield();
        }
        
        return this.Enemy.GetAttackRushDamage();
    }
}


public class PlaceHolderAction : Action
{
    public PlaceHolderAction(){
    }

    public override void QueueAction(PlayerActionHandler AQ){
        Debug.LogError("Trying to queue a Placeholder Action! This should NOT be happening.");
    }

    public override void DequeueAction(PlayerActionHandler AQ){
        Debug.LogError("Trying to dequeue a Placeholder Action! This should NOT be happening.");
    }

    public override Action Copy(){
        Action A =  new PlaceHolderAction();
        A.abilityIndex = this.abilityIndex;
        return A;
    } 
}