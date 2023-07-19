using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

// ec0
public abstract class Effect
{   
    // evg1
    public Action Action;
    // evg2
    public string id;
    // ef1
    public abstract void ActivateEffect();
    // ef2
    public override string ToString(){
        return this.id;
    }
}

// WORKS! MH: once, AOE: once  == once
// ec1
// public class ApGainPlus : Effect
// {
//     private int apGain = 0;

//     public ApGainPlus(int apGain, Action A){
//         this.apGain = apGain;
//         this.Action = A;
//         this.id = "APGn+"+apGain.ToString();
//     }

//     public override void ActivateEffect(){
//         this.Action.apGain += this.apGain;
//     }
// }

// WORKS! MH: once, AOE: once  == once
// ec2
public class ManaSteal : Effect
{
    private int manaGain = 0;

    public ManaSteal(int manaGain, Action A){
        this.manaGain = manaGain;
        this.Action = A;
        this.id = "MSteal+"+manaGain.ToString();
    }

    public override void ActivateEffect(){
        this.Action.manaGain += this.manaGain + this.Action.manaCost;
    }
}

// WORKS! MH: x, AOE: x  == mult
// ec3
public class BonusShieldDamage : Effect
{
    private float damagetoShieldf = 1.0f;

    public BonusShieldDamage(float mult, Action A){
        this.damagetoShieldf = mult;
        this.Action = A;
        this.id = "ShieldDmg"+mult.ToString();
    }

    public override void ActivateEffect(){
        if (this.Action.Enemy.shield > 0) {
            int oDamage = this.Action.damage;
            this.Action.damage = (int)Mathf.Round(this.Action.damage * this.damagetoShieldf);
            this.Action.bonusEffectDamage += this.Action.damage - oDamage;
        }
        this.Action.StartEffects.Enqueue(this);
    }
}

// WORKS! MH: x, AOE: x  == mult
// ec4
public class BonusHpDamage : Effect
{
    private float hpDamageMulitplier = 1.0f;

    public BonusHpDamage(float mult, Action A){
        this.hpDamageMulitplier = mult;
        this.Action = A;
        this.id = "HpDmg"+mult.ToString();
    }

    public override void ActivateEffect(){
        if (this.Action.Enemy.shield <= 0) {
            int oDamage = this.Action.damage;
            this.Action.damage = (int)Mathf.Round(this.Action.damage * this.hpDamageMulitplier);
            this.Action.bonusEffectDamage += this.Action.damage - oDamage;
        }
        this.Action.StartEffects.Enqueue(this);
    }
}

// WORKS! MH: once, AOE: once  == once
// ec5
public class BonusCreditOnKill : Effect
{
    private float bonusCreditf = 1.0f; // NOTE: would be better as a factorial increase but im too lazy rn :<

    public BonusCreditOnKill(float bonusCreditf, Action A){
        this.bonusCreditf = bonusCreditf;
        this.Action = A;
        this.id = "Credit_f"+bonusCreditf.ToString();
    }

    public override void ActivateEffect(){
        this.Action.bonusCreditOnKill = (int)Mathf.Round(this.Action.Enemy.killPrice * this.bonusCreditf);
    }
}

// WORKS! MH: x, AOE: x  == mult
// ec6
public class ExecuteDamage : Effect
{
    /*private int t1 = 50; // first threshhold is at 50% enemy maxhealth
    private int t2 = 30; // second threshhold is at 25% enemy maxhealth
    private int t3 = 15; // third threshhold is at 10% enemy maxhealth

    private float t1Mult = 1.5f; // at threshold t1 damage is multiplied by 1.5x
    private float t2Mult = 2.0f;
    private float t3Mult = 3.0f;*/

    private int maxDamage;
    private int baseDamage;

    public ExecuteDamage(int baseD, int max, Action A){
        this.maxDamage = max;
        this.baseDamage = baseD;
        this.Action = A;
        this.id = "XDmg<"+max.ToString();
    }

