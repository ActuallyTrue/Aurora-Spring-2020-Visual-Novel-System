using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXScript : MonoBehaviour
{
    private Animator animController;
    public static EventManager eventManager;
    private int VFXInstanceIndex;
    void Start() {
        animController = this.GetComponent<Animator>();
        animController.SetBool("animExit", false);
        eventManager = GameObject.Find("Overlord").GetComponent<EventManager>();
    }

    public void SetIndex(int i) {
        VFXInstanceIndex = i;
    }

    public void DestroySelf() {
        eventManager.RemoveVFX(VFXInstanceIndex);
    }
}
