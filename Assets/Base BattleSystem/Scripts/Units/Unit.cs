using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    [Header("System")]
    [SerializeField] protected int depletionTime = 500; // in ms
    [SerializeField] protected int pointsPerTick = 1;
    [SerializeField] protected int tickRate = 1; //ms delay between points
    [SerializeField] protected int textShowTime = 1000; // in ms

	[Header("Universal Stats")]
	[SerializeField] public string unitName;

	[SerializeField] public int healthPoints = 0;
	[SerializeField] public int maxHealthPoints = 0;

    public int battleSpeed = 1;
    public int baseBattleSpeed = 5;


    public abstract Task Death();

    protected void CalculateTickRate(int totalAmount){
    	if(this.depletionTime <= 0){
    		this.pointsPerTick = 0;
    		this.tickRate = 0;
    		return;
    	}

    	ResetTickRate();

        while(totalAmount < (int)Mathf.Round((float)this.depletionTime / (float)this.tickRate)){
            this.tickRate++;
        }

        this.pointsPerTick = (int)Mathf.Round((float)totalAmount/((float)this.depletionTime/(float)this.tickRate));

        // Debug.Log(totalAmount.ToString()+"n in "+this.depletionTime.ToString()+"ms: "
        // 	+this.tickRate.ToString()+"ms Tick Rate / "+this.pointsPerTick.ToString()+"ppT, Rest: "+
        // 	(totalAmount - (this.depletionTime/this.tickRate)*this.pointsPerTick).ToString()+"ms.");
    }

    protected void ResetTickRate(){
    	this.tickRate = 1;
    	this.pointsPerTick = 1;
    }
}