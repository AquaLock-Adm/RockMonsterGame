using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum ItemType {heal, repair, other}

public class Item {
	public int value;
	public int useCost;
	public ItemType type;
	public override string ToString(){
		string s = this.type.ToString()+" :\n\n"
		+ "Value: " + this.value.ToString()+"\n"
		+ "UseCost: " + this.useCost.ToString()+"\n";

		return s;
	}
}

public abstract class Unit : MonoBehaviour
{

	//public Debuffs Debuffs;
    [Header("System")]
    public float attackIntoShieldMultiplier = 0.8f;
    public int depletionTime = 500; // in ms
    public int pointsPerTick = 1;
    public int tickRate = 1; //ms delay between points
    public int textShowTime = 1000; // in ms

	public string unitName;
	[Header("Universal Stats")]
	public SpellElement element = SpellElement.none;

	public int level = 1;

	public int healthPoints = 0;
	public int maxHealthPoints = 0;

	public int shield = 0;
	public int maxShield = 0;

	public int attackMin = 0;
	public int attackMax = 0;
	
	public int accuracy = 0;
	public int crit = 0;

	public bool damageTesting = false;

    public abstract void Death();

    public abstract Task<int> DealDamage(int dmg);

    public async Task<int> ExecuteUnit(){
    	Debug.Log("Executed "+this.unitName);
    	int damageDealt = this.healthPoints;
    	if(this.gameObject.activeSelf){
			List<Task> tasksTmp = new List<Task>();
			tasksTmp.Add(ShowDamageText(this.shield+this.healthPoints));
			if(shield > 0){
				tasksTmp.Add(DepleteShield(this.shield));
				await Task.WhenAll(tasksTmp);
			}
	        tasksTmp.Add(DepleteHp(this.healthPoints+1));
	        await Task.WhenAll(tasksTmp);
	    }else {
	    	this.shield = 0;
	    	this.healthPoints = 0;
	    }
	    this.Death();
	    return damageDealt;
    }

    abstract public void MissedAttack();

    abstract public void Crit();

    abstract public Task ShowDamageText(int n);

    /*private async Task IncreaseHp(int n){ 
    	Debug.Log("Healing has been moved to repairing your armor!");
    	await Task.Yield();
    }*/

    public async Task DepleteShield(int amount){
    	if(amount <= 0) return;

    	int cDepletion = 0;

    	CalculateTickRate(amount);
        int timeWaited = 0;

        while(cDepletion < amount && this.shield > 0 && this.healthPoints > 0){
        	await Task.Delay(this.tickRate);
        	timeWaited += this.tickRate;

        	this.shield -= this.pointsPerTick;
        	cDepletion += this.pointsPerTick;
        }

        if(this.healthPoints > 0){
            if(cDepletion > amount) this.shield += cDepletion - amount;
            await Task.Delay((int)Mathf.Clamp(this.depletionTime - timeWaited, 0.0f, this.depletionTime));
        }
    }

    public async Task DepleteHp(int amount){
    	if(amount <= 0) return;

    	int cDepletion = 0;

    	CalculateTickRate(amount);
        int timeWaited = 0;

        int startHp = this.healthPoints;

        while(cDepletion < amount && this.healthPoints > 0){
        	await Task.Delay(this.tickRate);
        	timeWaited += this.tickRate;

        	this.healthPoints -= this.pointsPerTick;
        	cDepletion += this.pointsPerTick;
        }

        if(startHp - amount > 0){
            if(cDepletion > amount) this.healthPoints += cDepletion - amount;
            await Task.Delay((int)Mathf.Clamp(this.depletionTime - timeWaited, 0.0f, this.depletionTime));
        }
    }

    public void CalculateTickRate(int amount){
    	if(this.depletionTime <= 0){
    		this.pointsPerTick = 0;
    		this.tickRate = 0;
    		return;
    	}

    	ResetTickRate();

        while(amount < (int)Mathf.Round((float)this.depletionTime / (float)this.tickRate)){
            this.tickRate++;
        }

        this.pointsPerTick = (int)Mathf.Round((float)amount/((float)this.depletionTime/(float)this.tickRate));

        // Debug.Log(amount.ToString()+"n in "+this.depletionTime.ToString()+"ms: "
        // 	+this.tickRate.ToString()+"ms Tick Rate / "+this.pointsPerTick.ToString()+"ppT, Rest: "+
        // 	(amount - (this.depletionTime/this.tickRate)*this.pointsPerTick).ToString()+"ms.");
    }

    public void ResetTickRate(){
    	this.tickRate = 1;
    	this.pointsPerTick = 1;
    }
}