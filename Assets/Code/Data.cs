using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Data : MonoBehaviour //Holds immutable data at the moment
{
	public class Flag {
		public string FlagName { get; set; }
		public int FlagValue { get; set; }
		public int FlagID { get; set; }
		public override string ToString() {
			return "FlagName: " + FlagName + ", FlagValue: " + FlagValue;
		}
		public override bool Equals(object obj) {
			if (obj == null) return false;
			Flag objAsFlag = obj as Flag;
			if (objAsFlag == null) return false;
			else return Equals(objAsFlag);
		}
		public override int GetHashCode() {
			return FlagID;
		}
		public bool Equals(Flag other) {
			if (other == null) return false;
			return (this.FlagID.Equals(other.FlagID));
		}
	}

	public readonly List<string> speakerList = new List<string>
	{
		"Default",
		"Morgan",
		"Jules",
		"Thena",
		"EmptySlot1",
		"EmptySlot2",
		"???"
	};
	public readonly List<string> dialogueBoxStateList = new List<string>
	{
		"Default",
		"Normal",
		"Exclaim",
		"Think",
		"Question",
		"EmptySlot1",
		"EmptySlot2"
	};

	public List<Flag> flagList = new List<Flag> {
		new Flag { FlagName = "datedPapyrus", FlagValue = 0 },
		new Flag { FlagName = "killedPapyrus", FlagValue = 0 }
	};
	public AudioClip[] voiceClips = new AudioClip[4];
}