    public override void ActivateEffect(){
        int EnemyHpP = (int)Mathf.Round(((float)this.Action.Enemy.healthPoints/this.Action.Enemy.maxHealthPoints) * 100);

        //Debug.Log("Enemy Hp is at "+EnemyHpP+"% maximum health.");

        int increase = (int)Mathf.Round((this.maxDamage - this.baseDamage)/100);

        this.Action.damage += this.baseDamage + (int)Mathf.Round(increase*(100 - EnemyHpP));
        this.Action.bonusEffectDamage += this.baseDamage + (int)Mathf.Round(increase*(100 - EnemyHpP));

        this.Action.StartEffects.Enqueue(this);

        /*if(EnemyHpP <= this.t3) this.Action.damage = (int)Mathf.Round(this.Action.damage * this.t3Mult);
        else if(EnemyHpP <= this.t2) this.Action.damage = (int)Mathf.Round(this.Action.damage * this.t2Mult);
        else if(EnemyHpP <= this.t1) this.Action.damage = (int)Mathf.Round(this.Action.damage * this.t1Mult);*/
    }
}

// WORKS! MH: x, AOE: x  == mult
// ec7
public class ExtraElementalDamage : Effect
{
    private float bonusf = 0.0f;
    private int bonus = 0;

    public ExtraElementalDamage(int bonus, float bonusf, Action A){
        this.bonusf = bonusf;
        this.bonus = bonus;
        this.Action = A;
        this.id = "EleDmg+"+bonus.ToString() + "_f" + bonusf;
    }

    public override void ActivateEffect(){
        this.Action.elemWeaknessBonus = this.bonus;
        this.Action.elemWeaknessBonusFactor = this.bonusf;
        this.Action.StartEffects.Enqueue(this);
    }
}

// WORKS! MH: once, AOE: once  == once
// ec8
// public class ResetEnemyAttackTimer : Effect
// {
//     public ResetEnemyAttackTimer(Action A){
//         this.Action = A;
//         this.id = "RESET";
//     }

//     public override void ActivateEffect(){
//         this.Action.resetEnemyAttackTimer = true;
//     }
// }

// WORKS! MH: once, AOE: once  == once
// ec9
public class DecreasedDamageToWeapon : Effect
{
    float d2Weaponf = 1.0f;

    public DecreasedDamageToWeapon(float damagef, Action A){
        this.d2Weaponf = damagef;
        this.Action = A;
        this.id = "WpnDmg_x"+damagef.ToString();
    }

    public override void ActivateEffect(){
        this.Action.damageToWeapon = (int)Mathf.Round(this.Action.damageToWeapon * this.d2Weaponf);
    }
}

// WORKS! MH: once, AOE: once  == once
// ec10
public class DropChanceOnKill : Effect
{
    int increase = 0; // 0% drop chance increase

    public DropChanceOnKill(int inc, Action A){
        this.increase = inc;
        this.Action = A;
        this.id = "Drop+"+inc.ToString()+"%";
    }

    public override void ActivateEffect(){
        this.Action.dropChanceIncrease = this.increase;
    }
}

// WORKS! MH: once, AOE: once  == once
// ec11
public class BonusCritChance : Effect
{
    int increase = 0; // 0% crit chance increase

    public BonusCritChance(int inc, Action A){
        this.increase = inc;
        this.Action = A;
        this.id = "CrtP+"+inc.ToString()+"%";
    }

    public override void ActivateEffect(){
        this.Action.crit += this.increase;
    }
}

// WORKS! MH: once, AOE: once  == once
// ec12
public class BonusAccuracy : Effect
{
    int increase = 0; // 0% accuracy increase

    public BonusAccuracy(int inc, Action A){
        this.increase = inc;
        this.Action = A;
        this.id = "AccP+"+inc.ToString()+"%";
    }

    public override void ActivateEffect(){
        this.Action.accuracy += this.increase;
    }
}

