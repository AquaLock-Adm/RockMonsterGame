using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Armor : MonoBehaviour
{
    public string armorName;
    public PlayerCharacter User;

    public int durability;
    public int maxDurability;
    public int shield;
    public int shieldRegenRate;
    /*
    Stab res
    Slash res
    Blunt res
    Elem Res
    Elem Weakness[]
    Elem Advantage[]
    */

    /*
    Durability of armor + shield of armor solely determine how much hp the player character has
    invest in good armor people!
    shield refreshes with shield refresh rate(in ms) and can be created by spells
    */
    void Awake(){
        if(this.shieldRegenRate > 0)ShieldRegenLoop();
    }

    public void Setup(PlayerCharacter PC){
        User = PC;
        PC.Armor = this;
        SetHealthPoints();
        SetShield();
    }

    private void SetHealthPoints(){
        User.maxHealthPoints = maxDurability;
        User.healthPoints = durability;
    }

    private void SetShield(){
        User.maxShield = shield;
        User.shield = shield;
    }

    public async void ShieldRegenLoop(){
        while(true){
            await Task.Delay(shieldRegenRate);
            if(User.shield < this.shield) User.shield = this.shield;
        }
    }

    public void Repair(int val){
        if(val < 0 ) Debug.Log("Healing for negative amount. Use DealDamage instead?");

        this.durability = (int)Mathf.Clamp(this.durability + val, 0.0f, (float)this.maxDurability);
        this.User.healthPoints = this.durability;
    }

    public void PrintStatus(){
        Debug.Log("Status Armor " + this.armorName);

        string s = "User: " + this.User.unitName+"\n\n"

                +  "Durability: " + this.durability.ToString()+"\n"
                +  "MaxDurability: " + this.maxDurability.ToString()+"\n"
                +  "Shield: " + this.shield.ToString()+"\n"
                +  "ShieldRegenRate: " + this.shieldRegenRate.ToString()+"\n";

        Debug.Log(s);
    }
}
