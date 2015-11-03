using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eppy;

public class ChainCode : MonoBehaviour, IPointerClickHandler {
	private WebCamTexture deviceCamera;
	private Texture2D image;
	public GameObject text;
	private bool isEdit = false;
	// Use this for initialization
	void Start () {
		deviceCamera = new WebCamTexture ();
		this.GetComponent<RawImage> ().texture = deviceCamera;
		deviceCamera.Play ();
		text.SetActive (false);
	}
	
	// Update is called once per frame
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
		rawTexture.Grayscale ();
		
		//rawTexture.OtsuThreshold ();
		
		float[,] matrix = new float[3, 3] {
			{-1f,0f,1f},
			{-1f,0f,1f},
			{-1f,0f,1f}
		};
		float[,] gaussMatrix = new float[5, 5]{
			{0.003765f, 0.015019f, 0.023792f, 0.015019f, 0.003765f},
			{0.015019f, 0.059912f, 0.094907f, 0.059912f, 0.015019f},
			{0.023792f, 0.094907f, 0.150342f, 0.094907f, 0.023792f},
			{0.015019f, 0.059912f, 0.094907f, 0.059912f, 0.015019f},
			{0.003765f, 0.015019f, 0.023792f, 0.015019f, 0.003765f}
		};
		
		rawTexture.Convolve (matrix);
		
		
		RawTexture2D originRawTexture = new RawTexture2D (image);
		originRawTexture.Grayscale ();
		Tuple<int,int>[] bands = rawTexture.BandDetection();
		Tuple<int,int>[] plates = new Tuple<int, int>[bands.Length];
		RawTexture2D[] bandTextures = new RawTexture2D[bands.Length];
		RawTexture2D[] plateTextures = new RawTexture2D[bands.Length];
		for(int i = 0; i < bands.Length; i++) {
			bandTextures[i] = rawTexture.BandClipping (bands[i]);
			plates[i] = bandTextures[i].PlateDetection();
			bandTextures[i] = originRawTexture.BandClipping(bands[i]);
			plateTextures[i] = bandTextures[i].PlateClipping(plates[i]);
		}
		
		RawTexture2D plate = plateTextures [0].PlateClipping2nd ();
		plate.Convolve (gaussMatrix);
		plate.Inverse ();
		plate.OtsuThreshold ();
		this.GetComponent<RawImage> ().texture = plate.ToTexture2D ();
		//plate.ZhangSuenThinning ();
		//Debug.Log (rawTexture.ColorCount ());
		
		List<string> results = new List<string> ();
		List<RawTexture2D> blobs = plate.BlobDetection ();
		Debug.Log ("blob" + blobs.Count);
		foreach (RawTexture2D blob in blobs) {
			//blob.GetChainCode();
			results.Add(this.GetComponent<Classifier>().ClassifyChainCode(blob));
		}
		
		string resultString = "Angka :"; 
		foreach (string result in results) {
			resultString += (" "+result); 
		}
		text.GetComponent<Text> ().text = resultString;
		
		//this.GetComponent<RawImage> ().texture = blobs[0].ToTexture2D();
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
			if (isEdit) {
				Edit ();
				isEdit = false;
			}
			else{
				this.GetComponent<RawImage> ().texture = deviceCamera;
				deviceCamera.Play ();
				text.SetActive(false);
				text.GetComponent<Text> ().text = "O";
			}
		}
	}
	
	#endregion
	
}
