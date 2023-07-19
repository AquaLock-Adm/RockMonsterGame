using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public GameObject StartMenu;
    public GameObject CharacterSelectMenu;
    public GameObject MainMenu;
    public GameObject DungeonStartMenu;

    public GameHandler GameHandler;

    void Start(){
        this.GameHandler = GameObject.Find("Game Handler").GetComponent<GameHandler>();

        SetupStartMenu();
        SetupCharacterSelectMenu();
        SetupMainMenu();
        SetupDungeonStartMenu();

        
        MainMenuHandler MMH = MainMenu.GetComponent<MainMenuHandler>();
        MMH.SkillSelectScreen.SetActive(false);
        MMH.UnloadDropDownMenu(MMH.WeaponDropDownMenu);
        MMH.UnloadDropDownMenu(MMH.ArmorDropDownMenu);
        
        this.MainMenu.SetActive(false);
        this.DungeonStartMenu.SetActive(false);

        this.StartMenu.SetActive(true);

        if(this.GameHandler.newRunAvailable){
            this.GameHandler.ReturnFromBattleScene();
        }
    }

    private void SetupStartMenu(){
        this.StartMenu.SetActive(false);
        this.StartMenu.GetComponent<StartMenuHandler>().GameHandler = this.GameHandler;
    }

    private void SetupCharacterSelectMenu(){
        this.CharacterSelectMenu.SetActive(false);
        this.CharacterSelectMenu.GetComponent<CharacterSelectHandler>().GameHandler = this.GameHandler;
    }

    private void SetupMainMenu(){
        this.MainMenu.SetActive(false);
        this.MainMenu.GetComponent<MainMenuHandler>().GameHandler = this.GameHandler; 
    }

    private void SetupDungeonStartMenu(){
        this.DungeonStartMenu.SetActive(false);
        this.DungeonStartMenu.GetComponent<DungeonStartHandler>().GameHandler = this.GameHandler; 
    }

    public void LoadStartMenu(GameObject LastMenu){
        LastMenu.SetActive(false);
        this.StartMenu.SetActive(true);
    }

    public void LoadMainMenu(GameObject LastMenu){
        LastMenu.SetActive(false);
        this.MainMenu.SetActive(true);
        this.MainMenu.GetComponent<MainMenuHandler>().Setup();
    }

    public void LoadCharacterSelectMenu(GameObject LastMenu){
        LastMenu.SetActive(false);
        this.CharacterSelectMenu.SetActive(true);
        this.CharacterSelectMenu.GetComponent<CharacterSelectHandler>().Setup();
    }

    public void LoadDungeonStartMenu(){
        this.DungeonStartMenu.SetActive(true);
        this.DungeonStartMenu.GetComponent<DungeonStartHandler>().Setup();
    }
}
