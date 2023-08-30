using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    [HideInInspector] public PlayerCharacter Player;
    [HideInInspector] public Weapon PlayerWeapon;
    [HideInInspector] public Armor PlayerArmor;

    [HideInInspector] public BattleSystem BattleSystem;

    public int earnedCredits;

    private void OnSceneLoad(){
        SceneSetups();
    }

    private void SceneSetups(){
        switch(SceneManager.GetActiveScene().name){
            case "Start Menu":
                GameObject.Find("StartMenuHandler").GetComponent<StartMenuHandler>().StartSetup(this);
            break;

            case "Main Menu":
                GameObject.Find("MenuHandler").GetComponent<NewMenuHandler>().StartSetup(this);
            break;

            default:
                Debug.LogError("Setup called in unexpected Scene: "+ SceneManager.GetActiveScene().name);
            break;
        }
    }
}
