using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour //Handles all text functionality
{
	//Scripts
	public static GameManager gameManager;
	public static EventManager eventManager;
	public static DialogueManager dialogueManager;
	public static TextCommands textCommands;
    public static DialogueBoxScript dialogueBoxScript;

	//Text Elements
	public GameObject nameTextObject;
	public GameObject dialogueTextObject;
	public GameObject historyTextObject;
	public TextMeshPro dialogueText;
	public TextMeshProUGUI historyText;
	public RectTransform historyTextContainer;
	private List<string> sceneScript = new List<string>();

	//Values
	[HideInInspector]
	public float waitTime = 0.04f;
	public string finalText = "";
	public string currentText = "";
	public int lineIndex = 0;
	private int textIndex = 0;
	[HideInInspector]
	public bool delaySignal = false;
	[HideInInspector]
	public float delayValue;

	public AudioSource musicSource;
	public AudioSource voiceSource;

	void Awake(){ //instantiate dialoguemanager script and maintain between scenes
		if (!dialogueManager){
			dialogueManager = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
	}

	void Start() //Load appropriate assets for the scene
	{	
		gameManager = GetComponent<GameManager>();
		eventManager = GetComponent<EventManager>();
		textCommands = GetComponent<TextCommands>();
        dialogueBoxScript = GameObject.Find("DialogueBox").GetComponent<DialogueBoxScript>();

		nameTextObject = GameObject.FindWithTag("NameText");
		dialogueTextObject = GameObject.FindWithTag("DialogueText");
		historyTextObject = GameObject.FindWithTag("HistoryText");
		dialogueText = dialogueTextObject.GetComponent<TextMeshPro>();
		historyText = historyTextObject.GetComponent<TextMeshProUGUI>();
		historyTextContainer = historyTextObject.GetComponent<RectTransform>();

		historyTextObject.SetActive(false);

		dialogueText.text = "";
		historyText.text = "";

		LoadScript("Assets/Script/testScript.txt");
	}

	void LoadScript (string path){ //loads text script into a queue
		try {
			StreamReader sr = new StreamReader(path);
			string line;
			while ((line = sr.ReadLine()) != null){
				line = line.Replace("\\n", "\n");
				sceneScript.Add(line);
			}
			Debug.Log("List Loaded. Count: "+sceneScript.Count+", Capacity: "+sceneScript.Capacity);
		} catch (FileNotFoundException e){
			Debug.Log("<color=red> File failed to read: </color>" + e.Message);
		}
	}

	#region TextManagement
	public void LoadNextLine(int line = -1){ //loads next line into finalText and increments the lineIndex, can jump to a given line. Resets textIndex.
		if (line == -1){
			Debug.Log("Reading next line #"+lineIndex);
			finalText = sceneScript[lineIndex];
			lineIndex++;
		} else {
			Debug.Log("Jumping to line #"+line);
			finalText = sceneScript[line];
			lineIndex = line + 1;
		}
		currentText += "<color=\"black\">";
		textIndex = 0;
	}
	IEnumerator LetterByLetter(){ //Plays a line of dialogue
		gameManager.playingDialogue = true;
        dialogueBoxScript.SetTalking(true);
		while (textIndex < finalText.Length){
			CheckNextChar();
			dialogueText.text = currentText;
			PlayVoiceClip();
			textIndex++;
			if (delaySignal == true) {
				delaySignal = false;
				IEnumerator delay = eventManager.Delay(delayValue);
				yield return StartCoroutine(delay);
			} else {
				yield return new WaitForSeconds(waitTime);
			}
		}
		ResetTextVariables();
		yield return null;
	}
	public void DisplayAllText() { //Immediately display all lines in dialogue
		gameManager.playingDialogue = false;
		StopCoroutine("LetterByLetter");
		PlayVoiceClip();
		while (textIndex < finalText.Length){
			CheckNextChar();
			dialogueText.text = currentText;
			textIndex++;
		}
		ResetTextVariables();
	}
	void CheckNextChar(){ //Checks for special characters to run commands. If none detected, displays next character in line.
		if (finalText[textIndex] == '[') {
			textCommands.CheckBracket(textIndex, false);
		} else if (finalText[textIndex] == '<'){ //Checking for markup tags and adding them immediately instead of character by character
			while (finalText[textIndex] != '>'){
				currentText += finalText[textIndex];
				textIndex++;
			}
			currentText += finalText[textIndex];
		} else {
			currentText += finalText[textIndex];
		}
	}

	public int GetTextIndex(){
		return textIndex;
	}
	public void SetTextIndex(int n){
		if (n < finalText.Length){
			textIndex = n;
		} else {
			Debug.Log("<color=red>SetTextIndex() is attempting to set textIndex(</color>" + textIndex+" > "+n+") past finalText.Length("+finalText.Length+")");
		}
	}
	public void IncrementTextIndex(int i = 1){
		textIndex += i;
	}
	void ResetTextVariables(){
		currentText = "";
		finalText = "";
		textIndex = 0;
		gameManager.playingDialogue = false;

        dialogueBoxScript.SetTalking(false);
    }
	#endregion

	#region History
	public void AddToHistory(string line){ //Converts given line to a history-ready line and adds it to the history text
		string convertedLine = line;
		convertedLine += "\n\n";

		int count = 0;
		int removalSize;
		int newLineCount = 2;
		while (count < convertedLine.Length){
			while(convertedLine[count] == '['){
				removalSize = 1;
				while (convertedLine[count+removalSize] != ']'){
					removalSize++;
				}
				convertedLine = convertedLine.Remove(count, removalSize+1);
			}
			if (count < convertedLine.Length - 1){
				if(convertedLine.Substring(count, 2) == "\n"){
					newLineCount++;
				}
			}
			count++;
		}

		historyText.text += "<color=\"black\">";
		historyText.text += convertedLine;
		historyText.text += "</color>";
		historyTextContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, historyTextContainer.sizeDelta.y+(float)newLineCount*30);
	}
	public void HistoryBox(){ //Opens and closes the history
		if (gameManager.playingDialogue == false && historyTextObject.activeInHierarchy == false){
			dialogueTextObject.SetActive(false);
			historyTextObject.SetActive(true);
		} else if (gameManager.playingDialogue == false && historyTextObject.activeInHierarchy == true){
			historyTextObject.SetActive(false);
			dialogueTextObject.SetActive(true);
		}
	}
	#endregion

	void PlayVoiceClip() { //play a voice clip if the current character is not a space
		if (voiceSource.isPlaying == true) {
			voiceSource.Stop();
		}
		if (finalText[textIndex] != ' ' || finalText[textIndex] != '[' || finalText[textIndex] != '<') {
				voiceSource.Play();
			}
	}

    public void ChangeUnderlay (Color c) {
        dialogueText.fontSharedMaterial.SetColor("_UnderlayColor", c);
    }

}

