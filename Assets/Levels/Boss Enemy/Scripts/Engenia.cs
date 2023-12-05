using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Engenia : Enemy
{
    [Header("Engenia Stats")]
    [SerializeField] private int maxBlockStamina = 20;
    [SerializeField] private int blockStamina = 20;

    [SerializeField] private bool freeAttackTurn = false;

    [SerializeField] private int currentBlockStringLength = 1;

    private void CreateBlockString(){
        this.shieldModeIndex = 0;
        this.CurrentShieldModes = new List<ShieldMode>();
        if(!this.freeAttackTurn){
            for(int i=0; i < this.currentBlockStringLength; i++){
                AddShieldToCurrentShieldModes();
            }
            this.nextShield = this.CurrentShieldModes[0];
            SetShieldVisualizer(true);
        }
    }

    private void AddShieldToCurrentShieldModes(){
        this.CurrentShieldModes.Add(ShieldMode.ANY);
    }               // Todo

    private void CreateAttackString(){

    }

    private void CheckFreeAttackTurn(){
        if(!this.freeAttackTurn && this.blockStamina <= 0){
            Debug.Log("Activate Free Attack Turn");
            this.freeAttackTurn = true;
            this.blockStamina = maxBlockStamina;
        }else if(this.freeAttackTurn){
            this.freeAttackTurn = false;
        }
    }

    private void RaiseBlockStringLength(){
        if(!this.freeAttackTurn) this.currentBlockStringLength++;
    }



#region overrides
    public override void BattleSetup(){
        this.blockStamina = maxBlockStamina;
        this.currentBlockStringLength = 1;

        this.heatGainedFromBlocks = 0;
        this.speedGainedFromPerfectAttacks = 0;
        this.speedGainedFromPerfectBlocks = 0;
        this.speedLostFromBadBlocks = 0;

        base.BattleSetup();

        SetShieldVisualizer(false);
    }

    public override void PassRound(){
        if(BattleSystem.Player.defendModeActive){
            CreateAttackString();
        }else{
            CreateBlockString();
        }
    }

    public override void SwitchBattleModes(bool playerInDefendMode){

        SetAttackVisualizer(false);
        SetShieldVisualizer(false);

        if(playerInDefendMode) {
            ResetAttackSequence();
        }else{
            CheckFreeAttackTurn();
            this.enemyAttacksExecuted++;
        }
    }


    protected override async Task<bool> HandleAttackAction(Action A){
        bool addHeat = false;
        bool hitDetect = false;

        int damageDealt = await A.NormalExecute();
        if(BattleSystem.Player.state == PlayerState.QUEUE){
            if(this.shieldModeIndex < this.CurrentShieldModes.Count){
                addHeat = CheckShieldMode(A.AbilityType);
                if(this.nextShield == ShieldMode.NONE && this.healthPoints <= 0){
                    this.deathTriggered = true;
                }
            }else {
                hitDetect = true;
                DealDamage(A.damage);
                int lifeGain = (int)Mathf.Ceil(A.damage * ((float)A.Player.GetLifeSteal()/100.0f));
                A.Player.Heal(lifeGain);
                addHeat = true;
            }
        }

        if(hitDetect){
            A.TriggerOnHit();
            RaiseBlockStringLength();
        }else if(addHeat) {
            if(!this.freeAttackTurn) this.blockStamina--;
            // Debug.Log("Current Block Stamina: "+this.blockStamina.ToString()+"/"+this.maxBlockStamina.ToString());
        }

        if(this.deathTriggered) A.TriggerOnDeath();
        this.critTaken = false;
        return addHeat;
    }

    protected override Color GetMiniShieldColor(int shieldIndex){
        return new Color(this.AnyShieldColor.r,this.AnyShieldColor.g,this.AnyShieldColor.b);
    }

    protected override bool CheckShieldMode(AbilityType aType){
        bool addHeat = false;
        if(this.CurrentShieldModes[this.shieldModeIndex] == ShieldMode.ANY || (int)aType == (int)this.CurrentShieldModes[this.shieldModeIndex]){
            this.shieldModeIndex++;
            addHeat = true;
        }
        if(this.shieldModeIndex < this.CurrentShieldModes.Count) {
            this.nextShield = this.CurrentShieldModes[this.shieldModeIndex];
            // this.CurrentDiscoveredShields[this.shieldModeIndex] = this.CurrentShieldModes[this.shieldModeIndex];
        }
        else this.nextShield = ShieldMode.NONE;
        SetShieldVisualizer(true);
        return addHeat;
    }
#endregion
}