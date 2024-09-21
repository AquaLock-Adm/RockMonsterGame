using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerBattleResourceHandler : MonoBehaviour
{

    protected PlayerCharacter Player;

    private bool battleActive = true;
    private bool battleUpdateLoopRunning = false;

    [SerializeField] protected TextMeshProUGUI NameText;
    [SerializeField] protected Slider HpSlider;

    private const int TICK_TIME_MS = 50;

    private bool healthDepleteLoopRunning = false;
    private float lifeDrainPerTick = 1.0f;
    private float lifeDrained = 0.0f;

    private float currentHCPS = 0.0f;

    private bool healthRegenLoopRunning = false;

    private bool enemyDOTLoopRunning = false;
    private float enemyDOTPerTick = 1.0f;
    private float currentEnemyDOT = 0.0f;



    public virtual void Setup(PlayerCharacter Player, TextMeshProUGUI NameText, Slider HpSlider){
        this.Player = Player;

        this.NameText = NameText;
        this.HpSlider = HpSlider;

        if (Player.playerIsInFront)
        {
            this.NameText.text = Player.unitName;
            StartHealthDepleteLoop();
        }
        //else StartPlayerHealthRegenLoop(); 
        if(!this.battleUpdateLoopRunning) BattleUpdateLoop();
    } // changed in: PlayerResource_Tutorial.cs

    protected async void BattleUpdateLoop(){
        this.battleUpdateLoopRunning = true;

        while(!Player.deathTriggered && this.battleActive && Application.isPlaying){
            UpdateHUDElements();
            UpdateArmorStats();
            await Task.Yield();
        }

        this.battleUpdateLoopRunning = false;
    }


    public void UpdateHUDElements(){
        if(!Application.isPlaying) return;
        this.HpSlider.value = Player.healthPoints;
        this.HpSlider.maxValue = Player.maxHealthPoints;
    }

    private void UpdateArmorStats(){
        Player.GetArmor().healthPoints = Player.healthPoints;
    }

    public void BattleEnd()
    {
        this.battleActive = false;
        Destroy(this);
    }


    public virtual void DealDamage(int damage){
        Player.healthPoints = (int)Mathf.Max(0f, (float)( Player.healthPoints - damage ));

        if(Player.healthPoints <= 0) {
            Player.deathTriggered = true;
        }
    } // changed in: PlayerResource_Tutorial.cs

    public void Heal(int healAmount){
        Player.GetArmor().Repair(healAmount);
        Player.healthPoints = Player.GetArmor().healthPoints;
        if(Player.deathTriggered) {
            Player.deathTriggered = false;
            StartHealthDepleteLoop();
        }
        UpdateHUDElements();
    }

    public void HealPercentual(int heal_p){
        int healAmount = (int)Mathf.Round( ((float)Player.maxHealthPoints/100f)*(float)heal_p );
        Heal(healAmount);
    }



#region Health Updates
    public void StartHealthDepleteLoop(){
        if(this.healthDepleteLoopRunning){
            Debug.Log("Health Deplete Loop is already running!");
            return;
        }
        CalcCurrentHCPS();
        this.lifeDrained = 0.0f;
        this.healthDepleteLoopRunning = true;
        HealthDepleteLoop();
    }

    public void CalcCurrentHCPS(){
        Weapon PW = Player.GetWeapon();
        float hcps = PW.healthCostPerSecond;
        int levelOverRamp = 0;

        if( (levelOverRamp = Player.GetCurrentComboLevel() - PW.lifeDrainIncreaseStartLevel) > 0 ){
            /*
                increase per level:
                    2 ^ (levelOverRamp-1) * stdIncrease
                    stdIncrease = (hcpsmax - hcps) / 2^(7 - strtLVL -1)

                    for maria lvl1: stdIncrease = (4.9f - 0.8f) / 2^(7-3-1) = 4.1f / 2^3 = 0.5125f

                    cmbLVL4 : 2 ^ 0 * 0.5125f => 0.8f + 0.5125f = 1.3125f
                    cmbLVL5 : 2 ^ 1 * 0.5125f => 0.8f + 1.025f  = 1.825f
                    cmbLVL6 : 2 ^ 2 * 0.5125f => 0.8f + 2.05f   = 2.85f
                    cmbLVL7 : 2 ^ 3 * 0.5125f => 0.8f + 4.1f    = 4.9f

            */

            float stdIncrease = (PW.HCPSmax - hcps) / Mathf.Pow( 2.0f, 7.0f - 1.0f - (float)PW.lifeDrainIncreaseStartLevel );
            float drainIncrease = Mathf.Pow( 2.0f, (float)levelOverRamp - 1.0f ) * stdIncrease;
            hcps += drainIncrease;
        }

        this.currentHCPS = hcps;
        // Debug.Log("New HCPS: "+this.currentHCPS.ToString());
    }

    private async void HealthDepleteLoop(){
        while(Player.healthPoints > 0 && Application.isPlaying){

            if(Player.state == PlayerState.PLAYERTURN){

                CalcLifeDrainPerTick();
    
                if(Player.GetCurrentComboLevel() >= 5) this.lifeDrained += this.lifeDrainPerTick*2;
                else this.lifeDrained += this.lifeDrainPerTick;

                int lifeLoss = 0;
                if(this.lifeDrained >= 1.0f){
                    lifeLoss = (int)this.lifeDrained;
                    this.lifeDrained -= (float)lifeLoss;
                }
                Player.healthPoints -= lifeLoss;

                if(Player.healthPoints <= 0) {
                    Player.deathTriggered = true;
                    await Player.Death();
                    continue;
                }

                await Task.Delay(TICK_TIME_MS);
            }else await Task.Yield();
        }
    }

    private void CalcLifeDrainPerTick(){ 
        this.lifeDrainPerTick = (float)( this.currentHCPS / (1000.0f/(float)TICK_TIME_MS)); 
    }

    public void StartPlayerHealthRegenLoop()
    {
        if (this.healthRegenLoopRunning) return;

        this.healthRegenLoopRunning = true;
        HealthRegenLoop();
    }

    private async void HealthRegenLoop()
    {
        float healthRegened = 0.0f;
        float currentHealthRegen = Player.GetArmor().healthRegen;

        float lastCallTime = 0.0f;
        float deltaTime = 1.0f;

        while (Player.healthPoints > 0 && Application.isPlaying)
        {
            deltaTime = Time.time - lastCallTime;
            healthRegened += currentHealthRegen * deltaTime;

            if(healthRegened > 1.0f)
            {
                int healthGain = (int)healthRegened;
                healthRegened -= (float)healthGain;
                Player.healthPoints = (int)Mathf.Min((float)Player.GetArmor().maxHealthPoints, (float)(Player.healthPoints + healthGain));
            }

            lastCallTime = Time.time;
            await Task.Yield();
        }
    }
#endregion



    #region Enemy DOT
    public void StartEnemyDOTLoop(){
        if(this.enemyDOTLoopRunning){
            Debug.Log("Enemy DOT Loop is already running!");
            return;
        }

        this.currentEnemyDOT = 0.0f;
        this.enemyDOTLoopRunning = true;
        EnemyDOTLoop();
    }

    private async void EnemyDOTLoop(){
        if(Player.GetWeapon().enemyDamagePerSecond <= 0f) return;

        while(Player.healthPoints > 0){
            if(Player.GetCurrentComboLevel() > 5
                && (Player.state == PlayerState.PLAYERTURN || Player.state == PlayerState.QUEUE)){

                CalcDOTPerTick();

                this.currentEnemyDOT += this.enemyDOTPerTick;
                if(this.currentEnemyDOT >= 1f){
                    int dot = (int)this.currentEnemyDOT;
                    this.currentEnemyDOT -= (float)dot;
                    Enemy CurrentEnemy = Player.GetCurrentEnemy();
                    if(CurrentEnemy != null){
                        CurrentEnemy.DealWeaponDOT(dot);
                    }
                }
                await Task.Delay(TICK_TIME_MS);
            }else await Task.Yield();
        }
    }

    private void CalcDOTPerTick(){
        this.enemyDOTPerTick = Player.GetWeapon().enemyDamagePerSecond / ( 1000.0f/(float)TICK_TIME_MS );
    }
#endregion
}
