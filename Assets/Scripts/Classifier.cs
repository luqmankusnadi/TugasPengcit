using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Classifier : MonoBehaviour {
	private Texture2D[] imageData;
	public string path;
	public Dictionary<string, int[]> chainCodeDataSet = new Dictionary<string, int[]>();
	public Dictionary<string, int[]> chainCodeCountDataSet = new Dictionary<string, int[]>();
	public Dictionary<string, int[]> turnCodeDataSet = new Dictionary<string, int[]>();
	public Dictionary<string, int[]> turnCodeCountDataSet = new Dictionary<string, int[]>();
	// Use this for initialization
	void Start () {
		imageData = System.Array.ConvertAll (Resources.LoadAll ("Images/"+path, typeof(Texture2D)), i => (Texture2D)i);
		foreach (Texture2D image in imageData) {
			RawTexture2D rawTexture2D = new RawTexture2D(image);
			int[] chainCode = rawTexture2D.GetChainCode();
			int[] turnCode = ChainCodeToTurnCode(chainCode);
			foreach(int x in turnCode){;
				Debug.Log (image.name+"-- "+x);
			}
			chainCodeDataSet.Add(image.name, chainCode);
			chainCodeCountDataSet.Add(image.name, CountChainCode(chainCode));
			turnCodeDataSet.Add(image.name, turnCode);
			turnCodeCountDataSet.Add(image.name, CountTurnCode(turnCode));
		}
	}

	private int[] CountChainCode(int[] chainCode){
		int[] count = new int[8];
		foreach (int code in chainCode) {
			count[code]++;
		}
		return count;
	}
	private int[] CountTurnCode(int[] turnCode){
		int[] count = new int[6];
		foreach (int code in turnCode) {
			if(code == 3) count[0]++;
			else if(code == 2) count[1]++;
			else if(code == 1) count[2]++;
			else if(code == -3) count[3]++;
			else if(code == -2) count[4]++;
			else if(code == -1) count[5]++;
		}
		return count;
	}

	public string ClassifyChainCode(RawTexture2D rawTexture){
		string chainCodeClass = "";
		int lowestScore = 99999999;
		int[] count = CountChainCode(rawTexture.GetChainCode ());
		foreach (KeyValuePair<string, int[]> data in chainCodeCountDataSet) {
			int score = 0;
			for(int i = 0; i < 8; i++){
				score += Mathf.Abs(count[i] - data.Value[i]);
			}
			if(score < lowestScore){
				lowestScore = score;
				chainCodeClass = data.Key;
			}
		}
		return chainCodeClass;
	}
	public string ClassifyTurnCountCode(RawTexture2D rawTexture){
		string turnCodeClass = "";
		int lowestScore = 99999999;
		int[] count = CountChainCode(rawTexture.GetChainCode ());
		foreach (KeyValuePair<string, int[]> data in turnCodeCountDataSet) {
			int score = 0;
			for(int i = 0; i < 6; i++){
				score += Mathf.Abs(count[i] - data.Value[i]);
			}
			if(score < lowestScore){
				lowestScore = score;
				turnCodeClass = data.Key;
			}
		}
		return turnCodeClass;
	}

	public string ClassifyTurnCode(RawTexture2D rawTexture){
		string turnCodeClass = "";
		int lowestScore = 0;
		int[] turnCode = ChainCodeToTurnCode(rawTexture.GetChainCode());
		foreach (KeyValuePair<string, int[]> data in turnCodeDataSet) {
			int score = 0;
			for(int i = 0; i < Mathf.Min(turnCode.Length, data.Value.Length); i++){
				score += turnCode[i] == data.Value[i] ? 1 : 0;
			}
			if(score > lowestScore){
				lowestScore = score;
				turnCodeClass = data.Key;
			}
		}
		return turnCodeClass;
	}

	public int[] ChainCodeToTurnCode(int[] chainCode){
		//  -3      3 
		//  -2      2
		//  -1      1
		List<int> turnCode = new List<int> ();
		for (int i = 0; i < chainCode.Length; i++) {
			if(chainCode[i] != chainCode[(i+1)%chainCode.Length]){
				int code = ((chainCode[(i+1)%chainCode.Length] - chainCode[i] +8)%8)-4;
				//Debug.Log(chainCode[i]+"==="+chainCode[(i+1)%chainCode.Length]+"=>"+code);
				turnCode.Add(code);
			}
		}
		Debug.Log ("panjang " + turnCode.Count);
		return turnCode.ToArray ();
	}

	public bool[,] ConvertToMatrix(RawTexture2D texture){
		bool[,] matrix = new bool[5, 5];
		for (int j = 0; j < 5; j++) {
			for(int i = 0; i < 5; i++){
				bool isFound = false;
				for(int y = j*texture.height/5; y < (j+1)*texture.height/5; y++){
					for(int x = i*texture.width/5; x < (i+1)*texture.width/5; x++){
						if(texture.GetPixel(x,y).r < 255){
							isFound = true;
							break;
						}
					}
					if(isFound) break;
				}
				matrix[j,i] = isFound;
			}
		}
		return matrix;
	}
	// Update is called once per frame
	void Update () {
	
	}
}
