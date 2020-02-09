using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour //Manages general game logic, communication between lower level scripts/systems
{
    //Scripts
    public static GameManager gameManager;
	public static EventManager eventManager;
    public static DialogueManager dialogueManager;
	public static TextCommands textCommands;
	public static Data data;

	//Values
	[HideInInspector]
	public bool playerInControl = true;
    [HideInInspector]
	public bool playerInChoice = false;
	[HideInInspector]
	public bool playingDialogue = false;
	public string speaker;

    public Queue eventQueue = new Queue();
	public List<System.Object[]> eventQueueParamList = new List<System.Object[]>();

    void Awake(){ //instantiate dialoguemanager script and maintain between scenes
		if (!gameManager){
			gameManager = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);
	}
    
    void Start()
    {
		eventManager = GetComponent<EventManager>();
        dialogueManager = GetComponent<DialogueManager>();
		textCommands = GetComponent<TextCommands>();
		data = GetComponent<Data>();
        
        speaker = data.speakerList[0];
    }

    // Update is called once per frame
    void Update()
    {
		if (playerInControl == true){
			if (Input.GetKeyDown(KeyCode.Space) /*|| Input.GetKeyDown(KeyCode.Mouse0)*/){
				if (playingDialogue == false){
					dialogueManager.LoadNextLine();
					dialogueManager.AddToHistory(dialogueManager.finalText);

					textCommands.CheckBracket(dialogueManager.GetTextIndex(), true); //Parsing Step #1
					StartCoroutine("ParseQueue");
					dialogueManager.StartCoroutine("LetterByLetter"); //Parsing Step #3
				} else { //Same functionality as left control/right click while dialogue is playing.
					dialogueManager.DisplayAllText();
			    }
			}
			if (/*Input.GetKeyDown(KeyCode.LeftControl) || */Input.GetKeyDown(KeyCode.Mouse1)){ //Skips text playback.
				if (dialogueManager.finalText == ""){
					dialogueManager.LoadNextLine();
					dialogueManager.AddToHistory(dialogueManager.finalText);

					textCommands.CheckBracket(dialogueManager.GetTextIndex(), true); //Parsing Step #1
					StartCoroutine("ParseQueue"); //Parsing Step #2
				}
				dialogueManager.DisplayAllText(); //Parsing Step #3
			}

			if (Input.GetKeyDown(KeyCode.C)){ // Open History
				dialogueManager.HistoryBox();
			}
		}
    }

    IEnumerator ParseQueue(){ //Runs through start-of-line events
		Debug.Log("Parsing queue...");
        playerInControl = false;
		int count = 0;
		Type type = typeof(EventManager);
		MethodInfo mi;
        while (eventQueue.Count != 0){
			Debug.Log("Running event: "+eventQueue.Peek());
			if ((string)eventQueue.Peek() == "Delay") {
				IEnumerator delay = eventManager.Delay((float)eventQueueParamList[count][0]);
				yield return StartCoroutine(delay);
			} else {
				mi = type.GetMethod(eventQueue.Peek().ToString());
				mi.Invoke(eventManager, eventQueueParamList[count]);
			}
			eventQueue.Dequeue();
			count++;
        }
		eventQueueParamList.Clear();
        playerInControl = true;
		yield return null;
    }

	void SetPlayerControl (bool control) {
		playerInControl = control;
	}
	bool getPlayerControl () {
		return playerInControl;
	}
}
