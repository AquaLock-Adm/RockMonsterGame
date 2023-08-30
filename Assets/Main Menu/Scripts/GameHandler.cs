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

    public GameObject PlayerPrefab;
    public GameObject WeaponPrefab;
    public GameObject ArmorPrefab;

    [HideInInspector] public BattleSystem BattleSystem;

    public int earnedCredits;

    private void Awake()
    {
        if(Player == null)
        {
            CreateNewPlayer();
        }
    }
    private void CreateNewPlayer() 
    {
        Player = Instantiate(PlayerPrefab).GetComponent<PlayerCharacter>();
        PlayerWeapon = Instantiate(WeaponPrefab).GetComponent<Weapon>();
        PlayerArmor = Instantiate(ArmorPrefab).GetComponent<Armor>();
        Player.MenuSetup(PlayerWeapon, PlayerArmor);
    }
}