// WORKS! MH: once, AOE: once  == once
// ec13
public class BonusCritDamage : Effect
{
    float increasef = 0.0f; // +x0.0 crit damage

    public BonusCritDamage(float incf, Action A){
        this.increasef = incf;
        this.Action = A;
        this.id = "CrtDmg+"+incf.ToString()+"f";
    }

    public override void ActivateEffect(){
        this.Action.bonusCritFactor += this.increasef; // important! is set to x1.0 automaticly and this factor will be on top!
    }
}

// WORKS! MH: once, AOE: once  == once
// ec14
public class ExecuteEffect : Effect // NOTE!!!!! Does NOT work on bosses, TODO: add bosses
{
    int threshholdP = 0; // kills under 0% max health

    public ExecuteEffect(int tP, Action A){
        this.threshholdP = tP;
        this.Action = A;
        this.id = "ExeKill"+tP.ToString()+"%";
    }

    public override void ActivateEffect(){
        this.Action.executeThreshhold = this.threshholdP;
    }
}

// WORKS!!!  == once
// ec15
// public class AOE : Effect
// {
//     int unitCount = 0;  // aoe hits the next 0 units

//     public AOE(int c, Action A){
//         this.unitCount = c;
//         this.Action = A;
//         this.id = "AOE"+c.ToString();
//     }

//     public override void ActivateEffect(){
//         this.Action.aoeON = true;
//         this.Action.aoeUnitCount = this.unitCount;
//     }
// }

// WORKS 2EZ MH: once, AOE: once  == once
// ec16
// public class OverFlow : Effect
// {

//     public OverFlow(Action A){
//         this.Action = A;
//         this.id = "OVRF";
//     }

//     public override void ActivateEffect(){
//         this.Action.overFlowON = true;
//     }
// }

// WORKS! MH: x, AOE: x  == mult
// ec17
public class LowHpDamageBonus : Effect
{
    private int max;
    private int baseDamage;

    public LowHpDamageBonus(int baseD, int max, Action A){
        this.baseDamage = baseD;
        this.max = max;
        this.Action = A;
        this.id = "LoHpDMG<"+this.max.ToString();
    }

    public override void ActivateEffect(){

        int PlayerHpP = (int)Mathf.Round(((float)this.Action.Player.healthPoints/this.Action.Player.maxHealthPoints) * 100);
        
        int increase = (int)Mathf.Round((this.max - this.baseDamage)/100);

        this.Action.damage += this.baseDamage + (int)Mathf.Round(increase*(100 - PlayerHpP));
        this.Action.bonusEffectDamage += this.baseDamage + (int)Mathf.Round(increase*(100 - PlayerHpP));

        this.Action.StartEffects.Enqueue(this);
    }
}

// WORKS! MH: once, AOE: once  == once
// ec18
public class MaxBaseDamage : Effect // IMPORTANT! musst be triggered before any other bonus dmg calulation
{

    public MaxBaseDamage(Action A){
        this.Action = A;
        this.id = "MaxBaseDMG";
    }

    public override void ActivateEffect(){

        this.Action.baseDamage = this.Action.maxBaseDamage;
        this.Action.damage = this.Action.maxBaseDamage;
    }
}

// WORKS! AOE: x  == mult
// ec19
// public class MultiHit : Effect
// {
//     int hitCount = 0;

//     public MultiHit(int c, Action A){
//         this.hitCount = c;
//         this.Action = A;
//         this.id = "MULT"+this.hitCount.ToString();
//     }

//     public override void ActivateEffect(){
//         this.Action.multiHitON = true;
//         this.Action.multiHitc = this.hitCount;
//     }
// }

// ecX
public abstract class UniqueEffect : Effect
{
    public override void ActivateEffect(){
        this.Action.uniqueEffect = true;
        this.Action.UniqueEffect = this;
    }
    // ecX_f1
    public abstract Task<int> ActivateUniqueEffect();
}