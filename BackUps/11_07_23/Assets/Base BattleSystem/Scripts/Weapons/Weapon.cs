using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum WeaponType {SWORD, SCYTHE, AXE, DAGGER, NONE} // bsvg1

// wc0
public abstract class Weapon : MonoBehaviour
{   
    // wvg1
    public string weaponName;

    public WeaponType Type;

    // wvg2
    public PlayerCharacter Player;

    public int weaponLevel = 1;
    public int maxWeaponLevel = 5;
    public int upgradeCost = 0;

    public int actionsPerRound = 3;

    public List<Action> Abilities;

    // wvg3
    public int baseAttackMax;
    public int baseAttackMin;
    public int attackMax;
    public int attackMin;

    public int baseCrit;
    public int crit;

    public int baseLifeSteal; // in percent
    public int lifeSteal;

    public int baseHealthCostPerSecond;
    public int healthCostPerSecond;

    public int enemyDamagePerSecond;

    // wf1
    public abstract void InitAbilities();

    public abstract List<Action> GetCompleteMoveList();
    // wf2
    public abstract Action CombineActions(string comboList, List<Action> Action_l);
    // wf3
    public abstract List<Action> GetMovesToTest();
    // wf4
    public abstract void SetWeaponType();

    public abstract int[,] GetUpgradeTable();

    public abstract Action GetAttackRushFinisher(int finisherLevel);

    public void Init(PlayerCharacter PC){
        // called when created by GameHandler at Start of Game
        SetBaseStats();
        SetWeaponType();
        this.Player = PC; // InitAbilities needs a Player
        InitAbilities();
        SetPlayer(PC);
    }

    public void SetPlayer(PlayerCharacter PC){
        this.Player = PC;
        PC.SetWeapon(this);
    }

    private void SetBaseStats(){
        int[,] upgradeTable = this.GetUpgradeTable();
        this.maxWeaponLevel = upgradeTable.GetLength(0);
        GetUpgrade(1);
    }

    public void GetUpgrade(int level){
        if(level > this.maxWeaponLevel || level <= 0) return;

        this.weaponLevel = level;

        int[,] upgradeTable = this.GetUpgradeTable();

        this.baseAttackMin = upgradeTable[this.weaponLevel-1, 0];
        this.attackMin = this.baseAttackMin;
        this.baseAttackMax = upgradeTable[this.weaponLevel-1, 1];
        this.attackMax = this.baseAttackMax;

        this.actionsPerRound = upgradeTable[this.weaponLevel-1, 2];

        this.upgradeCost = upgradeTable[this.weaponLevel-1, 4];

        // PrintStatus();
    }

    // wf11
    public void CopyFrom(Weapon Other){
        this.weaponName = Other.weaponName;
        this.Type = Other.Type;
        this.weaponLevel = Other.weaponLevel;
        this.maxWeaponLevel = Other.maxWeaponLevel;
        this.upgradeCost = Other.upgradeCost;
        this.actionsPerRound = Other.actionsPerRound;
        this.Abilities = Other.Abilities;
        this.baseAttackMax = Other.baseAttackMax;
        this.baseAttackMin = Other.baseAttackMin;
        this.attackMax = Other.attackMax;
        this.attackMin = Other.attackMin;
        SetPlayer(Other.Player);
    }

    public void PrintStatus(){
        Debug.Log("Status Weapon "+this.weaponName);
        string s = "Type: " + this.Type.ToString()+"\n"
                +  "Player: " + this.Player.GetUnitName()+"\n\n"

                +  "Level: " + this.weaponLevel.ToString()+"\n\n"
                
                +  "AttackMax: " + this.attackMax.ToString()+"\n"
                +  "AttackMin: " + this.attackMin.ToString()+"\n\n"

                +  "APR: "+ this.actionsPerRound.ToString()+"\n\n"

                +  "Upgrade Cost: "+ this.upgradeCost.ToString()+" Cd\n";

        Debug.Log(s);
    }
}