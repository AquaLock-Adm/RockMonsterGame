using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    [Header("Menu References")]
    [SerializeField] public MenuHandler MenuHandler;
    [SerializeField] public MenuInputHandler MenuInputHandler;

    [Header("Static Data")]
    public GameObject PlayerPrefab;
    public GameObject WeaponPrefab;
    public GameObject ArmorPrefab;
    public int totalCredits = 0;
    private static GameHandler GameHandlerInstance;

    [Header("Data into Dungeon")]
    [SerializeField] public PlayerCharacter Player;
    [SerializeField] public Weapon PlayerWeapon;
    [SerializeField] public Armor PlayerArmor;

    public BattleSystem BattleSystem;

    [Header("Data from Dungeon")]
    [SerializeField] public int earnedCredits = 0;

    void Awake(){
        DontDestroyOnLoad(this.gameObject);

        if(GameHandlerInstance == null){
            GameHandlerInstance = this;
            FirstStart();
        } else {
            Destroy(this.gameObject);
            return;
        }
    }

    void OnEnable(){
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    async void OnSceneLoaded(Scene scene, LoadSceneMode mode){
        if(scene.name == "Main Menu") {
            SetupMenuReferences();
            this.Player.SetWeapon(this.PlayerWeapon);
            this.Player.SetArmor(this.PlayerArmor);
            this.MenuHandler.SetPlayerCharacter(this.Player);

            this.totalCredits += this.earnedCredits;
            await this.MenuHandler.ShowCreditGain(this.earnedCredits);
            this.earnedCredits = 0;
        }
    }

    private void FirstStart(){
        SetupMenuReferences();
        CreateNewPlayer();
        this.MenuHandler.SetPlayerCharacter(this.Player);
    }

    private void SetupMenuReferences(){
        this.MenuHandler = GameObject.Find("MenuHandler").GetComponent<MenuHandler>();
        this.MenuHandler.GameHandler = this;
        this.MenuInputHandler = GameObject.Find("MenuInputHandler").GetComponent<MenuInputHandler>();
        this.MenuInputHandler.MenuHandler = this.MenuHandler;
    }

    private void CreateNewPlayer(){
        this.Player = this.PlayerPrefab.GetComponent<PlayerCharacter>();
        Weapon WeaponFromPrefab = this.WeaponPrefab.GetComponent<Weapon>();
        Armor ArmorFromPrefab = this.ArmorPrefab.GetComponent<Armor>();

        this.Player.SetWeapon(WeaponFromPrefab);
        this.Player.SetArmor(ArmorFromPrefab);
        
        WeaponFromPrefab.Init(this.Player);
        ArmorFromPrefab.Init(this.Player);

        this.PlayerWeapon = WeaponFromPrefab;
        this.PlayerArmor = ArmorFromPrefab;
    }

    public void StartGame(){
        SceneManager.LoadScene("Battle Scene");
    }
}
