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
        this.lockedWidth = this.FillRect.rect.width;
        this.unlockedWidth = gameObject.GetComponent<RectTransform>().rect.width - 10f;
        // this.FillRect.sizeDelta = new Vector2(this.lockedWidth, FillRect.rect.height);
        // this.FillRect.sizeDelta = new Vector2(0f, 0f);
        this.Lock_GO.SetActive(true);
    }

    public void ShowLockedStatus(bool on){
        Debug.Log("Todo!");
    }
}