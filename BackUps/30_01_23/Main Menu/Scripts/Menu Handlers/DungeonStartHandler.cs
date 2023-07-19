using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonStartHandler : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI WeaponText;
    [SerializeField] public TextMeshProUGUI ArmorText;

    [SerializeField] public GameObject WarningScreen;
    [SerializeField] public TextMeshProUGUI WarningText;

    public GameHandler GameHandler;
    public MenuHandler MenuHandler;

    private PlayerCharacter Player;
    private Weapon PlayerWeapon;
    private Armor PlayerArmor;

    private bool warningEnabled = false;

    void Awake(){ this.WarningScreen.SetActive(false); }

    void Update(){ if(this.gameObject.activeSelf) CheckInputs(); }

    public void Setup(){
        SetPlayerReferencesFromGameHandler();
        SetWeaponText();
        SetArmorText();
    }

    private void SetPlayerReferencesFromGameHandler(){
        this.Player = this.GameHandler.Player;
        this.PlayerWeapon = this.GameHandler.PlayerWeapon;
        this.PlayerArmor = this.GameHandler.PlayerArmor;
    }

    private void SetWeaponText(){
        if(this.PlayerWeapon != null) this.WeaponText.text = "Weapon: " + this.PlayerWeapon.weaponName;
        else this.WeaponText.text = "Weapon: NO WEAPON SET!";
    }

    private void SetArmorText(){
        if(this.PlayerArmor != null) this.ArmorText.text = "Armor: " + this.PlayerArmor.armorName;
        else this.ArmorText.text = "Armor: NO ARMOR SET!";
    }

    public void StartDungeon(){

        if(this.PlayerArmor == null || this.PlayerWeapon == null) EnableWarning();
        else this.GameHandler.StartDungeon();
    }

    public void Cancel(){

        this.MenuHandler.LoadMainMenu(this.gameObject);
    }

    private void EnableWarning(){
        this.WarningScreen.SetActive(true);
        this.WarningText.text = "Your Equipment is not completely set!\nDo you wish to continue anyway?";
        this.warningEnabled = true;
    }

    public void DisableWarning(){
        this.warningEnabled = false;
        this.WarningScreen.SetActive(false);
    }

    private void CheckInputs(){
        if(!this.warningEnabled){
            if(Input.GetKeyDown(KeyCode.A)) StartDungeon();
            else if(Input.GetKeyDown(KeyCode.D)) Cancel();
        } else{
            if(Input.GetKeyDown(KeyCode.A)) this.GameHandler.StartDungeon();
            else if(Input.GetKeyDown(KeyCode.D)) DisableWarning();
        }
    }
}
