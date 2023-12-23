using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class StandartMenuHandler : MonoBehaviour
{
	[Header("Standart Menu Data")]
    [SerializeField] private GameObject GameHandlerPrefab;
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject WeaponPrefab;
    [SerializeField] private GameObject ArmorPrefab;

    protected GameHandler GameHandler;
    protected PlayerCharacter Player;

    void Awake(){
        GameObject GHGO = GameObject.Find("GameHandler");

        if(GHGO == null){
            if(this.GameHandlerPrefab == null){
                Debug.LogError("No Game Handler Prefab Set!");
                return;
            }

            GHGO = Instantiate(this.GameHandlerPrefab);
            GHGO.name = "GameHandler";
        }
    }

    void Update(){
        CheckPlayerInput();
    }

    public virtual void StartSetup(GameHandler GH){
        this.GameHandler = GH;
        if(this.GameHandler.Player == null){
            CreateNewPlayer();
        }else{
            Player = GameHandler.Player;
        }
    }

    protected virtual void CheckPlayerInput(){

    }

    private void CreateNewPlayer(){
        Debug.Log("Creating new player");
        
        Player = Instantiate(PlayerPrefab).GetComponent<PlayerCharacter>();

        Weapon PlayerWeapon = Instantiate(WeaponPrefab).GetComponent<Weapon>();
        this.GameHandler.PlayerWeapon = PlayerWeapon;
        Armor PlayerArmor = Instantiate(ArmorPrefab).GetComponent<Armor>();
        this.GameHandler.PlayerArmor = PlayerArmor;
        Player.MenuSetup(PlayerWeapon, PlayerArmor);
        
        this.GameHandler.SetPlayer(Player);
        this.GameHandler.SetAbilitiesList = Player.GetStandartAbilitiesList();

        this.GameHandler.UnlockedAbilitiesList = new List<Action>();
        foreach(Action A in this.GameHandler.SetAbilitiesList){
            this.GameHandler.UnlockedAbilitiesList.Add(A.Copy());
        }
    }

}
