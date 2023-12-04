using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageSelectHandler : MonoBehaviour
{
    [SerializeField] private GameObject GameHandlerPrefab;
    private GameHandler GameHandler;

    [SerializeField] private GameObject StageOptionPrefab;
    [SerializeField] private GameObject MenuOptions_GO;
    [SerializeField] private int buttonIndex;

    [SerializeField] private List<StartMenuButton> buttonList = new List<StartMenuButton>();


    void Awake(){
        GameObject GHGO = GameObject.Find("GameHandler");

        if(GHGO == null){
            if(this.GameHandlerPrefab == null){
                Debug.LogError("No Game Handler Prefab Set!");
                return;
            }

            GHGO = Instantiate(this.GameHandlerPrefab);
            GHGO.name = "GameHandler";
        }
    }

    void Update(){
        CheckPlayerInput();
    }

    public void StartSetup(GameHandler GH){
        this.GameHandler = GH;

        for(int i=0; i<GH.GetCurrentHighestStage(); i++){
            StartMenuButton B = Instantiate(StageOptionPrefab, MenuOptions_GO.transform).GetComponent<StartMenuButton>();
            B.SetOptionText("Stage "+(i+1).ToString());
            B.UnHoverMenuButton();
            buttonList.Add(B);
        }

        this.buttonIndex = 0;
        this.buttonList[this.buttonIndex].HoverMenuButton();
    }

    private void CheckPlayerInput(){
        if( Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ){
            OptionDown();
        }else if( Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ){
            OptionUp();
        }else if( Input.GetKeyDown(KeyCode.Space) ){
            SelectStage();
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

    private void SelectStage(){
        GameHandler.PlaySelectMenuOptionSound();
        // switch(this.buttonIndex){
        //     case 0:
        //         StartGame();
        //     break;

        //     case 1:
        //         StartTutorial();
        //     break;

        //     case 2:
        //         QuitGame();
        //     break;

        //     default:
        //         Debug.LogError("Error: Button Index Out Of Bounds!");
        //     break;
        // }
    }
}