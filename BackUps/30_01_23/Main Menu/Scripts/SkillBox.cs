using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillBox : MonoBehaviour
{
    public Action Skill;
    [SerializeField] public TextMeshProUGUI NameText;
    [SerializeField] public TextMeshProUGUI LevelText;
    [SerializeField] public Image Background;
    [SerializeField] public Color32 BaseColor;
    [SerializeField] public Color32 InactiveColor;

    public void Setup(Action A){
        this.Skill = A;
        this.NameText.text = A.name;
        this.LevelText.text = A.comboLevel.ToString();
    }

    public void LoadSkillIntoInfoText(){
        MainMenuHandler MMH = GameObject.Find("Main Menu Options").GetComponent<MainMenuHandler>();
        MMH.SetSkillInfoText(this.Skill);
    }

    public void ChangeActiveState(bool on){
        if(on){
            this.Background.color = this.BaseColor;
        }else{
            this.Background.color = this.InactiveColor;
        }
    }
}
