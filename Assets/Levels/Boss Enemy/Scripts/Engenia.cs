using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

public class Engenia : Enemy
{
    [Header("Engenia Stats")]

    [SerializeField] private bool freeAttackTurn = false;

    [SerializeField] private int currentBlockStringLength = 1;

    [Header("Enginia AI")]
    [SerializeField] private int useLightBlockChance_p = 34;
    [SerializeField] private int useHeavyBlockChance_p = 33;
    // [SerializeField] private int useSpecialBlockChance_p = 33;

    private int lightBlockScore;
    private int heavyBlockScore;
    private int specialBlockScore;

    [SerializeField] private int lightReceivedCount;
    [SerializeField] private int heavyReceivedCount;
    [SerializeField] private int specialReceivedCount;
    [SerializeField] private float totalAttacksReceivedBias = 1.1f;

    [SerializeField] private List<AbilityType> AttacksReceivedList;
    [SerializeField] private int ARListSize = 20;
    [SerializeField] private float recentAttacksReceivedBias = 1.7f;

    [SerializeField] private float randomFactorBias = 0.8f;



    private void CreateBlockString(){
        this.shieldModeIndex = 0;
        this.CurrentShieldModes = new List<ShieldMode>();
        if(!this.freeAttackTurn){
            if(this.currentBlockStringLength <= 1){
                this.CurrentShieldModes.Add(ShieldMode.ANY);
            }else{
                for(int i=0; i < this.currentBlockStringLength; i++){
                    ShieldMode nextShield = RollNextShield();
                    this.CurrentShieldModes.Add(nextShield);
                }
            }
            
            this.nextShield = this.CurrentShieldModes[0];
            SetShieldVisualizer(true);
        }
    }

    private ShieldMode RollNextShield(){
        int ran = Random.Range(1,101);
        if(ran <= this.useLightBlockChance_p){
            return ShieldMode.LIGHT;
        }else if(ran <= this.useLightBlockChance_p + this.useHeavyBlockChance_p){
            return ShieldMode.HEAVY;
        }else{
            return ShieldMode.SPECIAL;
        }
    }               // Todo

    private void CreateAttackString(){

    }

    private void CalcBlockChances(){
        // this.useLightBlockChance_p = 34;
        // this.useHeavyBlockChance_p = 33;
        // this.useSpecialBlockChance_p = 33;
        this.lightBlockScore = 0;
        this.heavyBlockScore = 0;
        this.specialBlockScore = 0;
        CalcTotalBlockScore();
        CalcRecentBlockScore();
        CalcRandomBlockScore();

        Debug.Log("Total Scores: "+this.lightBlockScore.ToString()+"/"+this.heavyBlockScore.ToString()+"/"+this.specialBlockScore.ToString());

        if(lightBlockScore <= heavyBlockScore && lightBlockScore <= specialBlockScore){
            Debug.Log("Adding LightBlock");
            UpdateBlockData(AbilityType.LIGHT);
        }else if(heavyBlockScore <= lightBlockScore && heavyBlockScore <= specialBlockScore){
            Debug.Log("Adding HeavyBlock");
            UpdateBlockData(AbilityType.HEAVY);
        }else{
            Debug.Log("Adding SpecialBlock");
            UpdateBlockData(AbilityType.SPECIAL);
        }
    }

    private void UpdateBlockData(AbilityType A){
        if(this.AttacksReceivedList.Count >= this.ARListSize){
            this.AttacksReceivedList.RemoveAt(0);
        }
        this.AttacksReceivedList.Add(A);

        if(A == AbilityType.LIGHT){
            this.lightReceivedCount++;
        }else if(A == AbilityType.HEAVY){
            this.heavyReceivedCount++;
        }else{
            this.specialReceivedCount++;
        }
    }

    private void CalcRandomBlockScore(){
        int lightScore = 0;
        int heavyScore = 0;
        int specialScore = 0;
        for(int i = 0; i<20; i++){
            int ran = Random.Range(1,101);

            if(ran <= 34) lightScore++;
            else if(ran <= 67) heavyScore++;
            else specialScore++;
        }

        lightScore *= 5;
        heavyScore *= 5;
        specialScore *= 5;

        Debug.Log("Random Res: "+lightScore.ToString()+"/"+heavyScore.ToString()+"/"+specialScore.ToString());

        this.lightBlockScore = (int)Mathf.Round( (float)this.lightBlockScore + ((float)lightScore * this.randomFactorBias) );
        this.heavyBlockScore = (int)Mathf.Round( (float)this.heavyBlockScore + ((float)heavyScore * this.randomFactorBias) );
        this.specialBlockScore = (int)Mathf.Round( (float)this.specialBlockScore + ((float)specialScore * this.randomFactorBias) );

    }

