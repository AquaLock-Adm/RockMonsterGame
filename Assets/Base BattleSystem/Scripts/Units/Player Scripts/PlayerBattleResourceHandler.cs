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

    [SerializeField] protected TextMeshProUGUI NameText;
    [SerializeField] protected Slider HpSlider;

    private const int TICK_TIME_MS = 50;

    private bool healthDepleteLoopRunning = false;
    private float lifeDrainPerTick = 1.0f;
    private float lifeDrained = 0.0f;

    private bool enemyDOTLoopRunning = false;
    private float enemyDOTPerTick = 1.0f;
    private float currentEnemyDOT = 0.0f;



    public virtual void Setup(PlayerCharacter Player, TextMeshProUGUI NameText, Slider HpSlider){
        this.Player = Player;

        this.NameText = NameText;
        this.NameText.text = Player.unitName;

        this.HpSlider = HpSlider;

        StartHealthDepleteLoop();
        if(Player.GetWeapon().actionsPerRound > 5) StartEnemyDOTLoop();
        BattleUpdateLoop();
    } // changed in: PlayerResource_Tutorial.cs

    protected async void BattleUpdateLoop(){
        while(!Player.deathTriggered && this.battleActive && Application.isPlaying){
            UpdateHUDElements();
            UpdateArmorStats();
            await Task.Yield();
        }
    }


    public void UpdateHUDElements(){
        if(!Application.isPlaying) return;
        this.HpSlider.value = Player.healthPoints;
        this.HpSlider.maxValue = Player.maxHealthPoints;
    }

    public void BattleEnd(){
        this.battleActive = false;
        Destroy(this);
    }

    private void UpdateArmorStats(){
        Player.GetArmor().healthPoints = Player.healthPoints;
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



#region Health Drain
    public void StartHealthDepleteLoop(){
        if(this.healthDepleteLoopRunning){
            Debug.Log("Health Deplete Loop is already running!");
            return;
        }
        this.lifeDrained = 0.0f;
        this.healthDepleteLoopRunning = true;
        HealthDepleteLoop();
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
        this.lifeDrainPerTick = (float)(Player.GetWeapon().healthCostPerSecond / (1000.0f/(float)TICK_TIME_MS)); 
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
