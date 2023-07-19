using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [Header("GameObject References")]
    public BattleSystem BattleSystem;

    public GameObject PauseMenuScreen;
    public GameObject StartMenuScreen;
    public GameObject DarkFilter;

    public Text MainText;
    public Text WeaponStatusText;
    public Text WeaponDurabilityText;
    public Text ItemsLeftText;

    public Image StartButtonW;
    public Image StartButtonA;
    public Image StartButtonS;
    public Image StartButtonD;

    private bool startButtonWPressed = false;
    private bool startButtonAPressed = false;
    private bool startButtonSPressed = false;
    private bool startButtonDPressed = false;

    [SerializeField] public Color StartButtonPressedColor;

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

        if(turnOn){
            //BattleSystem.Enemy.StopAttack();
            switch(BattleSystem.state){
                case BattleState.SETUP:
                    this.StartMenuScreen.SetActive(turnOn);
                    PauseMenuScreen.SetActive(false);
                break;
                case BattleState.WAVEOVER:
                    PauseMenuScreen.SetActive(turnOn);
                    LoadText(waveOverText);
                    showContinueButton = true;
                    showLeaveButton = true;
                    if(BattleSystem.Player.repairItemCount > 0)ShowWeaponStatus(true);            // if the player has an item
                break;
                case BattleState.PLAYERDIED:
                    PauseMenuScreen.SetActive(turnOn);
                    LoadText(gameOverText);
                    showContinueButton = false;
                    showLeaveButton = true;
                    ShowWeaponStatus(false);
                break;
                case BattleState.BAILOUT:
                    PauseMenuScreen.SetActive(turnOn);
                    LoadText(bailOutText);
                    showContinueButton = false;
                    showLeaveButton = true;
                    ShowWeaponStatus(false);
                break;
                default:
                    PauseMenuScreen.SetActive(turnOn);
                    LoadText(pauseText);
                    showContinueButton = true;
                    showLeaveButton = true;
                    ShowWeaponStatus(false);
                break;
            }

            ContinueButton.SetActive(showContinueButton);
            LeaveButton.SetActive(showLeaveButton);        
        } else {
            PauseMenuScreen.SetActive(turnOn);
            ShowWeaponStatus(false);
        }
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

    public void StartButtonTest(char In){
        switch(In){
            case 'W':
                this.StartButtonW.color = this.StartButtonPressedColor;
                this.startButtonWPressed = true;
            break;

            case 'S':
                this.StartButtonS.color = this.StartButtonPressedColor;
                this.startButtonSPressed = true;
            break;

            case 'A':
                this.StartButtonA.color = this.StartButtonPressedColor;
                this.startButtonAPressed = true;
            break;

            case 'D':
                this.StartButtonD.color = this.StartButtonPressedColor;
                this.startButtonDPressed = true;
            break;
        }

        if(AllStartButtonsPressed()) ContinueGame();
    }

    private bool AllStartButtonsPressed(){
        return this.startButtonWPressed &&
                this.startButtonAPressed &&
                this.startButtonSPressed &&
                this.startButtonDPressed;
    }

    public async void ContinueGame(){
        if(BattleSystem.state == BattleState.SETUP){
            ShowPauseMenu(false);
            this.StartMenuScreen.SetActive(false);
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
