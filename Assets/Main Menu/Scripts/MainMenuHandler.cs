using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuHandler : StandartMenuHandler
{
    [Header("Main Menu Text Ref")]
    [SerializeField] private TextMeshProUGUI CreditsText;
    [SerializeField] private TextMeshProUGUI CreditsCostText;
    [SerializeField] private TextMeshProUGUI StageText;
    [SerializeField] private TextMeshProUGUI CharNameText;
    [SerializeField] private TextMeshProUGUI ArmorLVText;
    [SerializeField] private TextMeshProUGUI HPText;
    [SerializeField] private TextMeshProUGUI NextHPText;
    [SerializeField] private TextMeshProUGUI WeaponLVText;
    [SerializeField] private TextMeshProUGUI ATKText;
    [SerializeField] private TextMeshProUGUI NextATKText;

    [Header("Main Menu Button Ref")]
    [SerializeField] private List<StartMenuButton> MenuButtonList;
    [SerializeField] private StartMenuButton BackButton;

    private int buttonIndex = 0;
    private bool backButtonHovered = false;

    public override void StartSetup(GameHandler GH){
        base.StartSetup(GH);
        UpdateAllTexts();
        ClearNextLVLTexts();

        SetupMenuButtons();
    }

    private void UpdateAllTexts(){
        this.CreditsText.text = this.GameHandler.earnedCredits.ToString()+" cp";
        this.StageText.text = this.GameHandler.GetCurrentStartStage().ToString();
        this.CharNameText.text = this.Player.unitName;
        this.ArmorLVText.text = "LV " + this.Player.GetArmorLevel().ToString();
        this.HPText.text = this.Player.healthPoints.ToString();
        this.WeaponLVText.text = "LV " + this.Player.GetWeaponLevel().ToString();
        this.ATKText.text = this.Player.GetAttackMin().ToString() + " - " + this.Player.GetAttackMax().ToString();
    }

    private void ClearNextLVLTexts(){
        this.NextATKText.text = "";
        this.NextHPText.text = "";
        this.CreditsCostText.text = "";
    }

    private void SetupMenuButtons(){
        this.buttonIndex = 0;
        this.backButtonHovered = false;

        this.BackButton.UnHoverMenuButton();

        foreach(StartMenuButton B in this.MenuButtonList){
            B.UnHoverMenuButton();
        }

        this.MenuButtonList[0].HoverMenuButton();
    }

    protected override void CheckPlayerInput(){
        if( Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) ){
            OptionDown();
        }else if( Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ){
            OptionUp();
        }else if( Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D)  || Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.RightArrow) ){
            HoverBackButton();
        }else if( Input.GetKeyDown(KeyCode.Space) ){
            SelectOption();
        }
    }

    private void OptionDown(){
        GameHandler.PlaySwitchMenuOptionSound();
        int lastIndex = this.buttonIndex;
        this.buttonIndex = (this.buttonIndex+1) % this.MenuButtonList.Count;

        UpdateHovers(lastIndex);
    }

    private void OptionUp(){
        GameHandler.PlaySwitchMenuOptionSound();
        int lastIndex = this.buttonIndex;
        if(this.buttonIndex > 0) this.buttonIndex--;
        else this.buttonIndex = this.MenuButtonList.Count-1;

        UpdateHovers(lastIndex);
    }

    private void UpdateHovers(int lastIndex){
        this.MenuButtonList[lastIndex].UnHoverMenuButton();
        this.MenuButtonList[this.buttonIndex].HoverMenuButton();

        ClearNextLVLTexts();
        
        if(this.buttonIndex == 3) HoverUpgradeWeapon();
        else if(this.buttonIndex == 4) HoverUpgradeArmor();

    }

    private void SelectOption(){
        if(this.backButtonHovered){
            this.GameHandler.LoadStartMenu();
            return;
        }

        switch(this.buttonIndex){
            case 0:
                this.GameHandler.LoadBattleScene();
            break;

            case 1:
                this.GameHandler.LoadStageSelect();
            break;

            case 2:
                this.GameHandler.LoadAbilityLoadOut();
            break;

            case 3:
                UpgradeWeapon();
            break;

            case 4:
                UpgradeArmor();
            break;

            case 5:
                this.GameHandler.LoadEnemyLibrary();
            break;

            default:
                Debug.LogError("Error: Button Index out of Range!");
            break;
        }
    }

    private void HoverBackButton(){
        GameHandler.PlaySwitchMenuOptionSound();
        
        if(this.backButtonHovered){
            this.MenuButtonList[this.buttonIndex].HoverMenuButton();
            this.BackButton.UnHoverMenuButton();
            this.backButtonHovered = false;
        }else{
            this.MenuButtonList[this.buttonIndex].UnHoverMenuButton();
            this.BackButton.HoverMenuButton();
            this.backButtonHovered = true;
        }
    }

    private void HoverUpgradeWeapon(){
        if(GameHandler.PlayerWeapon.weaponLevel >= GameHandler.PlayerWeapon.maxWeaponLevel) return;
        this.CreditsCostText.text = "- " + GameHandler.PlayerWeapon.upgradeCost.ToString()+" cp";

        WeaponUpgrade up = GameHandler.PlayerWeapon.GetUpgradeTable()[GameHandler.PlayerWeapon.weaponLevel];
        this.NextATKText.text = ">\t" + up.baseAttackMin.ToString() + " - " + up.baseAttackMax.ToString();
    }

    private void UpgradeWeapon(){
        if(GameHandler.earnedCredits < GameHandler.PlayerWeapon.upgradeCost) return;

        GameHandler.earnedCredits -= GameHandler.PlayerWeapon.upgradeCost;

        GameHandler.PlayerWeapon.UpgradeWeapon();

        UpdateAllTexts();
        HoverUpgradeWeapon();
    }

    private void HoverUpgradeArmor(){
        if(GameHandler.PlayerArmor.armorLevel >= GameHandler.PlayerArmor.maxArmorLevel) return;
        this.CreditsCostText.text = "- " + GameHandler.PlayerArmor.upgradeCost.ToString()+" cp";

        this.NextHPText.text = ">\t" + GameHandler.PlayerArmor.GetUpgradeTable()[GameHandler.PlayerArmor.armorLevel, 0].ToString();
    }

    private void UpgradeArmor(){
        if(GameHandler.earnedCredits < GameHandler.PlayerArmor.upgradeCost) return;

        GameHandler.earnedCredits -= GameHandler.PlayerArmor.upgradeCost;

        GameHandler.PlayerArmor.UpgradeArmor();

        UpdateAllTexts();
        HoverUpgradeArmor();
    }
}
