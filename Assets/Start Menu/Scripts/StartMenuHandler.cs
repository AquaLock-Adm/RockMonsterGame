using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StartMenuHandler : StandartMenuHandler
{
    [Header("Start Menu Data")]
    [SerializeField] private int buttonIndex;
    [SerializeField] private List<StartMenuButton> buttonList = new List<StartMenuButton>();

    public override void StartSetup(GameHandler GH){
        base.StartSetup(GH);

        foreach(StartMenuButton B in this.buttonList){
            B.UnHoverMenuButton();
        }

        this.buttonIndex = 0;
        this.buttonList[this.buttonIndex].HoverMenuButton();
    }

    protected override void CheckPlayerInput(){
        if( Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ){
            OptionDown();
        }else if( Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ){
            OptionUp();
        }else if( Input.GetKeyDown(KeyCode.Space) ){
            SelectOption();
        }
    }

    private void OptionDown(){
        GameHandler.PlaySwitchMenuOptionSound();
        int lastIndex = this.buttonIndex;
        this.buttonIndex = (this.buttonIndex+1) % this.buttonList.Count;

        this.buttonList[lastIndex].UnHoverMenuButton();
        this.buttonList[this.buttonIndex].HoverMenuButton();
    }

    private void OptionUp(){
        GameHandler.PlaySwitchMenuOptionSound();
        int lastIndex = this.buttonIndex;
        if(this.buttonIndex > 0) this.buttonIndex--;
        else this.buttonIndex = this.buttonList.Count-1;

        this.buttonList[lastIndex].UnHoverMenuButton();
        this.buttonList[this.buttonIndex].HoverMenuButton();
    }

    private void SelectOption(){
        GameHandler.PlaySelectMenuOptionSound();
        switch(this.buttonIndex){
            case 0:
                StartGame();
            break;

            case 1:
                StartTutorial();
            break;

            case 2:
                QuitGame();
            break;

            default:
                Debug.LogError("Error: Button Index Out Of Bounds!");
            break;
        }
    }

    private void StartGame(){
        GameHandler.LoadMainMenu();
    }

    private void StartTutorial(){
        GameHandler.LoadTutorial();
    }

    private void QuitGame(){
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
