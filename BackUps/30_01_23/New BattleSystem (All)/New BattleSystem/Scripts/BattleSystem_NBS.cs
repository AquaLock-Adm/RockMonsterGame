/*
    Hauptstück des gesamten Battle ablaufs. ALLE prozesse die gestartet werden kommen entweder hier vorbei oder werden größtenteils auch von hier gestartet.
    Viele wichtige daten und referenzen werden hier gehalten und können per referenz auch von hier geholt werden.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BattleSystem_NBS : BattleSystem
{
    [Header("NBS Variables")]
    [Header("")]
    public GameObject EnemyPrefab_NBS;
    public GameObject PlayerPrefab_NBS;

#region Override Functions

    public override void RunTests(){
        // PlayerCharacter temp_p = this.PlayerPrefab_NBS.GetComponent<PlayerCharacter>();
        // Debug.Log("Testing Tick rate calculator on Unit.DepleteShield().");
        // temp_p.depletionTime = 255;
        // temp_p.DepleteShield(1000); // CHANGED to public for testing
    }

    public override void PreStartActions(){
        RunTests();
        // this.PauseMenu.ShowPauseMenu(true);
    }

    public override void SetupEverything(){
        // ImportStartSettingsFromGameHandler();
        SetupSystemComponents();
        // if(this.useWaveScript) SetupWaveScript();
        SetupAllUnits();
        // if(this.useDialogueScript) SetupDialogueScript();
    }

    public override void SetupSystemComponents(){
        SetBattleMenuHandlerReferenz();
        SetActionQueueReferenz();
        SetEnemySpawnerReferenz();
        // SetPauseMenuReferenz();
        // SetResultHandlerReferenz();
        // SetDialogueHandlerReferenz();

        SetupBattleMenuHandlerInnerReferenzes();
        SetupActionQueueInnerReferenzes();
        // SetupPauseMenuInnerReferenzes();
        // SetupResultHandlerInnerReferenzes();
        // SetupDialogueHandlerInnerReferenzes();
    }

    public override void SetupBattleMenuHandlerInnerReferenzes(){
        this.BattleMenuHandler.BattleSystem = this;
        this.BattleMenuHandler.ActionQueue = this.ActionQueue;
        // this.BattleMenuHandler.PauseMenu = this.PauseMenu;
        // this.BattleMenuHandler.DialogueHandler = this.DialogueHandler;  
    }

    public override void SetupAllUnits(){
        SetupEnemy_NBS();
        SetupPlayer_NBS();
    }

    public override void SetupPlayerAbilities(){
        if(this.Player.Weapon.Type != WeaponType.SWORD){
            Debug.LogError("NBS is build for Sword.");
        }

        this.Player.Abilities = this.Player.Weapon.GetCompleteMoveList();

        SetMissingReferencesWithPlayerAbilities();

        InitiateAbilityDecayArray();
    }

    public override void InitiateAbilityDecayArray(){
        this.AbilityDecayArray = new int[Player.Abilities.Count];
        foreach(Action A in this.Player.Abilities){
            this.AbilityDecayArray[A.abilityIndex] = 0;
        }
    }

    public override void CastAttack(int attackIndex){
        // no need to use CastComboAbility() in NBS
        
        // if(AttackIsComboAbility(attackIndex)) {
        //     CastComboAbility(this.Player.Abilities[attackIndex]);
        //     return;
        // }
        if(!IndexInBoundsOfList(attackIndex, this.Player.Abilities.Count)){
            Debug.LogError("Attack index is not in Bounds of Player.Abilities");
            return;
        }

        Action A = this.Player.Abilities[attackIndex].Copy();
        // CastAbility(A);
        ActionQueue.AddAction(A);
    }

    public override void CancelLastAction(bool gainApBack = true){

        if(ActionQueue.Actions.Count > 0) {

            // Bug: eine ult form ability wird gecancelt
            // CheckUltFormAbilityCanceled();

            Action A = ActionQueue.RemoveLastAction();                 // defined in ActionQueue.cs
            // if(gainApBack) this.Player.spentActionPoints -= A.apCost;
        }
    }

    public override async Task PlayerTurn(){
        if(this.EnemySpawner.stopSpawn) this.EnemySpawner.StartSpawn(); // why does isnt nextwave() called at beginning of game?
        this.state = BattleState.START;
        // await GainAp(this.Player.actionPointGain + this.Player.roundStartActionPointGain);
        await Task.Yield();
        // this.Player.ResetActionPointGain();
        this.state = BattleState.PLAYERTURN;
    }

    public override async void PassRound(){
        await ActionQueue.ExecuteAllActions();
        if(!ActionQueue.stopQueue) {
            InitiateAbilityDecayArray();
            await PlayerTurn();
        }
        else ActionQueue.stopQueue = false;
    }

    public override int AbilityDecay(Action ActionIn){
        for(int i = 0; i < this.AbilityDecayArray[ActionIn.abilityIndex]; i++){
            ActionIn.damage = (int)Mathf.Clamp(ActionIn.damage * 0.5f, 1.0f, float.MaxValue);
        }
        this.AbilityDecayArray[ActionIn.abilityIndex]++;
        return ActionIn.damage;
    }
#endregion

#region New Functions

    private void SetupPlayer_NBS(){
        if(this.PlayerPrefab_NBS != null){
            GameObject P_GO = Instantiate(this.PlayerPrefab_NBS);
            this.Player = P_GO.GetComponent<PlayerCharacter>();
            this.Player.Setup(this);

            SetupPlayerAbilities();
        }else Debug.Log("Player Prefab is not set correctly.");
    }

    private void SetupEnemy_NBS(){
        if(this.EnemyPrefab_NBS != null){
            List<GameObject> EnemiesToBeSpawned = new List<GameObject>();
            EnemiesToBeSpawned.Add(this.EnemyPrefab_NBS);
            this.EnemySpawner.Setup(this, EnemiesToBeSpawned);
        }else Debug.Log("Enemy Prefab is not set correctly.");
    }
#endregion
} // EOF