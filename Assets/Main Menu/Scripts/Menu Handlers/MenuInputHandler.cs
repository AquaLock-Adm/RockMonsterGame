using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInputHandler : MonoBehaviour
{
    [SerializeField] public MenuHandler MenuHandler;
    [SerializeField] public List<GameObject> ButtonRims;
    [SerializeField] private int currentButtonIndex = 0;
    [SerializeField] public bool disableInput = false;

    void Awake(){
        UnhoverAll();
        ShowRim(this.currentButtonIndex);
    }

    void Update(){
        if(!this.disableInput) CheckInput();
    }

    private void UnhoverAll(){
        foreach(GameObject G in this.ButtonRims){
            G.SetActive(false);
        }
    }

    private void ShowRim(int buttonIndex){
        UnhoverAll();
        this.ButtonRims[buttonIndex].SetActive(true);
    }

    private void CheckInput(){
        if(Input.GetKeyDown(KeyCode.S) && this.currentButtonIndex+1 < this.ButtonRims.Count) {
            this.currentButtonIndex++;
            ShowRim(this.currentButtonIndex);
            this.MenuHandler.HoverOption(this.currentButtonIndex);
        }else if(Input.GetKeyDown(KeyCode.W) && this.currentButtonIndex > 0){
            this.currentButtonIndex--;
            ShowRim(this.currentButtonIndex);
            this.MenuHandler.HoverOption(this.currentButtonIndex);
        }else if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.A)){
            this.MenuHandler.ClickOption(currentButtonIndex);
        }
    }
}
