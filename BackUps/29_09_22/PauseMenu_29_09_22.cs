using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("GameObject References")]
    public BattleSystem BattleSystem;

    public GameObject PauseMenuScreen;
    public GameObject DarkFilter;

    public Text MainText;
    public Text WeaponStatusText;
    public Text WeaponDurabilityText;
    public Text ItemsLeftText;

    [Header("Pause Menu Texts")]

    [TextArea(5,8)]
    public string startingText;
    [TextArea(5,8)]
    public string pauseText;
    [TextArea(5,8)]
    public string bailOutText;
    [TextArea(5,8)]
    public string waveOverText;
    [TextArea(5,8)]
    public string gameOverText;

    [Header("Button Bools")]
    public GameObject ContinueButton;
    public GameObject LeaveButton;
    public GameObject RepairButton;
    public bool showContinueButton = true;
    public bool showLeaveButton = false;
    public bool showRepairButton = false;

    void Start(){
        ShowPauseMenu(true);
    }
    
    public void ShowPauseMenu(bool turnOn){
        DarkFilter.SetActive(turnOn);
        PauseMenuScreen.SetActive(turnOn);

        if(turnOn){
            //BattleSystem.Enemy.StopAttack();
            switch(BattleSystem.state){
                case BattleState.SETUP:
                    LoadText(startingText);
                    showContinueButton = true;
                    showLeaveButton = false;
                    ShowWeaponStatus(false);
                break;
                case BattleState.WAVEOVER:
                    LoadText(waveOverText);
                    showContinueButton = true;
                    showLeaveButton = true;
                    if(BattleSystem.Player.repairItemCount > 0)ShowWeaponStatus(true);            // if the player has an item
                break;
                case BattleState.PLAYERDIED:
                    LoadText(gameOverText);
                    showContinueButton = false;
                    showLeaveButton = true;
                    ShowWeaponStatus(false);
                break;
                case BattleState.BAILOUT:
                    LoadText(bailOutText);
                    showContinueButton = false;
                    showLeaveButton = true;
                    ShowWeaponStatus(false);
                break;
                default:
                    LoadText(pauseText);
                    showContinueButton = true;
                    showLeaveButton = true;
                    ShowWeaponStatus(false);
                break;
            }

            ContinueButton.SetActive(showContinueButton);
            LeaveButton.SetActive(showLeaveButton);        
        } else ShowWeaponStatus(false);
    }

    private void LoadText(string text){
        MainText.text = text;
    }

    private void ShowWeaponStatus(bool on){
        WeaponStatusText.gameObject.SetActive(on);
        WeaponDurabilityText.gameObject.SetActive(on);
        ItemsLeftText.gameObject.SetActive(on);
        showRepairButton = on;
        RepairButton.SetActive(on);

        if(on) {
            WeaponStatusText.text = BattleSystem.Player.Weapon.durability + "/"+ BattleSystem.Player.Weapon.maxDurability;
            WeaponDurabilityText.text = BattleSystem.Player.Weapon.weaponStatus;
            ItemsLeftText.text = "Left: " + BattleSystem.Player.repairItemCount;
        }
    }

    public async void ContinueGame(){
        if(BattleSystem.state == BattleState.SETUP){
            ShowPauseMenu(false);
            BattleSystem.Player.actionPointGain += BattleSystem.Player.startActionPoints;
            await BattleSystem.PlayerTurn();
        }else if(BattleSystem.state == BattleState.WAVEOVER){
            ShowPauseMenu(false);
            BattleSystem.NextWave();
        }else{
            Debug.Log("Continued the game from some other pause menu!");
            ShowPauseMenu(false);
            BattleSystem.state = BattleState.PLAYERTURN;
        }
    }

    public void RepairWeapon(){
        if(BattleSystem.state == BattleState.WAVEOVER){
            BattleSystem.RepairWeapon();
            WeaponStatusText.text = BattleSystem.Player.Weapon.durability + "/"+ BattleSystem.Player.Weapon.maxDurability;
            WeaponDurabilityText.text = BattleSystem.Player.Weapon.weaponStatus;
            ItemsLeftText.text = "Left: " + BattleSystem.Player.repairItemCount;
            showRepairButton = false;
            RepairButton.SetActive(false);
        }
    }

    public void LeaveGame(){
        ShowPauseMenu(false);
        //BattleSystem.pauseTimer = true;
        BattleSystem.ResultHandler.ShowResult();
    }
}
