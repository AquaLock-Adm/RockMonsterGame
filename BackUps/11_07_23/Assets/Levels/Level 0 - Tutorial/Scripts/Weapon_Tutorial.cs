using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon_Tutorial : Weapon
{
    public override void SetWeaponType(){
        
        this.Type = WeaponType.SWORD;
    }

    public override int[,] GetUpgradeTable(){
        int[,] table = new int[,]
        {
            // {baseMinAtk, baseMaxAtk, APR, Dur, UpgradeCost}
            {  1,   2, 2, 7500,  0}
        };

        return table;
    }

    public override void InitAbilities(){
        this.Abilities = new List<Action>();

        this.Abilities.Add(new Light(this.Player));
        this.Abilities.Add(new Heavy(this.Player));
        this.Abilities.Add(new Special(this.Player));

        int abilityIndex_c = 0;

        foreach(Action A in this.Abilities){
            A.abilityIndex = abilityIndex_c;
            abilityIndex_c++;
        }
    }

    public override List<Action> GetCompleteMoveList(){
        List<Action> Actions_l = new List<Action>();

        Actions_l.Add(new Light(this.Player));
        Actions_l.Add(new Heavy(this.Player));
        Actions_l.Add(new Special(this.Player));

        return Actions_l;
    }

    public override Action CombineActions(string comboList, List<Action> Actions_l){
        Action res;
        switch(comboList){

            default:
                Debug.Log("Combo Attack not found!");
                res = null;
            break;
        }
        return res;
    }

    public override List<Action> GetMovesToTest(){
        List<Action> Actions_l = new List<Action>();

        //Actions_l.Add(new ShieldBreak());
        //Actions_l.Add(new ElementalInfuse(new Element(SpellElement.FIRE)));
        //Actions_l.Add(new JumpAttack());
        //Actions_l.Add(new Execute());
        //Actions_l.Add(new SpinAttack());
        //Actions_l.Add(new Lunge(new Element(SpellElement.FIRE)));
        //Actions_l.Add(new MagicBoom(new Element(SpellElement.FIRE), new Element(SpellElement.FIRE)));
        //Actions_l.Add(new WyvernSlayer(new Element(SpellElement.FIRE), new Element(SpellElement.FIRE), new Element(SpellElement.FIRE)));
        //Actions_l.Add(new PaladinsBane());
        //Actions_l.Add(new MariasWrath(new Element(SpellElement.FIRE), new Element(SpellElement.FIRE)));

        return Actions_l;
    }

    public override Action GetAttackRushFinisher(int finisherLevel){
        return new Light(this.Player);
    }
}
