using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuHandler : MonoBehaviour
{
    public GameHandler GameHandler;
    public MenuHandler MenuHandler;

    [SerializeField] public GameObject ContinueButton;
    [SerializeField] public GameObject NewGameButton;
    [SerializeField] public GameObject QuitGameButton;

    void Start(){

        if(!GameHandler.SaveDataAvailable()) ContinueButton.gameObject.SetActive(false);
    }

    public void Continue(){

        this.MenuHandler.LoadMainMenu(this.gameObject);
    }

    public void NewGame(){

        this.MenuHandler.LoadCharacterSelectMenu(this.gameObject);
    }

    public void QuitGame(){
        Debug.Log("Quit Game Called");
    }
}