using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharScript : MonoBehaviour
{
    private Animator animController;
    public static EventManager eventManager;
    private int CharInstanceIndex;

    void Start() {
        animController = this.GetComponent<Animator>();
        eventManager = GameObject.Find("Overlord").GetComponent<EventManager>();
    }

    public void SetIndex(int index) {
        CharInstanceIndex = index;
    }
    public void SetAnimInt(int index) {
        animController.SetInteger("animState", index);
    }
    public void DisableAnimator() {
        animController.enabled = false;
    }
    public void DestroySelf() {
        eventManager.RemoveChar(CharInstanceIndex);
    }
}
