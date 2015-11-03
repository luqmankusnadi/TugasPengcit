using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TurnCode : MonoBehaviour, IDragHandler, IBeginDragHandler{
	Vector2 pastPos;
	public GameObject resultView;
	Texture2D texture;
	// Use this for initialization
	void Start () {
		texture = new Texture2D(Screen.width, Screen.height);
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = FilterMode.Point;
		this.GetComponent<RawImage> ().texture = texture;
		ClearScreen ();
		resultView.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit();
		}
	}
	
	public void ClearScreen(){
		Color32[] color = new Color32[texture.width * texture.height];
		for (int i = 0; i < color.Length; i++)
			color [i] = Color.white;
		texture.SetPixels32 (color);
		texture.Apply ();
	}
	
	public void ProcessTexture(){
		RawTexture2D rawTexture = new RawTexture2D (texture);
		List<RawTexture2D> blobs = rawTexture.BlobDetection ();
		bool[,] matrix = this.GetComponent<Classifier> ().ConvertToMatrix (blobs [0]);
		RawTexture2D resultRawTexture = new RawTexture2D (matrix);
		Texture2D resultTexture = resultRawTexture.ToTexture2D ();
		resultView.transform.GetChild(0).GetComponent<RawImage> ().texture = resultTexture;
		string resultClass = this.GetComponent<Classifier> ().ClassifyTurnCode (resultRawTexture);
		resultView.transform.GetChild (1).GetComponent<Text> ().text = "Angka : " + resultClass;
		resultView.SetActive (true);
		
	}
	
	#region IBeginDragHandler implementation
	
	
	public void OnBeginDrag (PointerEventData eventData)
	{
		pastPos = eventData.position;
	}
	
	
	#endregion
	
	#region IDragHandler implementation
	
	public void OnDrag (PointerEventData eventData)
	{
		for (int j = -5; j <= 5; j++) { 
			for (int i = -5; i <= 5; i++) {
				int x0 = (int)pastPos.x;
				int y0 = (int)pastPos.y;
				int x1 = (int)eventData.position.x;
				int y1 = (int)eventData.position.y;
				PlotLine(x0, y0, x1, y1);
			}
		}
		texture.Apply ();
		pastPos = eventData.position;
	}
	
	#endregion
	
	void PlotLine(int x0, int y0, int x1, int y1)
	{
		int dx = Mathf.Abs (x1 - x0); int sx = x0<x1 ? 1 : -1;
		int dy = -Mathf.Abs (y1 - y0); int sy = y0<y1 ? 1 : -1; 
		int err = dx+dy, e2; /* error value e_xy */
		
		for(;;){  /* loop */
			PlotSquare(x0, y0, 5);
			if (x0==x1 && y0==y1) break;
			e2 = 2*err;
			if (e2 >= dy) { err += dy; x0 += sx; } /* e_xy+e_x > 0 */
			if (e2 <= dx) { err += dx; y0 += sy; } /* e_xy+e_y < 0 */
		}
	}
	
	void PlotSquare(int x, int y, int s){
		for (int j = 0; j < s; j++) {
			for(int i = 0; i < s; i++){
				texture.SetPixel(x+i-s/2,y+j-s/2, Color.black);
			}
		}
	}
	
	void PlotCircle(int xm, int ym, int r)
	{
		int x = -r, y = 0, err = 2-2*r; /* II. Quadrant */ 
		do {
			texture.SetPixel(xm-x, ym+y, Color.black); /*   I. Quadrant */
			texture.SetPixel(xm-y, ym-x, Color.black); /*  II. Quadrant */
			texture.SetPixel(xm+x, ym-y, Color.black); /* III. Quadrant */
			texture.SetPixel(xm+y, ym+x, Color.black); /*  IV. Quadrant */
			r = err;
			if (r <= y) err += ++y*2+1;           /* e_xy+e_y < 0 */
			if (r > x || err > y) err += ++x*2+1; /* e_xy+e_x > 0 or no 2nd y-step */
		} while (x < 0);
	}
	
	void PlotLineWidth(int x0, int y0, int x1, int y1, float wd)
	{ 
		int dx = Mathf.Abs(x1-x0); int sx = x0 < x1 ? 1 : -1; 
		int dy = Mathf.Abs(y1-y0); int sy = y0 < y1 ? 1 : -1;
		
		int err = dx-dy; int e2; int x2; int y2;                          /* error value e_xy */
		float ed = dx+dy == 0 ? 1 : Mathf.Sqrt((float)dx*dx+(float)dy*dy);
		
		for (wd = (wd+1)/2; ; ) {                                   /* pixel loop */
			float g = (float)Mathf.Max(0,255*(Mathf.Abs(err-dx+dy)/ed-wd+1))/255f;
			texture.SetPixel(x0,y0, new Color(g,g,g));
			e2 = err; x2 = x0;
			if (2*e2 >= -dx) {                                           /* x step */
				for (e2 += dy, y2 = y0; e2 < ed*wd && (y1 != y2 || dx > dy); e2 += dx){
					g = (float)Mathf.Max(0,255*(Mathf.Abs(e2)/ed-wd+1))/255f;
					texture.SetPixel(x0, y2 += sy, new Color(g,g,g));
				}
				if (x0 == x1) break;
				e2 = err; err -= dy; x0 += sx; 
			} 
			if (2*e2 <= dy) {                                            /* y step */
				for (e2 = dx-e2; e2 < ed*wd && (x1 != x2 || dx < dy); e2 += dy){
					g = (float)Mathf.Max(0,255*(Mathf.Abs(e2)/ed-wd+1))/255f;
					texture.SetPixel(x2 += sx, y0, new Color(g,g,g));
				}
				if (y0 == y1) break;
				err += dx; y0 += sy; 
			}
		}
	}
}
