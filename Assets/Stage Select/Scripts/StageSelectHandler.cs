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
    [SerializeField] private StartMenuButton BackButton;
    private bool backButtonHovered = false;


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

        this.backButtonHovered = false;
        this.buttonIndex = 0;
        this.buttonList[this.buttonIndex].HoverMenuButton();
        this.BackButton.UnHoverMenuButton();
    }

    private void CheckPlayerInput(){
        if( Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ){
            OptionDown();
        }else if( Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ){
            OptionUp();
        }else if( Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ){
            HoverBackOption();
        }else if( Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ){
            UnhoverBackoption();
        }else if( Input.GetKeyDown(KeyCode.Space) ){
            SelectStage();
        }
    }

    private void HoverBackOption(){
        GameHandler.PlaySwitchMenuOptionSound();
        this.buttonList[this.buttonIndex].UnHoverMenuButton();
        BackButton.HoverMenuButton();
        backButtonHovered = true;

    }

    private void UnhoverBackoption(){
        GameHandler.PlaySwitchMenuOptionSound();
        this.buttonList[this.buttonIndex].HoverMenuButton();
        BackButton.UnHoverMenuButton();
        backButtonHovered = false;
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
        if(backButtonHovered){
            GameHandler.LoadMainMenu();
        }else{
            GameHandler.SetCurrentStartStage(this.buttonIndex+1);
            GameHandler.LoadBattleScene();
        }
    }
}