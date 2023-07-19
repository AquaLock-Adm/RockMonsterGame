using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuHandler : MonoBehaviour
{

    public GameHandler GameHandler;

    [SerializeField] private int creditCrawlTime = 1000; // ms 

    [Header("Reminders For Menu Hovers")]
    [SerializeField] private int actualWeaponLevel = 1;
    [SerializeField] private int actualArmorLevel = 1;

    [Header("Character Screen References")]
    [SerializeField] private TextMeshProUGUI CharacterNameText;

    [SerializeField] private TextMeshProUGUI WeaponNameText;
    [SerializeField] private TextMeshProUGUI WeaponLevelText;
    [SerializeField] private TextMeshProUGUI WeaponAPRText;
    [SerializeField] private TextMeshProUGUI WeaponDmgText;
    [SerializeField] private TextMeshProUGUI WeaponDurText;

    [SerializeField] private TextMeshProUGUI ArmorNameText;
    [SerializeField] private TextMeshProUGUI ArmorLevelText;
    [SerializeField] private TextMeshProUGUI ArmorHpText;
    [SerializeField] private TextMeshProUGUI ArmorShieldText;

    [Header("Credit Screen References")]
    [SerializeField] private TextMeshProUGUI CreditMainText;
    [SerializeField] private TextMeshProUGUI CreditGainText;

    void Start(){
        SetTotalCredits(this.GameHandler.totalCredits);
    }

#region Set Functions
    public void SetPlayerCharacter(PlayerCharacter P){
        if(P != null) this.CharacterNameText.text = P.unitName;
        else this.CharacterNameText.text = "";

        Weapon PlayerWeapon = P.GetWeapon();
        Armor PlayerArmor = P.GetArmor();

        if(PlayerWeapon != null) {
            this.actualWeaponLevel = PlayerWeapon.weaponLevel;
            SetPlayerWeapon(PlayerWeapon);
        }
        else SetPlayerWeapon(null);
        if(PlayerArmor != null) {
            this.actualArmorLevel = PlayerArmor.armorLevel;
            SetPlayerArmor(PlayerArmor);
        }
        else SetPlayerArmor(null);
    }
    public void SetPlayerWeapon(Weapon W){
        if(W != null){
            this.WeaponNameText.text = W.name;
            this.WeaponLevelText.text = W.weaponLevel.ToString();
            this.WeaponAPRText.text = W.actionsPerRound.ToString();
            this.WeaponDmgText.text = W.baseAttackMin.ToString() + " - " + W.baseAttackMax.ToString();
        }else {
            this.WeaponNameText.text = "";
            this.WeaponLevelText.text = "";
            this.WeaponAPRText.text = "";
            this.WeaponDmgText.text = "";
            this.WeaponDurText.text = "";
        }
    }
    public void SetPlayerArmor(Armor A){
        if(A != null){
            this.ArmorNameText.text = A.name;
            this.ArmorLevelText.text = A.armorLevel.ToString();
            this.ArmorHpText.text = A.maxHealthPoints.ToString();
            this.ArmorShieldText.text = "";
        }else {
            this.ArmorNameText.text = "";
            this.ArmorLevelText.text = "";
            this.ArmorHpText.text = "";
            this.ArmorShieldText.text = "";
        }
    }

    public void SetTotalCredits(int credits){
        this.CreditMainText.text = FormatIntegerCount(credits) +" Cd";
    }
    public void SetCreditGainText(int credits){
        if(credits >= 0) this.CreditGainText.text = "+ "+FormatIntegerCount(credits) +" Cd";
        else this.CreditGainText.text = "- "+FormatIntegerCount(credits*-1) +" Cd";
    }
    public void ClearCreditGainText(){
        this.CreditGainText.text = "";
    }
#endregion

    public async Task ShowCreditGain(int creditGain, int delayMS = 0){
        // this is only the visualization the actual totalCredit score has already been lowered or raised
        this.GameHandler.MenuInputHandler.disableInput = true;
        int initCreditScore = this.GameHandler.totalCredits - creditGain;
        SetCreditGainText(creditGain);
        
        await Task.Delay(delayMS);

        if(creditGain != 0){
            int tickRate = 50;

            while((int)Mathf.Abs(creditGain) < (int)Mathf.Round((float)this.creditCrawlTime / (float)tickRate)){
                tickRate++;
            }

            int pointsPerTick = (int)Mathf.Round(Mathf.Abs(creditGain)/((float)this.creditCrawlTime/(float)tickRate));

            // Debug.Log(creditGain.ToString()+"Cd in "+this.creditCrawlTime.ToString()+"ms. "+pointsPerTick.ToString()+"Cd every "+ tickRate.ToString()+"ms.");
            
            int cDepletion = 0;
            int timeWaited = 0;
            
            while(cDepletion <= (int)Mathf.Abs(creditGain)){
                int timeLoss = (int)Mathf.Round(Time.deltaTime*1000);
                await Task.Delay(tickRate-timeLoss);
                // await Task.Delay(tickRate);
                timeWaited += tickRate;

                if(creditGain >= 0){
                    SetTotalCredits(initCreditScore + cDepletion);
                    SetCreditGainText(creditGain - cDepletion);
                }else {
                    SetTotalCredits(initCreditScore - cDepletion);
                    SetCreditGainText(creditGain + cDepletion);
                }

                cDepletion += pointsPerTick;
            }
            if(cDepletion != (int)Mathf.Abs(creditGain)) {
                if(creditGain >= 0) SetTotalCredits(this.GameHandler.totalCredits);
                else SetTotalCredits(this.GameHandler.totalCredits);
            }

            await Task.Delay((int)Mathf.Clamp(this.creditCrawlTime - timeWaited, 0.0f, this.creditCrawlTime));
        }

        ClearCreditGainText();
        this.GameHandler.MenuInputHandler.disableInput = false;
    }

    private string FormatIntegerCount(int num){
        string numString = num.ToString();

        int i = 0;
        int signCount = 0;
        while(i + signCount < numString.Length){
            if(i != 0){
                numString = numString.Insert(numString.Length - i - signCount, ",");
                signCount++;
            }
            i += 3;
        }
        return numString;
    }

#region Button Functions
    public void HoverOption(int optionIndex){
        UnhoverAllOptions();
        switch(optionIndex){
            case 1:
                UpgradeWeaponHover();
            break;

            case 2:
                UpgradeArmorHover();
            break;
        }
    }

    public void ClickOption(int optionIndex){
        switch(optionIndex){
            case 0:
                GameStartClick();
            break;

            case 1:
                UpgradeWeaponClick();
            break;
            case 2:
                UpgradeArmorClick();
            break;

            case 3:
                EndGameClick();
            break;
        }
    }

    private void UnhoverAllOptions(){
        UpgradeWeaponUnhover();
        UpgradeArmorUnhover();
    }

    public void GameStartClick(){
        this.GameHandler.StartGame();
    }

    public async void UpgradeWeaponClick(){
        Weapon PlayerWeapon = this.GameHandler.PlayerWeapon;
        if(this.actualWeaponLevel >= PlayerWeapon.maxWeaponLevel) return;
        
        PlayerWeapon.GetUpgrade(this.actualWeaponLevel);

        if(this.GameHandler.totalCredits >= PlayerWeapon.upgradeCost){

            this.GameHandler.totalCredits -= PlayerWeapon.upgradeCost;
            await ShowCreditGain(PlayerWeapon.upgradeCost*-1);

            PlayerWeapon.GetUpgrade(this.actualWeaponLevel+1);
            this.actualWeaponLevel++;
        }
        
        UpgradeWeaponHover();
    }
    private void UpgradeWeaponHover(){
        Weapon PlayerWeapon = this.GameHandler.PlayerWeapon;
        if(this.actualWeaponLevel >= PlayerWeapon.maxWeaponLevel) return;

        SetCreditGainText(PlayerWeapon.upgradeCost*-1);
        PlayerWeapon.GetUpgrade(this.actualWeaponLevel+1);
        SetPlayerWeapon(PlayerWeapon);
    }
    private void UpgradeWeaponUnhover(){
        Weapon PlayerWeapon = this.GameHandler.PlayerWeapon;

        ClearCreditGainText();
        PlayerWeapon.GetUpgrade(this.actualWeaponLevel);
        SetPlayerWeapon(PlayerWeapon);
    }

    public async void UpgradeArmorClick(){
        Armor PlayerArmor = this.GameHandler.PlayerArmor;
        if(this.actualArmorLevel >= PlayerArmor.maxArmorLevel) return;
        
        PlayerArmor.GetUpgrade(this.actualArmorLevel);

        if(this.GameHandler.totalCredits >= PlayerArmor.upgradeCost){

            this.GameHandler.totalCredits -= PlayerArmor.upgradeCost;
            await ShowCreditGain(PlayerArmor.upgradeCost*-1);

            PlayerArmor.GetUpgrade(this.actualArmorLevel+1);
            this.actualArmorLevel++;
        }
        
        UpgradeArmorHover();
    }
    private void UpgradeArmorHover(){
        Armor PlayerArmor = this.GameHandler.PlayerArmor;
        if(this.actualArmorLevel >= PlayerArmor.maxArmorLevel) return;

        SetCreditGainText(PlayerArmor.upgradeCost*-1);
        PlayerArmor.GetUpgrade(this.actualArmorLevel+1);
        SetPlayerArmor(PlayerArmor);
    }
    private void UpgradeArmorUnhover(){
        Armor PlayerArmor = this.GameHandler.PlayerArmor;

        ClearCreditGainText();
        PlayerArmor.GetUpgrade(this.actualArmorLevel);
        SetPlayerArmor(PlayerArmor);
    }

    public void EndGameClick(){
        Debug.Log("EndGameClick() called");
        Application.Quit();
    }
#endregion
}
