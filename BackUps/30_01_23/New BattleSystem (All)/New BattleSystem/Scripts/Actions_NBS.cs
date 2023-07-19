using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Light_NBS : Action
{
    public Light_NBS(){
        this.name = "Light Attack";
        this.cover = "Light";
        this.comboLevel = 1;
        this.comboList = "L";
        this.totalTime = 300; //ms
        this.knockBack = 20;  //px
        this.accuracy = 99;
        this.Setup();
        this.StopPermanentEffect();
    }

    public override void Setup(){
        // this.SetGameHandler();
        this.BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        this.Player = this.BattleSystem.Player;
        this.BaseDamageCalculation();

        this.manaCost = 0;
        this.crit = this.Player.crit;

        this.SetEffects();
    }

    public override void SetDamageToWeapon(){

        this.damageToWeapon = this.Enemy.damageToWeapons;
    }

    public override void SetEffects(){
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new Light_NBS();
        A.abilityIndex = this.abilityIndex;
        return A;
    } 
}

public class Heavy_NBS : Action
{
    public Heavy_NBS(){
        this.name = "Heavy Attack";
        this.cover = "Heavy";
        this.comboLevel = 1;
        this.comboList = "H";
        this.totalTime = 600; //ms
        this.knockBack = 100;  //px
        this.accuracy = 95;

        this.Setup();
        this.StopPermanentEffect();
    }

    public override void Setup(){
        // this.SetGameHandler();
        this.BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        this.Player = this.BattleSystem.Player;
        this.BaseDamageCalculation();
        // this.BaseApCalculation();

        this.manaCost = 0;
        this.crit = this.Player.crit;
        this.SetAccuracy();

        this.SetEffects();
    }

    public override void SetDamageToWeapon(){
        this.damageToWeapon = this.Enemy.damageToWeapons;
    }

    public override void SetEffects(){
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new Heavy_NBS();
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

public class Special_NBS : Action
{
    public Special_NBS(){
        this.name = "Special Attack";
        this.cover = "Special";
        this.comboLevel = 1;
        this.comboList = "S";
        this.totalTime = 400; //ms
        this.knockBack = 800;  //px
        this.accuracy = 99;

        this.Setup();
        this.StopPermanentEffect();
    }

    public override void Setup(){
        // this.SetGameHandler();
        this.BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        this.Player = this.BattleSystem.Player;
        this.BaseDamageCalculation();
        // this.BaseApCalculation();

        this.manaCost = 0;
        this.crit = this.Player.crit;
        this.SetAccuracy();

        this.SetEffects();
    }

    public override void SetDamageToWeapon(){
        this.damageToWeapon = this.Enemy.damageToWeapons;
    }

    public override void SetEffects(){
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new Special_NBS();
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

public class Spike_NBS : Action
{
    public Spike_NBS(){
        this.name = "Spike";
        this.cover = "Spike";
        this.comboLevel = 3;
        this.comboList = "LHS";
        this.totalTime = 450; //ms

        this.Setup();
        this.StopPermanentEffect();
    }

    public override void Setup(){
        // this.SetGameHandler();
        this.BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        this.Player = this.BattleSystem.Player;
        this.BaseDamageCalculation();
        // this.BaseApCalculation();

        this.manaCost = 0;
        this.crit = this.Player.crit;
        this.SetAccuracy();

        this.SetEffects();
    }

    public override void SetDamageToWeapon(){
        this.damageToWeapon = this.Enemy.damageToWeapons;
    }

    public override void SetEffects(){
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new Spike_NBS();
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}

public class Tride_NBS : Action
{
    public Tride_NBS(){
        this.name = "Tride";
        this.cover = "Tride";
        this.comboLevel = 2;
        this.comboList = "HS";
        this.totalTime = 400; //ms

        this.Setup();
        this.StopPermanentEffect();
    }

    public override void Setup(){
        // this.SetGameHandler();
        this.BattleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        this.Player = this.BattleSystem.Player;
        this.BaseDamageCalculation();
        // this.BaseApCalculation();

        this.manaCost = 0;
        this.crit = this.Player.crit;
        this.SetAccuracy();

        this.SetEffects();
    }

    public override void SetDamageToWeapon(){
        this.damageToWeapon = this.Enemy.damageToWeapons;
    }

    public override void SetEffects(){
        return;
    }

    public override async Task Passive(){
        await Task.Yield();
    }

    public override Action Copy(){
        Action A =  new Tride_NBS();
        A.abilityIndex = this.abilityIndex;
        return A;
    }
}