using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExtraButton : MonoBehaviour
{
	public Text MainText;
	public Text SelectText;
	public Unit Target;

#region Unity Functions

	private void Awake(){
		foreach (Transform child in transform){
			if(child.name == "Select Text") {
				SelectText = child.gameObject.GetComponent<Text>();
				break;
			}
		}

		SelectText.gameObject.SetActive(false);
	}

#endregion
}