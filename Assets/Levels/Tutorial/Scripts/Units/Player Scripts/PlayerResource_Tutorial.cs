using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerResource_Tutorial : PlayerBattleResourceHandler
{
    // took out health deplete & enemy DOT
    public override void Setup(PlayerCharacter Player, TextMeshProUGUI NameText, Slider HpSlider){
        this.Player = Player;

        this.NameText = NameText;
        this.NameText.text = Player.unitName;

        this.HpSlider = HpSlider;
        
        BattleUpdateLoop();
    }

    // prevented player from dying by keeping hp >= 1
    public override void DealDamage(int damage){
        if(damage >= Player.healthPoints) return;

        Player.healthPoints = (int)Mathf.Max(0f, (float)( Player.healthPoints - damage ));
    }
}
