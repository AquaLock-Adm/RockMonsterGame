using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AbilityLoadoutHandler : StandartMenuHandler
{
    [Header("Ability Loadout Data")]
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
    private int[] MaxCountAbilitiesOfLevel = {3,1,3,4,4,2,1,1,1,1}; // how many abilities of each level can be set

    static int MAX_DISPLAYED_ABILITIES = 10;

    private int displayedOptionsIndex = 0;
    private int abilityListIndex = 3;
    private bool backButtonHovered = false;

    public override void StartSetup(GameHandler GH){
        base.StartSetup(GH);

        // Debug.Log("For testing: earned cp + 10000");
        // GameHandler.earnedCredits += 10000;

        this.CreditScoreText.text = GameHandler.earnedCredits.ToString() + "cp";

        this.displayedOptionsIndex = 0;
        this.abilityListIndex = 3; // skipping all 3 base abilities

        // FOR TESTING

        // this.displayedOptionsIndex = 0;
        // this.abilityListIndex = 5;
        
        SetupSelectBar();
        // SetupComboListOptions();
        UpdateComboListOptions();

        SelectBar.HoverSelectText(this.displayedOptionsIndex);
        UnhoverBackoption();
    }

    private void SetupSelectBar(){
        float selectTextHeight = ComboListOptionPrefab.GetComponent<RectTransform>().rect.height;
        float fillerTextHeight = ComboListOptionHeaderPrefab.GetComponent<RectTransform>().rect.height;

        this.SelectBar.AbilityLoadoutSetup(selectTextHeight, fillerTextHeight);
    }

    private void UpdateComboListOptions(){
        // NOTE: expects GameHandler.Player.GetWeapon().GetCompleteMoveList() to be sorted by combolevel!!!

        this.AbilityOptionButtons.Clear();
        ClearOptionButtons();
        this.SelectBar.ClearSelectTextList();

        List<Action> abilities = GameHandler.Player.GetWeapon().GetCompleteMoveList();
        int comboLevelDisplayed = abilities[ (int)Mathf.Max(0.0f, (float)(this.abilityListIndex-this.displayedOptionsIndex-1)) ].comboLevel;

        if(abilities.Count <= 3){
            Debug.LogError("ONLY BASIC ABILITIES IN LIST!");
            return;
        }

        for(int aI = 0; aI < MAX_DISPLAYED_ABILITIES; aI++){
            int getAbilityIndex = (this.abilityListIndex - this.displayedOptionsIndex + aI) % abilities.Count;
            Action A = abilities[getAbilityIndex];
            // Skip Base Abilities (Light, Heavy, Special)
            if(A.comboLevel <= 1) continue;

            if(A.comboLevel != comboLevelDisplayed){
                AddNewHeader(A.comboLevel);
                comboLevelDisplayed = A.comboLevel;
            }

            AbilityOptionButton NewButton = AddNewAbilityOption(A);
            this.SelectBar.AddSelectText();

            if(GameHandler.UnlockedAbilitiesList.Contains(A)) NewButton.ShowLockedStatus(false);

            if(GameHandler.SetAbilitiesList.Contains(A)) {
                NewButton.ShowSetStatus(true);
                this.SetAbilitiesOfLevel[A.comboLevel-1]++;
            }

            this.AbilityOptionButtons.Add(NewButton);
        } 
    }

    private void ClearOptionButtons(){
        foreach(Transform C in this.ComboList_GO.transform) Destroy(C.gameObject);
    }

    private void AddNewHeader(int comboLevel){
        GameObject Header_GO = Instantiate(ComboListOptionHeaderPrefab, ComboList_GO.transform);
        Header_GO.GetComponent<StartMenuButton>().SetOptionText("Level "+comboLevel.ToString());
        this.SelectBar.AddFillerText();
    }

    private AbilityOptionButton AddNewAbilityOption(Action A){
        GameObject ComboOption_GO = Instantiate(ComboListOptionPrefab, ComboList_GO.transform);
        AbilityOptionButton NewButton = ComboOption_GO.GetComponent<AbilityOptionButton>();
        NewButton.AbilityLoadoutSetup(A);

        return NewButton;
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

        this.abilityListIndex++;

        if(this.abilityListIndex >= GameHandler.Player.GetWeapon().GetCompleteMoveList().Count){
            this.abilityListIndex = 3;
            this.displayedOptionsIndex = 0;
            UpdateComboListOptions();
        }else if(displayedOptionsIndex < MAX_DISPLAYED_ABILITIES-1){
            this.displayedOptionsIndex++;
        }else UpdateComboListOptions();

        SelectBar.HoverSelectText(this.displayedOptionsIndex);
        DisplayAction(this.AbilityOptionButtons[this.displayedOptionsIndex].AssignedAction);
    }

    private void OptionUp(){
        GameHandler.PlaySwitchMenuOptionSound();

        this.abilityListIndex--;

        if(this.abilityListIndex < 3){
            this.abilityListIndex = GameHandler.Player.GetWeapon().GetCompleteMoveList().Count-1;
            this.displayedOptionsIndex = MAX_DISPLAYED_ABILITIES-1;
            UpdateComboListOptions();
        }else if(displayedOptionsIndex > 0){
            this.displayedOptionsIndex--;
        }else UpdateComboListOptions();

        SelectBar.HoverSelectText(this.displayedOptionsIndex);
        DisplayAction(this.AbilityOptionButtons[this.displayedOptionsIndex].AssignedAction);
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
        DisplayAction(this.AbilityOptionButtons[this.displayedOptionsIndex].AssignedAction);
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
            Action CurrentHoveredAction = this.AbilityOptionButtons[this.displayedOptionsIndex].AssignedAction;

            if(GameHandler.UnlockedAbilitiesList.Contains(CurrentHoveredAction)){
                if(GameHandler.SetAbilitiesList.Contains(CurrentHoveredAction)){
                    GameHandler.SetAbilitiesList.RemoveAt(GameHandler.SetAbilitiesList.IndexOf(CurrentHoveredAction));
                    this.AbilityOptionButtons[this.displayedOptionsIndex].ShowSetStatus(false);
                    this.SetAbilitiesOfLevel[CurrentHoveredAction.comboLevel-1]--;
                    CheckSetAbilitiesOfSameLevelCount(CurrentHoveredAction.comboLevel);
                }else{
                    GameHandler.SetAbilitiesList.Add(CurrentHoveredAction);
                    this.AbilityOptionButtons[this.displayedOptionsIndex].ShowSetStatus(true);
                    this.SetAbilitiesOfLevel[CurrentHoveredAction.comboLevel-1]++;
                    CheckSetAbilitiesOfSameLevelCount(CurrentHoveredAction.comboLevel);
                }
            }else{
                int price = CurrentHoveredAction.unlockPrice;
                if(GameHandler.earnedCredits >= price){
                    GameHandler.earnedCredits -= price;
                    this.CreditScoreText.text = GameHandler.earnedCredits.ToString() + "cp";
                    this.AbilityOptionButtons[this.displayedOptionsIndex].ShowLockedStatus(false);
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
        if(this.SetAbilitiesOfLevel[level-1] > this.MaxCountAbilitiesOfLevel[level-1]){
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
        for(int aI=0; aI < this.SetAbilitiesOfLevel.Length; aI++){
            if(this.SetAbilitiesOfLevel[aI] > this.MaxCountAbilitiesOfLevel[aI]) return true;
        }

        return false;
    }
}
