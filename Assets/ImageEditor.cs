using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ImageEditor : MonoBehaviour, IPointerClickHandler {
	private WebCamTexture deviceCamera;
	private Texture2D image;
	public GameObject text;
	private bool isEdit = false;
	// Use this for initialization
	void Start () {
		/*
		deviceCamera = new WebCamTexture ();
		this.GetComponent<RawImage> ().texture = deviceCamera;
		deviceCamera.Play ();
		text.SetActive (false);
		/*/
		Texture2D texture = (Texture2D)this.GetComponent<RawImage> ().mainTexture;
		Texture2D texture2 = new Texture2D (texture.width, texture.height);
		texture2.SetPixels32 (texture.GetPixels32());
		texture2.Apply ();
		this.GetComponent<RawImage> ().texture = texture2;
		RawTexture2D rawTexture = new RawTexture2D (texture2);
		rawTexture.Grayscale ();
		float[,] matrix = new float[3, 3] {
			{-1f,0f,1f},
			{-1f,0f,1f},
			{-1f,0f,1f}
		};

		rawTexture.Convolve (matrix);
		//rawTexture.OtsuThreshold ();
		rawTexture.BandDetection ();
		Debug.Log (rawTexture.ColorCount ());
		this.GetComponent<RawImage> ().texture = rawTexture.ToTexture2D();
		//((Texture2D)this.GetComponent<RawImage> ().texture).ZhangSuenThinning();
		//((Texture2D)this.GetComponent<RawImage> ().texture).BlobDetection();
		//int[] results = ((Texture2D)this.GetComponent<RawImage> ().texture).Classify ();
		//foreach (int result in results) {
		//	Debug.Log ("-> "+result);
		//}

	}
	/*
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}
		if (isEdit) {
			Edit ();
			isEdit = false;
		}
	}

	public Texture2D GetImage(){
		Texture2D texture = new Texture2D (deviceCamera.width, deviceCamera.height);
		texture.SetPixels32 (deviceCamera.GetPixels32 ());
		texture.Apply ();
		return texture;
	}

	public void Edit(){
		image.OtsuThreshold();
		image.ZhangSuenThinning();
		int[] results = image.Classify();
		string resultString = "Angka :"; 
		foreach (int result in results) {
			resultString += (" "+result); 
		}
		this.GetComponent<RawImage> ().texture = image;
		text.GetComponent<Text> ().text = resultString;
	}
	#region IPointerClickHandler implementation

	public void OnPointerClick (PointerEventData eventData)
	{
		if (deviceCamera.isPlaying) {
			deviceCamera.Pause ();
			image = GetImage();
			isEdit = true;
			text.SetActive (true);
		} else {
			this.GetComponent<RawImage> ().texture = deviceCamera;
			deviceCamera.Play ();
			text.SetActive(false);
		}
	}

	#endregion
	*/

	#region IPointerClickHandler implementation

	public void OnPointerClick (PointerEventData eventData)
	{
		float[,] matrix = new float[3, 3] {
			{-1f,0f,1f},
			{-1f,0f,1f},
			{-1f,0f,1f}
		};
		((Texture2D)this.GetComponent<RawImage> ().texture).Convolve (matrix);
	}

	#endregion
}
