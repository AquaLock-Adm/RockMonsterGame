using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerResource_Tutorial : PlayerBattleResourceHandler
{
    public override void Setup(PlayerCharacter Player, TextMeshProUGUI NameText, Slider HpSlider){
        this.Player = Player;

        this.NameText = NameText;
        this.NameText.text = Player.unitName;

        this.HpSlider = HpSlider;
        
        BattleUpdateLoop();
    }
}
