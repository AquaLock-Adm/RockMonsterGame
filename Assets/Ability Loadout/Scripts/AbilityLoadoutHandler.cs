using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AbilityLoadoutHandler : StandartMenuHandler
{
    [Header("Ability Loadout Data")]
    public int maxNumberOfSameLevelAbilities = 3;

    [SerializeField] private GameObject ComboListOptionPrefab;
    [SerializeField] private GameObject ComboListOptionHeaderPrefab;

    [SerializeField] private GameObject ComboList_GO;

    [SerializeField] private TextMeshProUGUI CreditScoreText;

    [SerializeField] private TextMeshProUGUI AbilityNameText;
    [SerializeField] private TextMeshProUGUI AbilityPriceText;
    [SerializeField] private TextMeshProUGUI AbilityLevelText;
    [SerializeField] private TextMeshProUGUI AbilityComboText;
    [SerializeField] private TextMeshProUGUI AbilityDamageText;
    [SerializeField] private TextMeshProUGUI AbilityHeatSpentText;

    [SerializeField] private AbilityOptionSelectBar SelectBar;
    [SerializeField] private StartMenuButton BackButton;
    private List<AbilityOptionButton> AbilityOptionButtons = new List<AbilityOptionButton>();

    private int[] SetAbilitiesOfLevel = {0,0,0,0,0,0,0,0,0,0};

    private int optionIndex = 0;
    private bool backButtonHovered = false;

    public override void StartSetup(GameHandler GH){
        base.StartSetup(GH);

        // Debug.Log("For testing: earned cp + 10000");
        // GameHandler.earnedCredits += 10000;

        this.CreditScoreText.text = GameHandler.earnedCredits.ToString() + "cp";
        
        SetupSelectBar();
        SetupComboListOptions();

        this.optionIndex = 0;
        SelectBar.HoverSelectText(this.optionIndex);
        UnhoverBackoption();
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
            NewButton.AbilityLoadoutSetup(A);
            this.SelectBar.AddSelectText();

            if(GameHandler.UnlockedAbilitiesList.Contains(A)) {
                NewButton.ShowLockedStatus(false);
                // Debug.Log(A.name);
            }

            if(GameHandler.SetAbilitiesList.Contains(A)) {
                NewButton.ShowSetStatus(true);
                this.SetAbilitiesOfLevel[A.comboLevel-1]++;
            }

            AbilityOptionButtons.Add(NewButton);
        }
    }

    private void AddNewHeader(int comboLevel){
        GameObject Header_GO = Instantiate(ComboListOptionHeaderPrefab, ComboList_GO.transform);
        Header_GO.GetComponent<StartMenuButton>().SetOptionText("Level "+comboLevel.ToString());
        this.SelectBar.AddFillerText();
    }

    protected override void CheckPlayerInput(){
        if( Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ){
            OptionDown();
        }else if( Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ){
            OptionUp();
        }else if( Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ){
            HoverBackOption();
        }else if( Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ){
            UnhoverBackoption();
        }else if( Input.GetKeyDown(KeyCode.Space) ){
            SelectOption();
        }
    }

    private void OptionDown(){
        GameHandler.PlaySwitchMenuOptionSound();
        int lastIndex = this.optionIndex;
        this.optionIndex = (this.optionIndex+1) % this.AbilityOptionButtons.Count;
        SelectBar.HoverSelectText(this.optionIndex);
        DisplayAction(this.AbilityOptionButtons[this.optionIndex].AssignedAction);
    }

    private void OptionUp(){
        GameHandler.PlaySwitchMenuOptionSound();
        int lastIndex = this.optionIndex;
        if(this.optionIndex > 0) this.optionIndex--;
        else this.optionIndex = this.AbilityOptionButtons.Count-1;
        SelectBar.HoverSelectText(this.optionIndex);
        DisplayAction(this.AbilityOptionButtons[this.optionIndex].AssignedAction);
    }

    private void HoverBackOption(){
        GameHandler.PlaySwitchMenuOptionSound();
        SelectBar.UnHoverSelectBar();
        BackButton.HoverMenuButton();
        backButtonHovered = true;

    }

    private void UnhoverBackoption(){
        GameHandler.PlaySwitchMenuOptionSound();
        SelectBar.HoverSelectBar();
        DisplayAction(this.AbilityOptionButtons[this.optionIndex].AssignedAction);
        BackButton.UnHoverMenuButton();
        backButtonHovered = false;
    }

    private void SelectOption(){
        if(this.backButtonHovered){
            if(!AbilitiesOfSameLevelOverloadActive()) {
                GameHandler.Player.SetAbilities = GameHandler.SetAbilitiesList;
                GameHandler.PlayerWeapon.UpdateAbilityIndex();
                GameHandler.LoadMainMenu();
            }
        }else{
            Action CurrentHoveredAction = this.AbilityOptionButtons[this.optionIndex].AssignedAction;

            if(GameHandler.UnlockedAbilitiesList.Contains(CurrentHoveredAction)){
                if(GameHandler.SetAbilitiesList.Contains(CurrentHoveredAction)){
                    GameHandler.SetAbilitiesList.RemoveAt(GameHandler.SetAbilitiesList.IndexOf(CurrentHoveredAction));
                    this.AbilityOptionButtons[this.optionIndex].ShowSetStatus(false);
                    this.SetAbilitiesOfLevel[CurrentHoveredAction.comboLevel-1]--;
                    CheckSetAbilitiesOfSameLevelCount(CurrentHoveredAction.comboLevel);
                }else{
                    GameHandler.SetAbilitiesList.Add(CurrentHoveredAction);
                    this.AbilityOptionButtons[this.optionIndex].ShowSetStatus(true);
                    this.SetAbilitiesOfLevel[CurrentHoveredAction.comboLevel-1]++;
                    CheckSetAbilitiesOfSameLevelCount(CurrentHoveredAction.comboLevel);
                }
            }else{
                int price = CurrentHoveredAction.unlockPrice;
                if(GameHandler.earnedCredits >= price){
                    GameHandler.earnedCredits -= price;
                    this.CreditScoreText.text = GameHandler.earnedCredits.ToString() + "cp";
                    this.AbilityOptionButtons[this.optionIndex].ShowLockedStatus(false);
                    GameHandler.UnlockedAbilitiesList.Add(CurrentHoveredAction);
                }
            }
            
            DisplayAction(CurrentHoveredAction);
        }
    }

    private void DisplayAction(Action A){

        this.AbilityNameText.text = A.name;
        this.AbilityLevelText.text = "LV. " + A.comboLevel.ToString();
        this.AbilityComboText.text = A.comboString;
        if(GameHandler.UnlockedAbilitiesList.Contains(A)) {
            this.AbilityPriceText.text = "UNLOCKED";
        }else{
            this.AbilityPriceText.text = A.unlockPrice.ToString() +"cp";
        }

        if(A.spentHeatOnHit) this.AbilityHeatSpentText.text = "YES";
        else this.AbilityHeatSpentText.text = "NO";

        this.AbilityDamageText.text = this.GetAbilityDamageRating(A).ToString();
    }

    private int GetAbilityDamageRating(Action A){
        if(A.comboLevel < 2) return 1;
        int res = (A.comboLevel-1) * 2;
        if(A.spentHeatOnHit) res += 1;
        return res;
    }

    private void CheckSetAbilitiesOfSameLevelCount(int level){
        if(this.SetAbilitiesOfLevel[level-1] > this.maxNumberOfSameLevelAbilities){
            foreach(AbilityOptionButton Button in this.AbilityOptionButtons){
                if(Button.AssignedAction.comboLevel == level && GameHandler.SetAbilitiesList.Contains(Button.AssignedAction)){
                    Button.OverloadSetStatus();
                }
            }
        }else{
            foreach(AbilityOptionButton Button in this.AbilityOptionButtons){
                if(Button.AssignedAction.comboLevel == level){
                    if(GameHandler.SetAbilitiesList.Contains(Button.AssignedAction)) Button.ShowSetStatus(true);
                    else Button.ShowSetStatus(false);
                }
            }
        }
    }

    private bool AbilitiesOfSameLevelOverloadActive(){
        foreach(int count in this.SetAbilitiesOfLevel){
            if(count > this.maxNumberOfSameLevelAbilities) return true;
        }

        return false;
    }
}
