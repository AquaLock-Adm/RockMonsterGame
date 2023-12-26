using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityOptionButton : StartMenuButton
{
    [Header("Ability Option")]
    [SerializeField] private GameObject Lock_GO;
    [SerializeField] private GameObject Fill_GO;
    private RectTransform FillRect;

    [SerializeField] private float lockedWidth;
    [SerializeField] private float unlockedWidth;

    public void AbilityLoadoutSetup(){
        this.FillRect = this.Fill_GO.GetComponent<RectTransform>();
        this.unlockedWidth = gameObject.GetComponent<RectTransform>().rect.width - 10f;
        this.lockedWidth = this.unlockedWidth - this.Lock_GO.GetComponent<RectTransform>().rect.width-5;
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
}