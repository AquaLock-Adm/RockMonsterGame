using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityOptionButton : StartMenuButton
{
    [Header("Ability Option")]
    [SerializeField] private GameObject OptionRim_GO;
    [SerializeField] private GameObject Lock_GO;
    [SerializeField] private GameObject Fill_GO;
    private RectTransform FillRect;

    public Action AssignedAction;

    [SerializeField] private float lockedWidth;
    [SerializeField] private float unlockedWidth;

    public void AbilityLoadoutSetup(Action A){
        this.FillRect = this.Fill_GO.GetComponent<RectTransform>();
        this.unlockedWidth = gameObject.GetComponent<RectTransform>().rect.width - 10f;
        this.lockedWidth = this.unlockedWidth - this.Lock_GO.GetComponent<RectTransform>().rect.width-5;
        
        this.AssignedAction = A;
        this.SetOptionText(A.name);
        
        ShowLockedStatus(true);
    }

    public void ShowLockedStatus(bool on){
        this.Lock_GO.SetActive(on);
        if(on){
            this.FillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.lockedWidth);
        }else{
            this.FillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.unlockedWidth);
        }
    }

    public void ShowSetStatus(bool on){
        if(on){
            this.OptionRim_GO.GetComponent<Image>().color = new Color32(42, 205, 42, 255); // Green
        }else {
            this.OptionRim_GO.GetComponent<Image>().color = new Color32(0, 0, 0, 255); // Black
        }
    }

    public void OverloadSetStatus(){
        this.OptionRim_GO.GetComponent<Image>().color = new Color32(205, 42, 42, 255); // red
    }
}