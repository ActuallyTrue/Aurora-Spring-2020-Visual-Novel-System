using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class TextCommands : MonoBehaviour //Processes commands and passes them back to either the EventManager or DialogueManager
{
	public static GameManager gameManager;
	public static DialogueManager dialogueManager;
	public static EventManager eventManager;
	public AudioClip[] voiceClips = new AudioClip[4];

	void Start()
	{
		gameManager = GetComponent<GameManager>();
		dialogueManager = GetComponent<DialogueManager>();
		eventManager = GetComponent<EventManager>();
	}

	public void CheckBracket(int givenIndex, bool isStartOfLine){ //checks bracket command and performs appropriate measures. repeats if another command immediately follows
		if (dialogueManager.finalText[givenIndex] == '[') { //[Test][Test]test line
			string com = "";
			dialogueManager.IncrementTextIndex();
			while (dialogueManager.finalText[dialogueManager.GetTextIndex()] != ']')
			{
				com += dialogueManager.finalText[dialogueManager.GetTextIndex()];
				dialogueManager.IncrementTextIndex();
			}
			if (dialogueManager.GetTextIndex() < dialogueManager.finalText.Length) {
				dialogueManager.IncrementTextIndex(); //textIndex should be located on the space after the command.
			}
			if (com.Length >= 4 && com.Substring(0, 4) == "name") {
				com = com.Substring(5, com.Length-5);
				ProcessName(com);
			} else if (com.Length >= 6 && com.Substring(0, 6) == "choice") {
				ProcessChoice(com);
			} else {
				if (isStartOfLine) {
					ProcessEvent(com, true);
				} else {
					ProcessEvent(com, false);
					dialogueManager.IncrementTextIndex(-1);
				}
			}
			if (dialogueManager.finalText[dialogueManager.GetTextIndex()] == '[') { //if another command follows immediately after, check it.
				CheckBracket(dialogueManager.GetTextIndex(), isStartOfLine);
			}
		} else {
			Debug.Log("TextCommands.CheckBracket(): No bracket found.");
		}
	}

	public void ProcessName(string name){ //if name is different from current speaker, queue a speaker change, else queue dialogue box next anim
		//gameManager.speaker = data.speakerList.Find(x => x.Contains(name));
	}
	public void ProcessChoice(string choiceString){
		//parse the entire string for values and pass values to ChoiceManager.cs
		Debug.Log("TextCommands.ProcessChoice() called, still needs to be written.");
	}
	public void ProcessEvent(string s, bool isStartOfLine){
		string com = "";
		string paramType, paramValue;
		System.Object[] invokerParams = new System.Object[10];
		int count = 0;
		int paramCount = 0;
		for (int i = 0; i < s.Length && s[i] != ' '; i++) { //reads and adds event name to a string.
			com += s[i];
			count = i;
		}
		string eventName = com; //Debug.Log("eventName registered as "+eventName);
		if (count + 1 < s.Length && s[count+1] == ' ') { //if first parameter detected...
			for (int i = 0; i < s.Count(x => x == ' '); i++) { //starts a for loop that runs for the number of spaces detected in the string
				count +=2 ; //increments past the space to the paramName
				paramCount++;
				paramType = AddParamType();
				count++; //skips past = sign
				paramValue = AddParamValue();
				ConvertParam(paramType, paramValue, paramCount-1);
			}
			Array.Resize(ref invokerParams, paramCount);
		} else {
			invokerParams = null;
		}
		if (isStartOfLine == true) { //if this event is being processed at the start of the line, send event and params to GameManager
			gameManager.eventQueue.Enqueue(eventName);
			gameManager.eventQueueParamList.Add(invokerParams);
		} else { //if this event is being processed in the middle of the line, call it immediately from eventManager
			try {
				Debug.Log("Condition eventName == Delay is "+(eventName=="Delay")+", Condition gameManager.playingDialogue== false is "+(gameManager.playingDialogue==false));
			} catch (Exception e) {
				Debug.Log(e);
			}
			if (eventName == "Delay" && gameManager.playingDialogue == true) { //if the event is a delay and dialogue is running, delay.
				dialogueManager.delaySignal = true;
				dialogueManager.delayValue = (float)invokerParams[0];
			} else { //else, call the given event immediately.
				Type type = typeof(EventManager);
				MethodInfo mi = type.GetMethod(eventName);
				mi.Invoke(eventManager, invokerParams);
			}
		}

		string AddParamType() { //reads and returns a parameter type from the string
			com = "";
			while (s[count] != '='){
				com += s[count];
				count++;
			}
			return com;
		}
		string AddParamValue() { //reads and returns a value type from the string int=1 float=0.1f
			com = "";
			for (int i = count; i < s.Length && s[i] != ' ' && s[i] != ']'; i++) {
				com += s[i];
				count = i;
			}
			return com;
		}
		void ConvertParam(string stringType, string stringValue, int n) { //Converts given string values for type and value and adds them to the invokerParams Object array
			switch (stringType) {
				case "b":
				case "bool":
				case "boolean":
					bool paramBool = bool.Parse(stringValue);
					invokerParams[n] = paramBool;
					break;
				case "i":
				case "int":
					int paramInt = Int32.Parse(stringValue);
					invokerParams[n] = paramInt;
					break;
				case "f":
				case "float":
					float paramFloat = float.Parse(stringValue, CultureInfo.InvariantCulture.NumberFormat);
					invokerParams[n] = paramFloat;
					break;
				case "s":
				case "string":
					invokerParams[n] = stringValue;
					break;
				case "c": //expected value should be of format 0.1f,0.0f,0f,1f
				case "color":
					float[] colorFloat = floatArray(4);
					Color paramColor = new Color(colorFloat[0], colorFloat[1], colorFloat[2], colorFloat[3]);
					invokerParams[n] = paramColor;
					break;
				case "v2":
				case "vector2":
					float[] vector2Float = floatArray(2);
					Vector2 paramVector2 = new Vector2(vector2Float[0], vector2Float[1]);
					invokerParams[n] = paramVector2;
					break;
				case "v3":
				case "vector3":
					float[] vector3Float = floatArray(3);
					Vector3 paramVector3 = new Vector3(vector3Float[0], vector3Float[1], vector3Float[2]);
					invokerParams[n] = paramVector3;
					break;
				default:
					Debug.Log("<color=red>TextCommands.ProcessEvent.ConvertParam() could not read given type string: </color>" + paramType);
					break;
			}
			float[] floatArray(int size){
				string[] numString = new string[size];
				float[] floatArr = new float[size];
				int c = 0;
				int index = 0;
				while (c < stringValue.Length) {
					if (c < stringValue.Length && stringValue[c] == ',') {
						c++;
						index++;
					} else {
						numString[index] += stringValue[c];
						c++;
					}
				}
				for (int i = 0; i < size; i++) {
					floatArr[i] = float.Parse(numString[i], CultureInfo.InvariantCulture.NumberFormat);
				}
				return floatArr;
			}
		}
	}
}
