using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuButton : MonoBehaviour
{
    void OnMouseOver(){
        Debug.Log("Mouse in");
    }

    void OnMouseExit(){
        Debug.Log("Mouse out");
    }

    public void HoverMenuButton(){
        transform.Find("Option Rim").gameObject.SetActive(true);
    }

    public void UnHoverMenuButton(){
        transform.Find("Option Rim").gameObject.SetActive(false);
    }
}
