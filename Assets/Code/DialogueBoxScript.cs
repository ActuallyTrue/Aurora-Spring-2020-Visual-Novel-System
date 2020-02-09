using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBoxScript : MonoBehaviour {

	//Scripts
	public static GameManager gameManager;
	public static DialogueManager dialogueManager;
	public static Data data;

	//GameObjects
	public GameObject dialogueBox;
	public GameObject pointer;
	public GameObject pointerGroup;
	public GameObject boxStatus;
	public GameObject nameplate;

	//Animators
	public Animator boxAnim;
	public Animator pointerAnim;
	public Animator boxBackAnim;
	public Animator pointerBackAnim;
	public Animator boxFillAnim;

	public Animator backFillAnim;
	public Animator nameplateAnim;
	public Animator boxStatusAnim;

	private AudioSource boxAudio;

	void Start() {
		gameManager = GameObject.Find("Overlord").GetComponent<GameManager>();
		dialogueManager = GameObject.Find("Overlord").GetComponent<DialogueManager>();
		data = GameObject.Find("Overlord").GetComponent<Data>();

		dialogueBox = this.gameObject;
		pointer = GameObject.Find("BoxPointer");
		pointerGroup = GameObject.Find("BoxPointerGroup");
		boxStatus = GameObject.Find("BoxStatus");
		nameplate = GameObject.Find("Nameplate");

		boxAnim = GetComponent<Animator>();
		pointerAnim = pointer.GetComponent<Animator>();
		boxBackAnim = GameObject.Find("BoxBack").GetComponent<Animator>();
		pointerBackAnim = GameObject.Find("PointerBack").GetComponent<Animator>();
		boxFillAnim = GameObject.Find("BoxFill").GetComponent<Animator>();
		nameplateAnim = nameplate.GetComponent<Animator>();
		boxStatusAnim = boxStatus.GetComponent<Animator>(); 
}
	
	void Update() {
		
	}

	public void PointerMove(Vector3 location) { //note: this seems to overshoot the final location by a little bit
        Vector3 dialogueBoxPosition = dialogueBox.GetComponent<Transform>().position;
        location += dialogueBoxPosition;
		IEnumerator movePointerCoroutine = MovePointerCoroutine(location);
		StartCoroutine(movePointerCoroutine);
		IEnumerator MovePointerCoroutine(Vector3 l) {
			Transform pointerTransform = pointerGroup.transform;
            Debug.Log("Moving from Location: [" + pointerTransform.position + "] to Destination: [" + location + "]");
            float step;
			while (Vector3.Distance(pointerTransform.position, l) > 0.001f) {
				step = Vector3.Distance(pointerTransform.position, l)*0.35f;
				Debug.Log("Stepping a distance of ["+step+"]");
				pointerTransform.position = Vector3.MoveTowards(pointerTransform.position, l, step);
                Debug.Log("Current position: ["+pointerTransform.position+"]");
				yield return new WaitForSeconds(0.025f);
			}
			pointerTransform.position = l;
			yield return null;
		}
	}
	public void PointerSet(Vector3 location) {
		pointerGroup.transform.position = location;
	}
	public void PointerFlip(bool facingRight) {
		pointerGroup.GetComponent<SpriteRenderer>().flipX = !facingRight;
	}

	public bool GetDialogueAnim() {
		return this.GetComponent<Animator>().enabled;
	}
	public void EnableDialogueAnim() { //no bool parameter to support animator events in Unity's Animator Window
		Debug.Log("Called EnableDialogueAnim()");
		boxAnim.enabled = true;
		pointerAnim.enabled = true;
		boxBackAnim.enabled = true;
		pointerBackAnim.enabled = true;
		nameplateAnim.enabled = true;
		boxFillAnim.enabled = true;
	}
	public void DisableDialogueAnim() {
		Debug.Log("Called DisableDialogueAnim()");
		boxAnim.enabled = false;
		pointerAnim.enabled = false;
		boxBackAnim.enabled = false;
		pointerBackAnim.enabled = false;
		nameplateAnim.enabled = false;
		boxFillAnim.enabled = false;
	}

	public void SetBoxState(int state) {
		Debug.Log("Called SetBoxState(" + state + ")");
		boxAnim.SetInteger("animState", state);
		pointerAnim.SetInteger("animState", state);
		boxBackAnim.SetInteger("animState", state);
		pointerBackAnim.SetInteger("animState", state);
		boxFillAnim.SetInteger("animState", state);

		nameplateAnim.SetInteger("animState", state);
	}
	public void SetNameState(int state) {
		nameplateAnim.SetInteger("charID", state);
	}
	public void SetBoxStatusState(int state) {
		boxStatusAnim.SetInteger("animState", state);
	}
	public void SetBoxFill(int state) {
		boxFillAnim.SetInteger("boxFill", state);
	}

	public void SetTalking(bool boolean) {
		Debug.Log("Called SetTalking(" + boolean + ")");
		boxAnim.SetBool("isTalking", boolean);
		pointerAnim.SetBool("isTalking", boolean);
		boxBackAnim.SetBool("isTalking", boolean);
		pointerBackAnim.SetBool("isTalking", boolean);
		boxFillAnim.SetBool("isTalking", boolean);

		nameplateAnim.SetBool("isTalking", boolean);
	}
}
