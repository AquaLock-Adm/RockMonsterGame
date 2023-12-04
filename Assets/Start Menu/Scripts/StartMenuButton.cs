using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartMenuButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI OptionText;

    void OnMouseOver(){
        Debug.Log("Mouse in");
    }

    void OnMouseExit(){
        Debug.Log("Mouse out");
    }

    public void SetOptionText(string text){
        OptionText.text = text;
    }

    public void HoverMenuButton(){
        transform.Find("Option Rim").gameObject.SetActive(true);
    }

    public void UnHoverMenuButton(){
        transform.Find("Option Rim").gameObject.SetActive(false);
    }
}
