using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("GameObject References")]
    public BattleSystem BattleSystem;

    public GameObject PauseMenuScreen;
    public GameObject StartMenuScreen;
    public GameObject DarkFilter;

    public Text MainText;

    public Image StartButtonW;
    public Image StartButtonA;
    public Image StartButtonS;
    public Image StartButtonD;

    private bool startButtonWPressed = false;
    private bool startButtonAPressed = false;
    private bool startButtonSPressed = false;
    private bool startButtonDPressed = false;

    [SerializeField] private Text NextWaveText;

    [SerializeField] private Slider PauseHpSlider;
    [SerializeField] private TextMeshProUGUI PauseHpText;

    [SerializeField] public Color StartButtonPressedColor;
    private Color StartButtonBaseColor;

    [Header("Button Bools")]
    public GameObject ContinueButton;
    public GameObject LeaveButton;
    public GameObject NextStageButton;

    public bool showContinueButton = true;
    public bool showLeaveButton = false;
    public bool showNextStageButton = false;

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
    public string stageOverText;
    [TextArea(5,8)]
    public string afterBossWaveText;
    [TextArea(5,8)]
    public string gameWonText;
    [TextArea(5,8)]
    public string gameOverText;

    public void Setup(BattleSystem BS){
        this.BattleSystem = BS;
        this.StartButtonBaseColor = this.StartButtonW.color;
        ShowPauseMenu(true);
    }
    
    public void ShowPauseMenu(bool turnOn){
        if(!Application.isPlaying) return;
        // Debug.Log("ShowPauseMenu() called");
        this.DarkFilter.SetActive(turnOn);

        if(turnOn){
            switch(this.BattleSystem.state){
                case BattleState.SETUP:
                    this.StartMenuScreen.SetActive(turnOn);
                    this.PauseMenuScreen.SetActive(false);
                    SetupStartButtonTest();
                break;
                case BattleState.WAVEOVER:
                    this.PauseMenuScreen.SetActive(turnOn);
                    if(!this.BattleSystem.inBossWave){
                        LoadText(this.waveOverText);
                        this.showContinueButton = true;
                        this.showLeaveButton = true;
                        this.showNextStageButton = false;
                    }else{
                        if(BattleSystem.finalStageReached){
                            LoadText(this.gameWonText);
                            this.showLeaveButton = true;
                            this.showContinueButton = false;
                            this.showNextStageButton = false;
                        }else if(BattleSystem.afterFinalWave){
                            LoadText(this.afterBossWaveText);
                            this.showLeaveButton = true;
                            this.showContinueButton = true;
                            this.showNextStageButton = true;
                        }else {
                            LoadText(this.stageOverText);
                            this.showLeaveButton = true;
                            this.showContinueButton = true;  
                            this.showNextStageButton = true;
                        }
                    }
                break;
                case BattleState.PLAYERDIED:
                    this.PauseMenuScreen.SetActive(turnOn);
                    LoadText(this.gameOverText);
                    this.showContinueButton = false;
                    this.showLeaveButton = true; 
                    this.showNextStageButton = false;
                break;
                default:
                    Debug.LogError("Unexpected Pause State!");
                    Debug.Log(this.BattleSystem.state);
                break;
            }

            this.ContinueButton.SetActive(this.showContinueButton);
            this.LeaveButton.SetActive(this.showLeaveButton); 
            this.NextStageButton.SetActive(this.showNextStageButton);
            if(BattleSystem.state != BattleState.SETUP){
                SetupNextWaveText();
                SetupPauseHealthBar(); 
            }      
        } else this.PauseMenuScreen.SetActive(turnOn);
    }

    private void LoadText(string text){
        MainText.text = text;
    }

    private void SetupStartButtonTest(){
        this.startButtonWPressed = false;
        this.startButtonAPressed = false;
        this.startButtonSPressed = false;
        this.startButtonDPressed = false;


        this.StartButtonW.color = this.StartButtonBaseColor;
        this.StartButtonA.color = this.StartButtonBaseColor;
        this.StartButtonS.color = this.StartButtonBaseColor;
        this.StartButtonD.color = this.StartButtonBaseColor;
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

    // changed in PauseMenu_Tutorial.cs
    public virtual void ContinueGame(){
        if(BattleSystem.state == BattleState.SETUP){
            ShowPauseMenu(false);
            this.StartMenuScreen.SetActive(false);
            BattleSystem.NextWave();
        }else if(BattleSystem.state == BattleState.WAVEOVER){
            ShowPauseMenu(false);
            BattleSystem.NextWave();
        }else{
            Debug.Log("Continued the game from some other pause menu!");
            ShowPauseMenu(false);
            BattleSystem.state = BattleState.PLAYERTURN;
        }
    }

    // changed in PauseMenu_Tutorial.cs
    public virtual void LeaveGame(){
        ShowPauseMenu(false);
        //BattleSystem.pauseTimer = true;
        BattleSystem.ResultHandler.ShowResult();
    }

    private void SetupNextWaveText(){
        this.NextWaveText.text = BattleSystem.GetNextWaveSize().ToString();
    }

    private void SetupPauseHealthBar(){
        this.PauseHpText.text = BattleSystem.Player.GetHealthPoints().ToString() + "/" + BattleSystem.Player.GetMaxHealthPoints().ToString();
        this.PauseHpSlider.value = BattleSystem.Player.GetHealthPoints();
        this.PauseHpSlider.maxValue = BattleSystem.Player.GetMaxHealthPoints();
    }
}
