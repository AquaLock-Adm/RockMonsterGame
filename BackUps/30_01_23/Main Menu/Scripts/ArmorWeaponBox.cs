using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArmorWeaponBox : MonoBehaviour
{
    [SerializeField] public Weapon Weapon = null;
    [SerializeField] public Armor Armor = null;

    [SerializeField] public TextMeshProUGUI NameText;

    public void SetupArmor(Armor A){
        if(A == null){
            Debug.LogError("ERROR Setup with null");
            return;
        }
        this.Armor = A;
        SetupNameText();
    }

    public void SetupWeapon(Weapon W){
        if(W == null){
            Debug.LogError("ERROR Setup with null");
            return;
        }
        this.Weapon = W;
        SetupNameText();

    }

    private void SetupNameText(){
        if(this.Armor != null){
            this.NameText.text = this.Armor.armorName;
        }else {
            this.NameText.text = this.Weapon.weaponName;
        }
    }

    public void SetWeapon(){
        MainMenuHandler MMH = GameObject.Find("Main Menu Options").GetComponent<MainMenuHandler>();
        MMH.SelectWeapon(this.Weapon);
    }

    public void SetArmor(){
        MainMenuHandler MMH = GameObject.Find("Main Menu Options").GetComponent<MainMenuHandler>();
        MMH.SelectArmor(this.Armor);
    }
}
