using System.Collections.Generic;
using UnityEngine;

public class AbilityLoadoutHandler : StandartMenuHandler
{
    [Header("Ability Loadout Data")]
    [SerializeField] private GameObject ComboListOptionPrefab;
    [SerializeField] private GameObject ComboListOptionHeaderPrefab;

    [SerializeField] private GameObject ComboList_GO;

    [SerializeField] private AbilityOptionSelectBar SelectBar;
    private List<AbilityOptionButton> AbilityOptionButtons = new List<AbilityOptionButton>();

    public override void StartSetup(GameHandler GH){
        base.StartSetup(GH);

        SetupSelectBar();
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

    protected override void CheckPlayerInput(){

    }

    private void SetupSelectBar(){
        float selectTextHeight = ComboListOptionPrefab.GetComponent<RectTransform>().rect.height;
        float fillerTextHeight = ComboListOptionHeaderPrefab.GetComponent<RectTransform>().rect.height;

        this.SelectBar.AbilityLoadoutSetup(selectTextHeight, fillerTextHeight);
    }

    private void SetupComboListOptions(){
        int comboLevelDisplayed = 1;
        foreach(Action A in GameHandler.Player.GetWeapon().GetCompleteMoveList()){
            // Skip Base Abilities (Light, Heavy, Special)
            if(A.comboLevel <= 1) continue;

            if(A.comboLevel > comboLevelDisplayed){
                AddNewHeader(A.comboLevel);
                comboLevelDisplayed = A.comboLevel;
            }
            GameObject ComboOption_GO = Instantiate(ComboListOptionPrefab, ComboList_GO.transform);
            AbilityOptionButton NewButton = ComboOption_GO.GetComponent<AbilityOptionButton>();
            NewButton.SetOptionText(A.name);
            NewButton.AbilityLoadoutSetup();
            this.SelectBar.AddSelectText();

            if(GameHandler.SetAbilitiesList.Contains(A)) {
                NewButton.ShowLockedStatus(false);
                // Debug.Log(A.name);
            }

            AbilityOptionButtons.Add(NewButton);
        }
    }

    private void AddNewHeader(int comboLevel){
        GameObject Header_GO = Instantiate(ComboListOptionHeaderPrefab, ComboList_GO.transform);
        Header_GO.GetComponent<StartMenuButton>().SetOptionText("Level "+comboLevel.ToString());
        this.SelectBar.AddFillerText();
    }
}
