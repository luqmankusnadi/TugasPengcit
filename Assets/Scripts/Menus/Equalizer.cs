using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eppy;

public class Equalizer : MonoBehaviour, IPointerClickHandler {
	private WebCamTexture deviceCamera;
	private Texture2D image;
	private bool isEdit = false;

	void Start () {
		deviceCamera = new WebCamTexture ();
		this.GetComponent<RawImage> ().texture = deviceCamera;
		deviceCamera.Play ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			deviceCamera.Stop();
			Application.LoadLevel("MainMenu");
		}
	}
	
	public Texture2D GetImage(){
		Texture2D texture = new Texture2D (deviceCamera.width, deviceCamera.height);
		texture.SetPixels32 (deviceCamera.GetPixels32 ());
		texture.Apply ();
		return texture;
	}
	
	public void Edit(){
		RawTexture2D rawTexture = new RawTexture2D (image);
		//rawTexture.Grayscale ();
		rawTexture.Equalize ();
	
		this.GetComponent<RawImage> ().texture = rawTexture.ToTexture2D ();
	}
	#region IPointerClickHandler implementation
	
	public void OnPointerClick (PointerEventData eventData)
	{
		if (deviceCamera.isPlaying) {
			deviceCamera.Pause ();
			image = GetImage();
			isEdit = true;
		} else {
			if (isEdit) {
				Edit ();
				isEdit = false;
			}
			else{
				this.GetComponent<RawImage> ().texture = deviceCamera;
				deviceCamera.Play ();
			}
		}
	}
	
	#endregion
}