    private void CalcTotalBlockScore(){

        int sum = lightReceivedCount + heavyReceivedCount + specialReceivedCount;

        if(sum > 0){
            int lightScore = (int)Mathf.Clamp( Mathf.Round( ((float)lightReceivedCount/(float)sum) * 100.0f ), 1.0f, 98.0f );
            int heavyScore = (int)Mathf.Clamp( Mathf.Round( ((float)heavyReceivedCount/(float)sum) * 100.0f ), 1.0f, 98.0f );
            int specialScore = 100 - lightScore - heavyScore;
            Debug.Log("Total Res: "+lightScore.ToString()+"/"+heavyScore.ToString()+"/"+specialScore.ToString());
            
            this.lightBlockScore = (int)Mathf.Round( (float)this.lightBlockScore + ((float)lightScore * this.totalAttacksReceivedBias) );
            this.heavyBlockScore = (int)Mathf.Round( (float)this.heavyBlockScore + ((float)heavyScore * this.totalAttacksReceivedBias) );
            this.specialBlockScore = (int)Mathf.Round( (float)this.specialBlockScore + ((float)specialScore * this.totalAttacksReceivedBias) );
        }
    }

    private void CalcRecentBlockScore(){
        int baseLightChance = 34;
        int baseHeavyChance = 33;
        int baseSpecialChance = 33;

        // Count Abilities in Last 10 Received

        int lCount = 0;
        int hCount = 0;
        int sCount = 0;

        foreach(AbilityType A in this.AttacksReceivedList){
            if(A == AbilityType.LIGHT) lCount++;
            else if(A == AbilityType.HEAVY) hCount++;
            else if(A == AbilityType.SPECIAL) sCount++;
        }

        // Debug.Log("Recent Counts: "+lCount.ToString()+"/"+hCount.ToString()+"/"+sCount.ToString());

        // Calc new Light Score
        baseLightChance += (int)Mathf.Max(lCount-hCount, 0.0f) * 20;
        baseLightChance += (int)Mathf.Max(lCount-sCount, 0.0f) * 20;

        // Calc new Heavy Score
        baseHeavyChance += (int)Mathf.Max(hCount-sCount, 0.0f) * 20;
        baseHeavyChance += (int)Mathf.Max(hCount-lCount, 0.0f) * 20;

        // Calc new Special Score
        baseSpecialChance += (int)Mathf.Max(sCount-hCount, 0.0f) * 20;
        baseSpecialChance += (int)Mathf.Max(sCount-lCount, 0.0f) * 20;

        // Sum up and redistribute new Chances
        int sum = baseLightChance + baseHeavyChance + baseSpecialChance;

        int lightScore = (int)Mathf.Round( ((float)baseLightChance/(float)sum) * 100.0f );
        int heavyScore = (int)Mathf.Round( ((float)baseHeavyChance/(float)sum) * 100.0f );
        int specialScore = 100 - lightScore - heavyScore;

        Debug.Log("Recent Res: "+lightScore.ToString()+"/"+heavyScore.ToString()+"/"+specialScore.ToString());
            
        this.lightBlockScore = (int)Mathf.Round( (float)this.lightBlockScore + ((float)lightScore * this.recentAttacksReceivedBias) );
        this.heavyBlockScore = (int)Mathf.Round( (float)this.heavyBlockScore + ((float)heavyScore * this.recentAttacksReceivedBias) );
        this.specialBlockScore = (int)Mathf.Round( (float)this.specialBlockScore + ((float)specialScore * this.recentAttacksReceivedBias) );
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

    private void RunTests(){
        Debug.Log("Running Tests...");
        AttacksReceivedList.Add(AbilityType.LIGHT);
        AttacksReceivedList.Add(AbilityType.LIGHT);
        AttacksReceivedList.Add(AbilityType.LIGHT);
        // AttacksReceivedList.Add(AbilityType.LIGHT);
        // AttacksReceivedList.Add(AbilityType.LIGHT);
        // AttacksReceivedList.Add(AbilityType.LIGHT);

        AttacksReceivedList.Add(AbilityType.HEAVY);
        AttacksReceivedList.Add(AbilityType.HEAVY);
        // AttacksReceivedList.Add(AbilityType.HEAVY);

        // AttacksReceivedList.Add(AbilityType.SPECIAL);

        this.lightReceivedCount = 6;
        this.heavyReceivedCount = 2;
        this.specialReceivedCount = 10;

        for(int i=0;i<5;i++){
            CalcBlockChances();
        }
    }

    /*
    Data to Track:
        For Defending:
        > Count of Light, Heavy, Special Attack received
        > Last 20 Attacks received
        > Count LHS of Last 20 Attacks Received
        > Count Followup attacks for LHS
        > Last 10 Blocks used
        > Successrate of each Block
        > Biases for each resulting chance of success
        For Attacking:
        > Last 10 Attacks used
        > Successrate for each attack
        > Switchup of Attacks in Pattern - timing
        > Last 10 Blocks received
    */

#region overrides
    public override void BattleSetup(){
        RunTests();
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
        base.PassRound();
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