using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AbilityOptionSelectBar : MonoBehaviour
{
    [Header("Select Bar Base")]
    [SerializeField] private GameObject SelectTextPrefab;

    [SerializeField] private float selectTextHeight;
    [SerializeField] private float selectFillerHeight;

    private List<TextMeshProUGUI> SelectTextList = new List<TextMeshProUGUI>();
    private TextMeshProUGUI CurrentHoveredText;

    public void AbilityLoadoutSetup(float selectBoxHeight, float fillerBoxHeight){
        this.selectTextHeight = selectBoxHeight;
        this.selectFillerHeight = fillerBoxHeight;
    }

    public void AddSelectText(){
        GameObject ST_GO = Instantiate(SelectTextPrefab, this.transform);
        ST_GO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.selectTextHeight);
        TextMeshProUGUI NewText = ST_GO.GetComponent<TextMeshProUGUI>();
        NewText.text = "";
        this.SelectTextList.Add(NewText);
        if(this.SelectTextList.Count <= 1){
            this.CurrentHoveredText = NewText;
            HoverSelectText(0);
        }
    }

    public void AddFillerText(){
        GameObject ST_GO = Instantiate(SelectTextPrefab, this.transform);
        ST_GO.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.selectFillerHeight);
        ST_GO.GetComponent<TextMeshProUGUI>().text = "";
        ST_GO.name = "Filler";
    }

    public void HoverSelectText(int textIndex){
        if(textIndex < 0 || textIndex >= this.SelectTextList.Count){
            Debug.LogError("Index Error!: "+textIndex.ToString());
            return;
        }

        this.CurrentHoveredText.text = "";
        this.SelectTextList[textIndex].text = ">";
        this.CurrentHoveredText = this.SelectTextList[textIndex];
    }
}
