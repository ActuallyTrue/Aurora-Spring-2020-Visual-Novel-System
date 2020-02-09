using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventManager : MonoBehaviour //Handles events, such as dialogue box changing, nameplate change, visual effects, branching, etc.
{
    //Scripts
	public static GameManager gameManager;
    public static DialogueManager dialogueManager;
	public static Data data;

	//UI Animations
	public GameObject dialogueBox;
	public static DialogueBoxScript dialogueBoxScript;
	public GameObject namePlate;

	//Characters
	public List<GameObject> CharList = new List<GameObject>();
	private Transform charShakeBackup;
	private bool charShake = false;

	//Visuals
	private GameObject background;
	private List<GameObject> VFXList = new List<GameObject>();
	private int flashIndex;

	//Sound
	public AudioSource musicSource;
	public AudioSource soundSource;
	public AudioSource voiceSource;

	private GameObject cameraObject;
	private bool cameraShake = false;
    //Values
	[HideInInspector]
	public string dialogueBoxState;

    void Start()
    {
        dialogueManager = GetComponent<DialogueManager>();
		data = GetComponent<Data>();

		dialogueBox = GameObject.Find("DialogueBox");
		dialogueBoxScript = dialogueBox.GetComponent<DialogueBoxScript>();

		background = GameObject.Find("Background");

		cameraObject = GameObject.FindWithTag("MainCamera");

		dialogueBoxState = data.dialogueBoxStateList[0];
    }

    #region test cases
    public void Test()
	{
		Debug.Log("Test() activated.");
	}
	public void Test2(int i, float f) {
		Debug.Log("Test2() activated! int = " + i + ", float = " + f);
	}
	public void Test3(string s) {
		Debug.Log("Test3() activated! string = " + s);
	}
	public void Test4(Color c, Vector2 v) {
		Debug.Log("Test4() activated! color = " + c.ToString() +", Vector2 = " + v.ToString());
	}
    #endregion

    #region Dialogue
    public void TalkSpeed(float speed) { //[TalkSpeed f=0.35]
		dialogueManager.waitTime = speed;
    }

    public void PointerMove(Vector3 location) {
        dialogueBoxScript.PointerMove(location);
    }
    public void PointerFlip(bool facingRight) {
        dialogueBoxScript.PointerFlip(facingRight);
    }

    public void BoxState(int state)
    {
        if (dialogueBoxScript.GetDialogueAnim() == false) {
            dialogueBoxScript.EnableDialogueAnim();
        }
        dialogueBoxScript.SetBoxState(state);
    }
    public void UnderColor(Color c) {
        dialogueManager.ChangeUnderlay(c);
    }
    #endregion

    #region Characters
    public void SpawnChar(string charName, string spriteName, Vector3 location) {
		GameObject charInstance = Instantiate(Resources.Load<GameObject>("Art/Characters/" + charName), location, Quaternion.identity);
		CharList.Add(charInstance);
		if (CharList[CharList.Count - 1].GetComponent<CharScript>() != null) {
			CharList[CharList.Count - 1].GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Art/Characters/Expressions/" + spriteName);
			CharScript CharScriptInstance = CharList[CharList.Count - 1].GetComponent<CharScript>();
			CharScriptInstance.SetIndex(CharList.Count - 1);
		}
	}
	public void RemoveChar(int index) {
		if ((index == -1) || index >= CharList.Count) {
			Destroy(CharList[CharList.Count - 1]);
			CharList.RemoveAt(CharList.Count - 1);
		} else {
			Destroy(CharList[index]);
			CharList.RemoveAt(index);
		}
	}
	public void CharExpress(int index, string spriteName) {
		SpriteRenderer charSR = CharList[index].GetComponent<SpriteRenderer>();
		charSR.sprite = Resources.Load<Sprite>("Art/Characters/Expressions/" + spriteName);
	}
	public void CharAnim(int index, int animState) {
		CharList[index].GetComponent<Animator>().enabled = true;
		CharList[index].GetComponent<CharScript>().SetAnimInt(animState);
	}
	public void CharAnimEnd(int index) {
		CharList[index].GetComponent<CharScript>().SetAnimInt(0);
		CharList[index].GetComponent<Animator>().enabled = false;
	}
	public void CharFadeTo(int index, Color color, float speed) { //[CharFadeTo i=index c=colorvalue with alpha less than 1, f=speed]
		IEnumerator charFadeToInstance = CharFadeToCoroutine(index, color, speed);
		StartCoroutine(charFadeToInstance);
		IEnumerator CharFadeToCoroutine(int i, Color c, float spd) {
			SpriteRenderer charSR = CharList[i].GetComponent<SpriteRenderer>();
			Color charColor = charSR.color;
			float[] increment = new float[4] { spd, spd, spd, spd };
			if (charColor.r > c.r)
				increment[0] *= -1;
			if (charColor.g > c.g)
				increment[1] *= -1;
			if (charColor.b > c.b)
				increment[2] *= -1;
			if (charColor.a > c.a)
				increment[3] *= -1;
			while (charSR.color != c) {
				if (charSR.color.r != c.r){
					if (WithinBounds((decimal)(charColor.r += increment[0])) == true){
						charColor.r += increment[0];
					} else {
						charColor.r = c.r;
					}
				}
				if (charSR.color.g != c.g){
					if (WithinBounds((decimal)(charColor.g += increment[1])) == true){
						charColor.g += increment[1];
					} else {
						charColor.g = c.g;
					}
				}
				if (charSR.color.b != c.b){
					if (WithinBounds((decimal)(charColor.b += increment[2])) == true){
						charColor.b += increment[2];
					} else {
						charColor.b = c.b;
					}
				}
				if (charSR.color.a != c.a){
					if (WithinBounds((decimal)(charColor.a += increment[3])) == true){
						charColor.a += increment[3];
					} else {
						charColor.a = c.a;
					}
				}
				charSR.color = charColor;
				yield return new WaitForSeconds(0.03f);
			}
			yield return null;
		}

		bool WithinBounds(decimal d){
			if (Math.Ceiling(d) > 1 || Math.Floor(d) < 0 ) {
				return false;
			} else {
				return true;
			}
		}
	}
	public void CharMove(int index, Vector3 location) {
		IEnumerator charMoveCoroutine = CharMoveCoroutine(index, location);
		StartCoroutine(charMoveCoroutine);
		IEnumerator CharMoveCoroutine(int i, Vector3 l) {
			Transform charTransform = CharList[i].GetComponent<Transform>();
			float step;
			Debug.Log("Initial distance between character and location: " + Vector3.Distance(charTransform.position, l));
			while (Vector3.Distance(charTransform.position, l) > 0.001f) {
				step = Vector3.Distance(charTransform.position, l)*0.4f;
				charTransform.position = Vector3.MoveTowards(charTransform.position, l, step);
				yield return new WaitForSeconds(0.025f);
			}
			charTransform.position = l;
			yield return null;
		}
	}
	public void CharTranslate(int index, Vector3 location, float speed) {
		IEnumerator charTranslateCoroutine = CharTranslateCoroutine(index, location, speed);
		StartCoroutine(charTranslateCoroutine);
		IEnumerator CharTranslateCoroutine(int i, Vector3 l, float s) {
			Transform charTransform = CharList[i].GetComponent<Transform>();
			while (charTransform.position != l) {
				charTransform.position = Vector3.MoveTowards(charTransform.position, l, s);
				Debug.Log("CharTranslate: Current distance: " + Vector3.Distance(charTransform.position, l));
				yield return new WaitForSeconds(0.025f);
			}
			Debug.Log("CharTranslate: Movement completed.");
			yield return null;
		}
	}
	public void CharShakeStart(int index, float intensity) { //needs to be improved to allow jitterbugging of multiple characters
		IEnumerator charShakeStartCoroutine = CharShakeStartCoroutine(index, intensity);
		charShake = true;
		StartCoroutine(charShakeStartCoroutine);
		IEnumerator CharShakeStartCoroutine (int i, float f) { //This is hilariously hacky, but can't think of a better way to ensure a clean looking shake.
			Transform charTransform = CharList[i].GetComponent<Transform>();
			Vector3 charShakeBackup = charTransform.position;
			while (true) {
				charTransform.position = charShakeBackup;
				charTransform.position = new Vector3(charTransform.position.x - f, UnityEngine.Random.Range(charTransform.position.y - f, charTransform.position.y + f), charTransform.position.z);
				yield return new WaitForSeconds(0.02f);
				charTransform.position = charShakeBackup;
				charTransform.position = new Vector3(charTransform.position.x + f, UnityEngine.Random.Range(charTransform.position.y - f, charTransform.position.y + f), charTransform.position.z);
				yield return new WaitForSeconds(0.02f);
				charTransform.position = charShakeBackup;
				charTransform.position = new Vector3(UnityEngine.Random.Range(charTransform.position.x - f, charTransform.position.x + f), charTransform.position.y + f, charTransform.position.z);
				yield return new WaitForSeconds(0.02f);
				charTransform.position = charShakeBackup;
				charTransform.position = new Vector3(UnityEngine.Random.Range(charTransform.position.x - f, charTransform.position.x + f), charTransform.position.y - f, charTransform.position.z);
				yield return new WaitForSeconds(0.02f);
				if (charShake == false) {
					break;
				}
			}
			yield return null;
		}
	}
	public void CharShakeStop(int index) { //Incomplete. Needs to be reworked.
		charShake = false;
		//CharList[index].GetComponent<Transform>().position = charShakeBackup.position;
	}
	public void CharShakeOnce(int index, float intensity) {
		IEnumerator charShakeOnceCoroutine = CharShakeOnceCoroutine(index, intensity);
		StartCoroutine(charShakeOnceCoroutine);
		IEnumerator CharShakeOnceCoroutine (int i, float f) {
			Transform charTransform = CharList[i].GetComponent<Transform>();
			Vector3 charShakeBackup = charTransform.position;
			while (f >= 0) { //This is hilariously hacky, but can't think of a better way to ensure a clean looking shake.
				charTransform.position = charShakeBackup;
				charTransform.position = new Vector3(charTransform.position.x - f, UnityEngine.Random.Range(charTransform.position.y - f, charTransform.position.y + f), charTransform.position.z);
				f -= 0.075f;
				yield return new WaitForSeconds(0.02f);
				charTransform.position = charShakeBackup;
				charTransform.position = new Vector3(charTransform.position.x + f, UnityEngine.Random.Range(charTransform.position.y - f, charTransform.position.y + f), charTransform.position.z);
				f -= 0.075f;
				yield return new WaitForSeconds(0.02f);
				charTransform.position = charShakeBackup;
				charTransform.position = new Vector3(UnityEngine.Random.Range(charTransform.position.x - f, charTransform.position.x + f), charTransform.position.y + f, charTransform.position.z);
				f -= 0.075f;
				yield return new WaitForSeconds(0.02f);
				charTransform.position = charShakeBackup;
				charTransform.position = new Vector3(UnityEngine.Random.Range(charTransform.position.x - f, charTransform.position.x + f), charTransform.position.y - f, charTransform.position.z);
				f -= 0.075f;
				yield return new WaitForSeconds(0.02f);
			}
			charTransform.position = charShakeBackup;
			yield return null;
		}
	}
	#endregion

	#region Visual Effects

	public void BG(string backgroundName) { //[BG s=background_name_in_assets]
		background.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Art/Backgrounds/" + backgroundName);
	}

	public void SpawnVFX(string prefabName) {
		GameObject spawnInstance = Instantiate(Resources.Load<GameObject>("Art/VFX/" + prefabName));
		VFXList.Add(spawnInstance);
		if (VFXList[VFXList.Count - 1].GetComponent<VFXScript>() != null) {
			VFXScript VFXScriptInstance = VFXList[VFXList.Count - 1].GetComponent<VFXScript>();
			VFXScriptInstance.SetIndex(VFXList.Count - 1);
		}
	}
	public void SpawnVFXAt(string prefabName, Vector3 location) {
		GameObject spawnInstance = Instantiate(Resources.Load<GameObject>("Art/VFX/" + prefabName), location, Quaternion.identity);
		VFXList.Add(spawnInstance);
		if (VFXList[VFXList.Count - 1].GetComponent<VFXScript>() != null) {
			VFXScript VFXScriptInstance = VFXList[VFXList.Count - 1].GetComponent<VFXScript>();
			VFXScriptInstance.SetIndex(VFXList.Count - 1);
		}
	}
	public void EndVFX(int index) {
		if ((index == -1) || index >= VFXList.Count) {
			if ((VFXList[VFXList.Count-1].GetComponent<Animator>() != null)) {
				Animator VFXAnimator = VFXList[VFXList.Count - 1].GetComponent<Animator>();
				VFXAnimator.SetBool("animExit", true);
			}
		} else {
			if ((VFXList[index].GetComponent<Animator>() != null)) {
				Animator VFXAnimator = VFXList[index].GetComponent<Animator>();
				VFXAnimator.SetBool("animExit", true);
			}
		}
	}
	public void EndVFXRange(int index) {
		if (index == -1) {
			for (int i = VFXList.Count-1; i >= 0; i--) {
				EndVFX(i);
			}
		} else {
			for (int i = VFXList.Count - 1; i >= index; i--) {
				EndVFX(i);
			}
		}
	}
	public void RemoveVFX(int index) {
		if ((index == -1) || index >= VFXList.Count) {
			Destroy(VFXList[VFXList.Count - 1]);
			VFXList.RemoveAt(VFXList.Count - 1);
		} else {
			Destroy(VFXList[index]);
			VFXList.RemoveAt(index);
		}
	}
	public void RemoveVFXRange(int index) {
		if (index == -1) {
			for (int i = 0; i < VFXList.Count; i++) {
				Destroy(VFXList[i]);
			}
			CleanVFXList();
		} else {
			for (int i = index; i < VFXList.Count; i++) {
				Destroy(VFXList[i]);
			}
			CleanVFXList(index);
		}
	}
	void CleanVFXList(int index = 0) {
		for (int i = VFXList.Count - 1; i >= index; i--) {
			VFXList.RemoveAt(i);
		}
	}

	public void Flash(float time, Color color) { //[Flash f=time c=colorvalue]
		IEnumerator flashInstance = FlashCoroutine(time, color);
		StartCoroutine(flashInstance);
		IEnumerator FlashCoroutine (float t, Color c) {
			VFXList.Add(Instantiate(Resources.Load<GameObject>("Art/VFX/FlashFade")));
			VFXList[VFXList.Count-1].GetComponent<SpriteRenderer>().color = c;
			flashIndex = VFXList.Count - 1;
			if (t == 0) {
				yield return null;
			} else {
				yield return new WaitForSeconds(t);
				RemoveVFX(flashIndex);
				yield return null;
			}
		}
	}
	public void FadeTo(float speed, Color color) { //[FadeTo f=speed c=colorvalue with alpha less than 1]
		IEnumerator fadeToInstance = FadeToCoroutine(speed, color);
		StartCoroutine(fadeToInstance);
		IEnumerator FadeToCoroutine(float s, Color c) {
			VFXList.Add(Instantiate(Resources.Load<GameObject>("Art/VFX/FlashFade")));
			flashIndex = VFXList.Count - 1;
			VFXList[flashIndex].GetComponent<SpriteRenderer>().color = c;
			SpriteRenderer instanceSR = VFXList[flashIndex].GetComponent<SpriteRenderer>();
			while (instanceSR.color.a <= 1f) {
				instanceSR.color = new Color(instanceSR.color.r, instanceSR.color.g, instanceSR.color.b, instanceSR.color.a + s);
				yield return new WaitForSeconds(0.03f);
			}
			yield return null;
		}
	}
	public void FadeFrom(float speed, Color color) { //[FadeFrom f=speed c=colorvalue with alpha greater than 0]
		IEnumerator fadeFromInstance = FadeFromCoroutine(speed);
		StartCoroutine(fadeFromInstance);
		IEnumerator FadeFromCoroutine(float s) {
			if (GameObject.Find("FlashFade(Clone)").activeInHierarchy == false) {
				VFXList.Add(Instantiate(Resources.Load<GameObject>("Art/VFX/FlashFade")));
				flashIndex = VFXList.Count - 1;
			}
			VFXList[flashIndex].GetComponent<SpriteRenderer>().color = color;
			GameObject flashFadeInstance = VFXList[flashIndex];
			SpriteRenderer instanceSR = flashFadeInstance.GetComponent<SpriteRenderer>();
			while (instanceSR.color.a >= 0f) {
				instanceSR.color = new Color(instanceSR.color.r, instanceSR.color.g, instanceSR.color.b, instanceSR.color.a - s);
				yield return new WaitForSeconds(0.03f);
			}
			RemoveVFX(flashIndex);
			yield return null;
		}
	}
	#endregion

	#region Camera
	public void CamShakeStart(float intensity) {
		IEnumerator camShakeInstance = CamShakeCoroutine(intensity);
		cameraShake = true;
		StartCoroutine(camShakeInstance);
		IEnumerator CamShakeCoroutine (float i) {
            Transform camTransform = cameraObject.GetComponent<Transform>();
            while (cameraShake == true) {
				camTransform.position = new Vector3(UnityEngine.Random.Range(-i, i), UnityEngine.Random.Range(-i, i), camTransform.position.z);
				yield return new WaitForSeconds(0.02f);
				if (cameraShake == false) {
					break;
				}
			}
			yield return null;
		}
	}
	public void CamShakeStop() {
		cameraShake = false;
        Transform camTransform = cameraObject.GetComponent<Transform>();
        camTransform.position = new Vector3(0, 0, camTransform.position.z);
	}
	public void CamShakeOnce(float intensity) {
		IEnumerator camShakeOnceInstance = CamShakeOnceCoroutine(intensity);
		StartCoroutine(camShakeOnceInstance);
		IEnumerator CamShakeOnceCoroutine (float i) {
            Transform camTransform = cameraObject.GetComponent<Transform>();
			while (i >= 0) {
				camTransform.position = new Vector3(UnityEngine.Random.Range(-i, i), UnityEngine.Random.Range(-i, i), camTransform.position.z);
				i -= 0.02f;
				yield return new WaitForSeconds(0.02f);
			}
			camTransform.position = new Vector3(0, 0, camTransform.position.z);
			yield return null;
		}
	}

	public void ChangeResolution(int w, int h, bool full) { //WORK IN PROGRESS
		Screen.SetResolution(w, h, full);
		Camera.main.orthographicSize = (float)(Camera.main.orthographicSize * 1.3 * (Screen.width / Screen.height));
	}
	#endregion

	#region Music and Sound
	public void Voice(string voice) { //[Voice i=0]
		voiceSource.clip = Resources.Load<AudioClip>("Sound/Voices/" + voice);
	}
	public void SFX(string soundName) { //[SFX s=sound_name_in_assets]
		soundSource.PlayOneShot(Resources.Load<AudioClip>("Sound/SFX/" + soundName));
	}
	public void PlayMusic(string trackName) { //[PlayMusic s=track_name_in_assets]
		musicSource.clip = Resources.Load<AudioClip>("Sound/Music/" + trackName);
		musicSource.Play();
	}
	public void PauseMusic() {
		if (musicSource.isPlaying)
			musicSource.Pause();
	}
	public void ResumeMusic() {
		musicSource.Play();
	}
	public void PitchMusic(float pitch) { //[PitchMusic f=value between -3 and 3, 1 being normal.]
		musicSource.pitch = pitch;
	}
	public void StopMusic() {
		if (musicSource.isPlaying)
			musicSource.Stop();
	}
	#endregion

	#region Branching and Flag Setting
	public void Branch (int line) {  //[Branch i=1]
		dialogueManager.lineIndex = line;
	}
	public void BranchFlag(int line, string flag = null, int value = -1) { //[Branch i=1 s=datedPapyrus i=1]
		if (flag == null) {
			dialogueManager.lineIndex = line;
		} else {
			Data.Flag tempFlag = data.flagList.Find(x => x.FlagName == flag);
			if (tempFlag.FlagValue == value) {
				dialogueManager.lineIndex = line;
			}
		}
	}
	public void Flag(string flagName, int value) { //[Flag s=datedPapyrus i=1]
		int index = data.flagList.IndexOf(data.flagList.Find(x => x.FlagName == flagName));
		data.flagList[index].FlagValue = value;
	}
	#endregion

	public IEnumerator Delay(float time) {
		yield return new WaitForSeconds(time);
	}
}
