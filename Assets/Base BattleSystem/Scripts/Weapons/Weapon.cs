using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponUpgrade
{
    public WeaponUpgrade(int baseAttackMin, int baseAttackMax, int actionsPerRound, float healthCostPerSecond, float enemyDamagePerSecond, int upgradeCost){
        this.baseAttackMin = baseAttackMin;
        this.baseAttackMax = baseAttackMax;
        this.actionsPerRound = actionsPerRound;
        this.healthCostPerSecond = healthCostPerSecond;
        this.enemyDamagePerSecond = enemyDamagePerSecond;
        this.upgradeCost = upgradeCost;
    }

    public int baseAttackMin;
    public int baseAttackMax;
    public int actionsPerRound;
    public float healthCostPerSecond;
    public float enemyDamagePerSecond;
    public int upgradeCost;
}

// wc0
public abstract class Weapon : MonoBehaviour
{   
    // wvg1
    public string weaponName;

    // wvg2
    public PlayerCharacter Player;

    public int weaponLevel = 1;
    public int maxWeaponLevel = 5;
    public int upgradeCost = 0;

    public int actionsPerRound = 3;

    public List<Action> Abilities;

    public List<Action> Blocks;

    // wvg3
    public int baseAttackMax;
    public int baseAttackMin;
    public int attackMax;
    public int attackMin;

    public int baseLifeSteal; // in percent
    public int lifeSteal;

    public float baseHealthCostPerSecond;
    public float healthCostPerSecond;

    public float enemyDamagePerSecond;

    // wf1
    public abstract void InitAbilities();

    public virtual void InitBlocks(){
        this.Blocks = new List<Action>();

        this.Blocks.Add(new LowBlock(this.Player));
        this.Blocks.Add(new MidBlock(this.Player));
        this.Blocks.Add(new HighBlock(this.Player));

        int abilityIndex_c = 0;

        foreach(Action A in this.Blocks){
            A.abilityIndex = abilityIndex_c;
            abilityIndex_c++;
        }
    }

    public abstract List<Action> GetCompleteMoveList();
    // wf2
    public abstract Action CombineActions(string comboList, List<Action> Action_l);
    // wf3
    public abstract List<Action> GetMovesToTest();

    public abstract List<WeaponUpgrade> GetUpgradeTable();

    public abstract Action GetAttackRushFinisher(int finisherLevel);

    public void Init(PlayerCharacter PC){
        // called when created by GameHandler at Start of Game
        SetBaseStats();
        this.Player = PC; // InitAbilities needs a Player
        InitAbilities();
        InitBlocks();
        SetPlayer(PC);
    }

    public void SetPlayer(PlayerCharacter PC){
        this.Player = PC;
        PC.SetWeapon(this);
    }

    private void SetBaseStats(){
        List<WeaponUpgrade> upgradeTable = this.GetUpgradeTable();
        this.maxWeaponLevel = upgradeTable.Count;
        UpgradeWeaponToLevel(1);
    }

    public void UpgradeWeapon(){
        UpgradeWeaponToLevel(this.weaponLevel+1);
    }

    public void DowngradeWeapon(){
        UpgradeWeaponToLevel(this.weaponLevel-1);
    }

    public void UpgradeWeaponToLevel(int level){
        if(level > this.maxWeaponLevel || level <= 0) return;

        this.weaponLevel = level;

        List<WeaponUpgrade> upgradeTable = this.GetUpgradeTable();

        this.baseAttackMin = upgradeTable[this.weaponLevel-1].baseAttackMin;
        this.attackMin = this.baseAttackMin;
        this.baseAttackMax = upgradeTable[this.weaponLevel-1].baseAttackMax;
        this.attackMax = this.baseAttackMax;

        this.actionsPerRound = upgradeTable[this.weaponLevel-1].actionsPerRound;

        this.baseHealthCostPerSecond = upgradeTable[this.weaponLevel-1].healthCostPerSecond;
        this.healthCostPerSecond = this.baseHealthCostPerSecond;

        this.enemyDamagePerSecond = upgradeTable[this.weaponLevel-1].enemyDamagePerSecond;

        this.upgradeCost = upgradeTable[this.weaponLevel-1].upgradeCost;

        // PrintStatus();
    }

    // wf11
    public void CopyFrom(Weapon Other){
        this.weaponName = Other.weaponName;
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
        string s = "Player: " + this.Player.unitName+"\n\n"

                +  "Level: " + this.weaponLevel.ToString()+"\n\n"
                
                +  "AttackMax: " + this.attackMax.ToString()+"\n"
                +  "AttackMin: " + this.attackMin.ToString()+"\n\n"

                +  "APR: "+ this.actionsPerRound.ToString()+"\n\n"

                +  "Upgrade Cost: "+ this.upgradeCost.ToString()+" Cd\n";

        Debug.Log(s);
    }
}