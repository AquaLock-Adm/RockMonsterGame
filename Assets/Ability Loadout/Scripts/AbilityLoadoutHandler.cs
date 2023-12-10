using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityLoadoutHandler : MonoBehaviour
{
    [SerializeField] private GameObject ComboListOptionPrefab;
    [SerializeField] private GameObject ComboListOptionHeaderPrefab;

    [SerializeField] private GameObject ComboList_GO;

    [SerializeField] private GameObject GameHandlerPrefab;

    private List<AbilityOptionButton> AbilityOptionButtons = new List<AbilityOptionButton>();

    private GameHandler GameHandler;

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

        SetupComboListOptions();

        // for(int i=0; i<GH.GetCurrentHighestStage(); i++){
        //     StartMenuButton B = Instantiate(StageOptionPrefab, MenuOptions_GO.transform).GetComponent<StartMenuButton>();
        //     B.SetOptionText("Stage "+(i+1).ToString());
        //     B.UnHoverMenuButton();
        //     buttonList.Add(B);
        // }

        // this.backButtonHovered = false;
        // this.buttonIndex = 0;
        // this.buttonList[this.buttonIndex].HoverMenuButton();
        // this.BackButton.UnHoverMenuButton();
    }

    private void CheckPlayerInput(){

    }

    private void SetupComboListOptions(){
        int comboLevelDisplayed = 1;
        foreach(Action A in GameHandler.Player.GetWeapon().GetCompleteMoveList()){
            if(A.comboLevel <= 1) continue;
            if(A.comboLevel > comboLevelDisplayed){
                GameObject Header_GO = Instantiate(ComboListOptionHeaderPrefab, ComboList_GO.transform);
                Header_GO.GetComponent<StartMenuButton>().SetOptionText("Level "+A.comboLevel.ToString());
                comboLevelDisplayed = A.comboLevel;
            }
            Debug.Log("Spawn ComboList Item");
        }
    }
}
