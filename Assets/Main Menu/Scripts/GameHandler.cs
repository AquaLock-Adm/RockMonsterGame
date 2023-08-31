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

    public int earnedCredits;

    void Awake(){
        DontDestroyOnLoad(this.gameObject);
    }

    void OnSceneLoaded (Scene scene, LoadSceneMode mode){
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

            case "Battle Scene":
                GameObject.Find("BattleSystem").GetComponent<BattleSystem>().GameStart(this);
            break;

            default:
                Debug.LogError("Setup called in unexpected Scene: "+ SceneManager.GetActiveScene().name);
            break;
        }
    }

    public void LoadTutorial(){
        SceneManager.LoadScene("Tutorial");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadBattleScene(){
        SceneManager.LoadScene("Battle Scene");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadMainMenu(){
        SceneManager.LoadScene("Main Menu");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void LoadStartMenu(){
        SceneManager.LoadScene("Start Menu");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void SetPlayer(PlayerCharacter P){
        if(this.Player != null) return;

        this.Player = P;
        DontDestroyOnLoad(P.gameObject);

        this.PlayerWeapon = P.GetWeapon();
        DontDestroyOnLoad(this.PlayerWeapon.gameObject);
        this.PlayerArmor = P.GetArmor();
        DontDestroyOnLoad(this.PlayerArmor.gameObject); 
    }
}
