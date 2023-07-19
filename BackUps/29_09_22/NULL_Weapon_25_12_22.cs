using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class NULL_Weapon : Weapon
{
    public override void SetWeaponType(){
        Debug.Log("Setting Type of NULL_Weapon to none");
        this.Type = WeaponType.NONE;
    }

    public override void InitAbilityList(){
        this.AbilityList = new List<Action>();

        this.AbilityList.Add(new Element(SpellElement.none));
        this.AbilityList.Add(new Light());
    }

    public override List<Action> GetCompleteMoveList(){
        List<Action> Actions_l = new List<Action>();

        Actions_l.Add(new Element(SpellElement.none));
        Actions_l.Add(new Light());
        return Actions_l;
    }

    public override Action CombineActions(string comboList, List<Action> Actions_l){
        Debug.Log("Calling CombineActions() on NULL_Weapon!\nReturning Light Attack;");
        Action res = new Light();
        return res;
    }

    public override List<Action> GetMovesToTest(){
        List<Action> Actions_l = new List<Action>();
        return Actions_l;
    }
}
