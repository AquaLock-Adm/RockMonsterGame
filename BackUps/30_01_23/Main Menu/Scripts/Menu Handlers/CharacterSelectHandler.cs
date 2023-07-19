using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectHandler : MonoBehaviour
{
    public GameHandler GameHandler;
    public MenuHandler MenuHandler;

    public GameObject CharacterPrefabM;
    public GameObject CharacterPrefabL;
    public GameObject CharacterPrefabHo;
    public GameObject CharacterPrefabK;
    public GameObject CharacterPrefabHa;

    public GameObject CharacterSelectScreenM;
    public GameObject CharacterSelectScreenL;
    public GameObject CharacterSelectScreenHo;
    public GameObject CharacterSelectScreenK;
    public GameObject CharacterSelectScreenHa;

    [Header("Character Select Screen Maria")]
    [SerializeField] public TextMeshProUGUI CharacterNameTextM;
    [SerializeField] public TextMeshProUGUI WeaponTextM;
    [SerializeField] public TextMeshProUGUI ManaTextM;

    [Header("Character Select Screen Lillia")]
    [SerializeField] public TextMeshProUGUI CharacterNameTextL;
    [SerializeField] public TextMeshProUGUI WeaponTextL;
    [SerializeField] public TextMeshProUGUI ManaTextL;

    [Header("Character Select Screen Hoover")]
    [SerializeField] public TextMeshProUGUI CharacterNameTextHo;
    [SerializeField] public TextMeshProUGUI WeaponTextHo;
    [SerializeField] public TextMeshProUGUI ManaTextHo;

    [Header("Character Select Screen Karmina")]
    [SerializeField] public TextMeshProUGUI CharacterNameTextK;
    [SerializeField] public TextMeshProUGUI WeaponTextK;
    [SerializeField] public TextMeshProUGUI ManaTextK;

    [Header("Character Select Screen Harpies")]
    [SerializeField] public TextMeshProUGUI CharacterNameTextHa;
    [SerializeField] public TextMeshProUGUI WeaponTextHa;
    [SerializeField] public TextMeshProUGUI ManaTextHa;

    private PlayerCharacter Maria;
    private PlayerCharacter Lillia;
    private PlayerCharacter Hoover;
    private PlayerCharacter Karmina;
    private PlayerCharacter Harpies;

    public void Setup(){
        if(this.CharacterPrefabM != null) SetupCharacterSelectScreenM();
        else CharacterSelectScreenM.SetActive(false);

        if(this.CharacterPrefabL != null) SetupCharacterSelectScreenL();
        else CharacterSelectScreenL.SetActive(false);

        if(this.CharacterPrefabHo != null) SetupCharacterSelectScreenHo();
        else CharacterSelectScreenHo.SetActive(false);

        if(this.CharacterPrefabK != null) SetupCharacterSelectScreenK();
        else CharacterSelectScreenK.SetActive(false);

        if(this.CharacterPrefabHa != null) SetupCharacterSelectScreenHa();
        else CharacterSelectScreenHa.SetActive(false);
    }

    public void SelectCharacterM(){
        this.GameHandler.SetNewPlayerCharacter(this.Maria);
        this.MenuHandler.LoadMainMenu(this.gameObject);
    }
    public void SelectCharacterL(){
        this.GameHandler.SetNewPlayerCharacter(this.Lillia);
        this.MenuHandler.LoadMainMenu(this.gameObject);
    }
    public void SelectCharacterHo(){
        this.GameHandler.SetNewPlayerCharacter(this.Hoover);
        this.MenuHandler.LoadMainMenu(this.gameObject);
    }
    public void SelectCharacterK(){
        this.GameHandler.SetNewPlayerCharacter(this.Karmina);
        this.MenuHandler.LoadMainMenu(this.gameObject);
    }
    public void SelectCharacterHa(){
        this.GameHandler.SetNewPlayerCharacter(this.Harpies);
        this.MenuHandler.LoadMainMenu(this.gameObject);
    }

    private void SetupCharacterSelectScreenM(){
        this.Maria = this.CharacterPrefabM.GetComponent<PlayerCharacter>();
        this.CharacterNameTextM.text = this.Maria.unitName;
        string weaponString = CharacterWeaponProficiencyToWeaponString(this.Maria);
        this.WeaponTextM.text = weaponString;
        this.ManaTextM.text = this.Maria.maxMana.ToString();
    }
    private void SetupCharacterSelectScreenL(){
        this.Lillia = this.CharacterPrefabL.GetComponent<PlayerCharacter>();
        this.CharacterNameTextL.text = this.Lillia.unitName;
        string weaponString = CharacterWeaponProficiencyToWeaponString(this.Lillia);
        this.WeaponTextL.text = weaponString;
        this.ManaTextL.text = this.Lillia.maxMana.ToString();
    }
    private void SetupCharacterSelectScreenHo(){
        this.Hoover = this.CharacterPrefabHo.GetComponent<PlayerCharacter>();
        this.CharacterNameTextHo.text = this.Hoover.unitName;
        string weaponString = CharacterWeaponProficiencyToWeaponString(this.Hoover);
        this.WeaponTextHo.text = weaponString;
        this.ManaTextHo.text = this.Hoover.maxMana.ToString();
    }
    private void SetupCharacterSelectScreenK(){
        this.Karmina = this.CharacterPrefabK.GetComponent<PlayerCharacter>();
        this.CharacterNameTextK.text = this.Karmina.unitName;
        string weaponString = CharacterWeaponProficiencyToWeaponString(this.Karmina);
        this.WeaponTextK.text = weaponString;
        this.ManaTextK.text = this.Karmina.maxMana.ToString();
    }
    private void SetupCharacterSelectScreenHa(){
        this.Harpies = this.CharacterPrefabHa.GetComponent<PlayerCharacter>();
        this.CharacterNameTextHa.text = this.Harpies.unitName;
        string weaponString = CharacterWeaponProficiencyToWeaponString(this.Harpies);
        this.WeaponTextHa.text = weaponString;
        this.ManaTextHa.text = this.Harpies.maxMana.ToString();
    }

    private string CharacterWeaponProficiencyToWeaponString(PlayerCharacter Character){
        string weaponString = "";

        if(Character.baseSwordExp >= 0) weaponString += "Sw, ";
        if(Character.baseScytheExp >= 0) weaponString += "Sc, ";
        if(Character.baseAxeExp >= 0) weaponString += "Ax, ";
        if(Character.baseDaggerExp >= 0) weaponString += "Dg, ";

        if(weaponString.Length > 0) weaponString = weaponString.Substring(0, weaponString.Length-2);

        return weaponString;
    }
}
