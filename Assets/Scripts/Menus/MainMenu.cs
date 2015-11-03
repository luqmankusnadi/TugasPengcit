using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eppy;

public class MainMenu : MonoBehaviour {
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}
	}

	public void OnMenuButtonClick(int menu){
		switch (menu) {
		case 0:
			Application.LoadLevel("Equalizer");
			break;
		case 1:
			Application.LoadLevel("Threshold");
			break;
		case 2:
			Application.LoadLevel("ChainCode");
			break;
		case 3:
			Application.LoadLevel("TurnCode");
			break;
		case 4:
			Application.LoadLevel("Thinning");
			break;
		}
	}
}
