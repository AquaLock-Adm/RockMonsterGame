using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DialogueText {
    private string speaker;
    private string mainText;
    private bool leftSideSpeaker;

    public DialogueText(string speaker, string mainText, bool leftSideSpeaker){
        this.speaker = speaker;
        this.mainText = mainText;
        this.leftSideSpeaker = leftSideSpeaker;
    }

    public string GetSpeaker() { return this.speaker; }
    public string GetMainText() { return this.mainText; }
    public bool GetLeftSideSpeaker() { return this.leftSideSpeaker; }
}

public class DialogueHandler : MonoBehaviour
{
    public BattleSystem BattleSystem;
    public ActionQueue ActionQueue;

    public GameObject DialogueScreen;

    public Text DialogueMainText;
    public Text SpeakerTextL;
    public Text SpeakerTextR;

    public Queue<DialogueText> DialogueQueue = new Queue<DialogueText>();

    public float dialogueCrawlSpeed = 0.2f;

    public string dialogueScriptName;
    public DialogueScript DialogueScript;

    private bool lastTimerState;
    private BattleState lastBattleState;
    // Start is called before the first frame update
    void Start() {
       DialogueScreen.SetActive(false); 
       ResetTextFields();
    }

    public void ActivateDialogueScript(){

        switch(dialogueScriptName){
            case "Tutorial":
                Tutorial Tutorial = new Tutorial(this, ActionQueue, BattleSystem);
                this.DialogueScript = Tutorial;
                Tutorial.Start();
            break;
            default:
                Debug.Log("Dialogue Script Name needs to be added to DialogueHandler.ActivateDialogueScript()!");
            break;
        }
    }

    public void EnableDialogueText(){
        PauseEverything(true);
        ShowNextDialogueText();
    }

    public void ShowNextDialogueText(){
        if(DialogueQueue.Count > 0){
            BattleSystem.stopTextCrawl = true;
            ResetTextFields();
            DialogueText currentText = DialogueQueue.Dequeue();
            ShowText(currentText);
        } else PauseEverything(false);
    }

    private void ResetTextFields(){
        DialogueMainText.text = "";
        SpeakerTextL.text = "";
        SpeakerTextR.text = "";
    }

    private void PauseEverything(bool on){
        
        if(on) {
            lastBattleState = BattleSystem.state;
            BattleSystem.state = BattleState.DIALOGUE;
        }else BattleSystem.state = lastBattleState;

        ActionQueue.pauseQueue = on;

        DialogueScreen.SetActive(on);
    }

    private void ShowText(DialogueText T){
        if(T.GetLeftSideSpeaker()){
            DialogueMainText.alignment = TextAnchor.MiddleLeft;
            SpeakerTextL.text = T.GetSpeaker();
        }
        else {
            DialogueMainText.alignment = TextAnchor.MiddleRight;
            SpeakerTextR.text = T.GetSpeaker();
        }

        StartCoroutine(BattleSystem.CrawlText(T.GetMainText(), DialogueMainText, dialogueCrawlSpeed));       
    }
}


public abstract class DialogueScript {
    public DialogueHandler DialogueHandler;
    public ActionQueue ActionQueue;
    public BattleSystem BattleSystem;
    public Queue<DialogueText> DialogueTexts;
    public bool running;
    public int scriptIndex;
    public abstract void Start();
    public abstract void ExecuteShowNextText();
}

public class Tutorial : DialogueScript {
    public Tutorial(DialogueHandler DH, ActionQueue AQ, BattleSystem BS){
        if(DH == null) {
            Debug.Log("Error! Dialogue Handler is null");
            return;
        }

        if(AQ == null) {
            Debug.Log("Error! ActionQueue is null");
            return;
        }

        if(BS == null) {
            Debug.Log("Error! Battle System is null");
            return;
        }

        this.DialogueHandler = DH;
        this.ActionQueue = AQ;
        this.BattleSystem = BS;
        this.DialogueTexts = new Queue<DialogueText>();
        this.scriptIndex = 0;

        InitiateScriptTexts();
    }
    
    public override async void Start(){
        running = true;
        while(this.running){
            switch(this.scriptIndex){
                case 0:
                    if(this.BattleSystem.enemyDefeatCount == 1 && this.BattleSystem.state == BattleState.PLAYERTURN) {
                        ExecuteShowNextText();
                        this.scriptIndex++;
                        InitiateScriptTexts();
                    }
                break;
                case 1:
                    if(this.BattleSystem.enemyDefeatCount == 2 && this.BattleSystem.state == BattleState.PLAYERTURN) {
                        ExecuteShowNextText();
                        this.scriptIndex++;
                        InitiateScriptTexts();
                    }
                break;
                default:
                    Debug.Log("Script Index reached end of Switch statement. Terminating Dialogue Script...");
                    this.running = false;
                break;
            }
            await Task.Delay(50);
        }
    }

    public override void ExecuteShowNextText(){
        while(this.DialogueTexts.Count > 0){
            this.DialogueHandler.DialogueQueue.Enqueue(this.DialogueTexts.Dequeue());
        }

        this.DialogueHandler.EnableDialogueText();
    }

    private void InitiateScriptTexts(){
        string mainText = "";
        DialogueText T;
        switch(this.scriptIndex){
            case 0:
                mainText = "Okay, this should work now!";
                T = new DialogueText(this.BattleSystem.Player.unitName, mainText, true);
                this.DialogueTexts.Enqueue(T);

                mainText = "Are you sure about that?";
                T = new DialogueText(this.BattleSystem.NextEnemy.unitName, mainText, false);
                this.DialogueTexts.Enqueue(T);
            break;

            case 1:
                mainText = "Was anderes!";
                T = new DialogueText(this.BattleSystem.Player.unitName, mainText, true);
                this.DialogueTexts.Enqueue(T);

                mainText = "Wie jetzt?";
                T = new DialogueText(this.BattleSystem.NextEnemy.unitName, mainText, false);
                this.DialogueTexts.Enqueue(T);
            break;
        }
    }
}