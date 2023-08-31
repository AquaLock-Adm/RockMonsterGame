using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Armor : MonoBehaviour
{
    public string armorName;
    public PlayerCharacter Player;

    public int armorLevel = 1;
    public int maxArmorLevel = 10;

    public int upgradeCost = 0;

    public int healthPoints;
    public int maxHealthPoints;

    public int mana = 0;
    public int maxMana = 0;
    public int manaRegen_s = 0;


    public void Init(PlayerCharacter PC){
        // called when created by GameHandler at Start of Game
        SetBaseStats();
        SetPlayer(PC);
    }

    public void SetPlayer(PlayerCharacter PC){
        this.Player = PC;
        PC.SetArmor(this);
    }

    private void SetBaseStats(){
        int[,] upgradeTable = this.GetUpgradeTable();
        this.maxArmorLevel = upgradeTable.GetLength(0);
        UpgradeArmorToLevel(1);
    }

    public void UpgradeArmor(){
        UpgradeArmorToLevel(this.armorLevel+1);
    }

    public void DowngradeArmor(){
        UpgradeArmorToLevel(this.armorLevel-1);
    }

    public void UpgradeArmorToLevel(int level){
        if(level > this.maxArmorLevel || level <= 0) return;

        this.armorLevel = level;

        int[,] upgradeTable = this.GetUpgradeTable();

        this.maxHealthPoints = upgradeTable[this.armorLevel-1, 0];
        this.healthPoints = this.maxHealthPoints;

        this.maxMana = upgradeTable[this.armorLevel-1, 1];
        this.mana = this.maxMana;
        this.manaRegen_s = upgradeTable[this.armorLevel-1, 2];

        this.upgradeCost = upgradeTable[this.armorLevel-1, 3];

        if(this.Player != null) SetPlayer(this.Player);

        // PrintStatus();
    }

    public int[,] GetUpgradeTable(){
        int[,] table = new int[,]
        {
            // {hp, mana, manaRegen, UpgradeCost}
            { 50,   0, 0,  500},
            {110, 100, 1, 1500},
            {190, 200, 2, 3800},
            {260, 400, 4, 7000},
            {330, 800, 8, 0}
        };

        return table;
    }

    public void Repair(int val){
        if(val < 0 ) Debug.Log("Healing for negative amount. Use DealDamage instead?");
        this.healthPoints = (int)Mathf.Clamp(this.healthPoints + val, 0.0f, (float)this.maxHealthPoints);
        this.Player.healthPoints = this.healthPoints;
    }

    public void CopyFrom(Armor Other){
        this.armorName = Other.armorName;
        this.Player = Other.Player;

        this.armorLevel = Other.armorLevel;
        this.maxArmorLevel = Other.maxArmorLevel;

        this.upgradeCost = Other.upgradeCost;

        this.healthPoints = Other.healthPoints;
        this.maxHealthPoints = Other.maxHealthPoints;
    }

    public void PrintStatus(){
        Debug.Log("Status Armor " + this.armorName);

        string s = "Player: " + this.Player.unitName+"\n\n"

                +  "Level: " + this.armorLevel.ToString()+"\n\n"

                +  "Upgrade Cost: "+ this.upgradeCost.ToString()+"\n\n"

                +  "HealthPoints: " + this.healthPoints.ToString()+"\n"
                +  "MaxHealthPoints: " + this.maxHealthPoints.ToString()+"\n";

        Debug.Log(s);
    }
}
