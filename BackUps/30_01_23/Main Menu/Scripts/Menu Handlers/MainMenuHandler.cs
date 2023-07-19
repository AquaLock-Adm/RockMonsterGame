using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuHandler : MonoBehaviour
{
    public GameHandler GameHandler;
    public MenuHandler MenuHandler;

    private PlayerCharacter Player;
    private Weapon PlayerWeapon;
    private Armor PlayerArmor;

    [SerializeField] public TextMeshProUGUI WeaponNameText;
    [SerializeField] public TextMeshProUGUI ArmorNameText;

    [SerializeField] public TextMeshProUGUI TotalCreditsEarnedText;
    
    [Header("Character Screen Texts")]
    [SerializeField] public TextMeshProUGUI CharacterNameText;
    [SerializeField] public TextMeshProUGUI CharacterHPText;
    [SerializeField] public TextMeshProUGUI CharacterShieldText;
    [SerializeField] public TextMeshProUGUI CharacterManaText;
    [SerializeField] public TextMeshProUGUI CharacterAtkText;
    [SerializeField] public TextMeshProUGUI CharacterProficiencyLevelText;
    [SerializeField] public TextMeshProUGUI CharacterExpText;

    [Header("Skill Select Screen Texts")]
    public GameObject SkillSelectScreen;
    public GameObject SkillBoxPrefab;
    public GameObject SkillListNotTaken;
    public GameObject SkillListTaken;

    public GameObject SkillInformation;
    public GameObject TakeButton;
    public GameObject ReturnButton;

    private Action SelectedSkill;

    [SerializeField] public TextMeshProUGUI SkillNameText;
    [SerializeField] public TextMeshProUGUI SkillComboListText;
    // [SerializeField] public TextMeshProUGUI SkillApText;
    [SerializeField] public TextMeshProUGUI SkillManaText;
    [SerializeField] public TextMeshProUGUI SkillLevelText;
    [SerializeField] public TextMeshProUGUI SkillDamageText;
    [SerializeField] public TextMeshProUGUI SkillEffectsText;
    [SerializeField] public TextMeshProUGUI SkillDescriptionText;

    private bool inSkillSelect = false;

    [Header("Armor and Weapon Select")]
    public GameObject WeaponDropDownMenu;
    public GameObject WeaponBoxPrefab;
    public GameObject ArmorDropDownMenu;
    public GameObject ArmorBoxPrefab;


    [Header("Dungeon Screen Texts")]
    [SerializeField] public TextMeshProUGUI DungeonNameText;
    [SerializeField] public TextMeshProUGUI EntryCostText;
    [SerializeField] public TextMeshProUGUI MostWavesText;

    [SerializeField] public TextMeshProUGUI LastRunCreditsText;
    [SerializeField] public TextMeshProUGUI LastRunTimeText;
    [SerializeField] public TextMeshProUGUI LastRunWaveCountText;

    [SerializeField] public TextMeshProUGUI BestRunCreditsText;
    [SerializeField] public TextMeshProUGUI BestRunTimeText;
    [SerializeField] public TextMeshProUGUI BestRunWaveCountText;

    public GameObject LastRunScreen;
    public GameObject BestRunScreen;
    [Header("Temp")]
    [SerializeField] public GameObject WeaponButtonHitBox;
    [SerializeField] public GameObject ArmorButtonHitBox;

    public void Setup(){
        SetPlayerReferencesFromGameHandler();
        SetupWeaponScreen();
        SetupArmorScreen();
        SetupCreditsScreen();
        SetupCharacterScreen();
        SetupDungeonScreen();
    }

    private void SetPlayerReferencesFromGameHandler(){
        this.Player = this.GameHandler.Player;
        this.PlayerWeapon = this.GameHandler.PlayerWeapon;
        this.PlayerArmor = this.GameHandler.PlayerArmor;
    }

    private void SetupWeaponScreen(){
        if(this.PlayerWeapon == null) this.WeaponNameText.text = "No Weapon Set!";
        else this.WeaponNameText.text = this.PlayerWeapon.weaponName;
    }

    private void SetupArmorScreen(){
        if(this.PlayerArmor == null) this.ArmorNameText.text = "No Armor Set!";
        else this.ArmorNameText.text = this.PlayerArmor.armorName;
    }

    private void SetupCreditsScreen(){

        this.TotalCreditsEarnedText.text = this.GameHandler.totalCreditsEarned.ToString()+" Cp";
    }

    private void SetupCharacterScreen(){
        SetCharacterNameText();
        SetHPShieldManaText();
        SetAtkText();
        SetProficiencyText();
    }

    private void SetCharacterNameText(){

        this.CharacterNameText.text = this.Player.unitName;
    }

    private void SetHPShieldManaText(){
        if(this.PlayerArmor == null) {
            this.CharacterHPText.text = "1";
            this.CharacterShieldText.text = "0";
        }
        else {
            this.CharacterHPText.text = this.PlayerArmor.maxDurability.ToString();
            this.CharacterShieldText.text = this.PlayerArmor.shield.ToString();
        }
        this.CharacterManaText.text = this.Player.maxMana.ToString();
    }

    private void SetAtkText(){
        if(this.PlayerWeapon == null) this.CharacterAtkText.text = "0 - 1";
        else this.CharacterAtkText.text = this.PlayerWeapon.baseAttackMin.ToString() + " - " + this.PlayerWeapon.baseAttackMax.ToString();
    }

    private void SetProficiencyText(){
        int profLevel = this.GameHandler.playerProficiencyLevel;
        this.CharacterProficiencyLevelText.text = profLevel.ToString();
        if(profLevel < this.GameHandler.maxProficiencyLevel){
            this.CharacterExpText.text = this.GameHandler.playerExperienceToNextLevel.ToString()+"/"+this.GameHandler.GetLevelLimit(profLevel).ToString();
        }else this.CharacterExpText.text = "";
    }

    private void SetupDungeonScreen(){
        SetCurrentDungeon();
        SetLastRun();
        SetBestRun();
    }

    private void SetCurrentDungeon(){
        if(this.GameHandler.CurrentDungeon != null){
            this.DungeonNameText.text = this.GameHandler.CurrentDungeon.dungeonName;
            this.EntryCostText.text = this.GameHandler.CurrentDungeon.entryCost.ToString();
            this.MostWavesText.text = this.GameHandler.CurrentDungeon.mostWavesCleared.ToString();
        }else{
            this.DungeonNameText.text = "No Dungeon Set!";
            this.EntryCostText.text = "";
            this.MostWavesText.text = "";
        }
    }

    private void SetLastRun(){
        DungeonRun LastRun = this.GameHandler.CurrentDungeon.LastRun;
        if(LastRun != null){
            this.LastRunScreen.SetActive(true);

            this.LastRunCreditsText.text = LastRun.creditsEarned.ToString();
            this.LastRunTimeText.text = this.GameHandler.TimeToString(LastRun.time);
            this.LastRunWaveCountText.text = LastRun.wavesCleared.ToString();
        }else {
            this.LastRunCreditsText.text = "";
            this.LastRunTimeText.text = "";
            this.LastRunWaveCountText.text = ""; 

            this.LastRunScreen.SetActive(false);
        }
    }

    private void SetBestRun(){
        DungeonRun BestRun = this.GameHandler.CurrentDungeon.BestRun;
        if(BestRun != null){
            this.BestRunScreen.SetActive(true);

            this.BestRunCreditsText.text = BestRun.creditsEarned.ToString();
            this.BestRunTimeText.text = this.GameHandler.TimeToString(BestRun.time);
            this.BestRunWaveCountText.text = BestRun.wavesCleared.ToString();
        }else {
            this.BestRunCreditsText.text = "";
            this.BestRunTimeText.text = "";
            this.BestRunWaveCountText.text = ""; 

            this.BestRunScreen.SetActive(false);
        }
    }

    public void Shop(){
        Debug.Log("Shop Called");
    }

    public void Skills(){
        if(!this.inSkillSelect){
            this.SkillSelectScreen.SetActive(true);
            LoadSkillsIntoSkillLists();
            SetSkillInfoText(null);
        }else {
            this.SkillSelectScreen.SetActive(false);
            this.SelectedSkill = null;
            SetSkillInfoText(null);
        }
        this.inSkillSelect = !this.inSkillSelect;
    }

    private void ReloadSkillMenu(){
        this.SkillSelectScreen.SetActive(false);
        this.SkillSelectScreen.SetActive(true);
        LoadSkillsIntoSkillLists();
        SetSkillInfoText(this.SelectedSkill);
    }

    private void LoadSkillsIntoSkillLists(){
        ClearSkillLists();
        LoadSkillListNotTaken();
        LoadSkillListTaken();
    }

    private void ClearSkillLists(){
        foreach(Transform t in this.SkillListTaken.transform){
            Destroy(t.gameObject);
        }
        foreach(Transform t in this.SkillListNotTaken.transform){
            Destroy(t.gameObject);
        }
    }

    private void LoadSkillListNotTaken(){
        foreach(Action A in this.GameHandler.PlayerAbilityList){
            GameObject NewSkillBox_GO = Instantiate(SkillBoxPrefab);
            NewSkillBox_GO.transform.SetParent(this.SkillListNotTaken.transform);
            SkillBox SB = NewSkillBox_GO.GetComponent<SkillBox>();

            SB.Setup(A);
            if(this.Player.AbilitiesContain(A)) SB.ChangeActiveState(false);
        }
    }

    private void LoadSkillListTaken(){
        foreach(Action A in this.Player.Abilities){
            GameObject NewSkillBox_GO = Instantiate(SkillBoxPrefab);
            NewSkillBox_GO.transform.SetParent(this.SkillListTaken.transform);
            SkillBox SB = NewSkillBox_GO.GetComponent<SkillBox>();

            SB.Setup(A);
        }
    }

    public void SetSkillInfoText(Action A){
        if(A == null){
            this.SkillInformation.SetActive(false);
            this.TakeButton.SetActive(false);
            this.ReturnButton.SetActive(false);
            return;
        }
        if(A.comboLevel < 2){
            this.TakeButton.SetActive(false);
            this.ReturnButton.SetActive(false);
        }else if(this.Player.AbilitiesContain(A)){
            this.TakeButton.SetActive(false);
            this.ReturnButton.SetActive(true);
        }else{
            this.TakeButton.SetActive(true);
            this.ReturnButton.SetActive(false);
        }

        this.SkillInformation.SetActive(true);
        this.SelectedSkill = A;

        this.SkillNameText.text = A.name;
        this.SkillLevelText.text = A.comboLevel.ToString();
        this.SkillComboListText.text = A.comboList;
        this.SkillManaText.text = A.manaCost.ToString();
        // this.SkillDamageText.text = A.GetDamageInfoString(); //<-- not implemented
        // this.SkillEffectsText.text = A.GetEffectListString(); //<-- not implemented
        // this.SkillDescriptionText.text = A.decriptionText; //<-- not implemented
        // TODO
    } // TODO

    public void AddSkillFromInactive(){
        if(this.SelectedSkill != null){
            this.Player.AddAbility(this.SelectedSkill);
            // Debug.Log("Skill added");
            // this.Player.PrintAbilities();
            ReloadSkillMenu();
        }
    }

    public void AddSkillFromActive(){
        if(this.SelectedSkill != null){
            this.Player.RemoveFromAbilities(this.SelectedSkill);
            ReloadSkillMenu();
        }
    }

    public void CurrentWeapon(){
        UnloadDropDownMenu(this.ArmorDropDownMenu);
        this.WeaponDropDownMenu.SetActive(true);
        GameObject WeaponBox_GO;
        ArmorWeaponBox B;

        foreach(Weapon W in this.GameHandler.InventoryWeapons){
            WeaponBox_GO = Instantiate(this.WeaponBoxPrefab);
            WeaponBox_GO.transform.SetParent(this.WeaponDropDownMenu.transform);
            B = WeaponBox_GO.GetComponent<ArmorWeaponBox>();
            B.SetupWeapon(W);
        }

        // FOR NULL_WEAPON
        WeaponBox_GO = Instantiate(this.WeaponBoxPrefab);
        WeaponBox_GO.transform.SetParent(this.WeaponDropDownMenu.transform);
        B = WeaponBox_GO.GetComponent<ArmorWeaponBox>();
        B.SetupWeapon(this.GameHandler.NULL_WeaponPrefab.GetComponent<Weapon>());


        // // FOR TESTING
        // this.GameHandler.SetNewWeapon(this.GameHandler.InventoryWeapons[0]);
        // this.MenuHandler.LoadMainMenu(this.gameObject);
        // this.WeaponButtonHitBox.SetActive(false);
    } // TODO

    public void SelectWeapon(Weapon W){
        this.GameHandler.SetNewWeapon(W);
        UnloadDropDownMenu(this.WeaponDropDownMenu);
        this.MenuHandler.LoadMainMenu(this.gameObject);
    }

    public void CurrentArmor(){
        UnloadDropDownMenu(this.WeaponDropDownMenu);
        this.ArmorDropDownMenu.SetActive(true);
        GameObject ArmorBox_GO;
        ArmorWeaponBox B;

        foreach(Armor A in this.GameHandler.InventoryArmor){
            ArmorBox_GO = Instantiate(this.ArmorBoxPrefab);
            ArmorBox_GO.transform.SetParent(this.ArmorDropDownMenu.transform);
            B = ArmorBox_GO.GetComponent<ArmorWeaponBox>();
            B.SetupArmor(A);
        }

        // FOR NULL_ARMOR
        ArmorBox_GO = Instantiate(this.ArmorBoxPrefab);
        ArmorBox_GO.transform.SetParent(this.ArmorDropDownMenu.transform);
        B = ArmorBox_GO.GetComponent<ArmorWeaponBox>();
        B.SetupArmor(this.GameHandler.NULL_ArmorPrefab.GetComponent<Armor>());

        // // FOR TESTING
        // this.GameHandler.SetNewArmor(this.GameHandler.InventoryArmor[0]);
        // this.MenuHandler.LoadMainMenu(this.gameObject);
        // this.ArmorButtonHitBox.SetActive(false);
    } // TODO

    public void SelectArmor(Armor A){
        this.GameHandler.SetNewArmor(A);
        UnloadDropDownMenu(this.ArmorDropDownMenu);
        this.MenuHandler.LoadMainMenu(this.gameObject);
    }

    public void UnloadDropDownMenu(GameObject DropDownMenu){
        foreach(Transform t in DropDownMenu.transform){
            Destroy(t.gameObject);
        }
        DropDownMenu.SetActive(false);
    }

    public void End(){
        Debug.Log("End Called");
    }

    public void Options(){
        Debug.Log("Options Called");
    }

    public void StartDungeon(){
        
        this.MenuHandler.LoadDungeonStartMenu();
    }

    public void Reset(){
        Debug.Log("Reset Called");
    }
}