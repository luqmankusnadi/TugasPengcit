using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class Texture2DExtension {
	public static int[] Histogram(this Texture2D value, int channel = 0){	
		int[] histogram = new int[256];
		Color32[] colors = value.GetPixels32 ();
		if (channel == 0) {
			foreach (Color32 color in colors) {
				int grayscale = (color.r + color.g + color.b) / 3;
				histogram[grayscale]++;
			}
		} else if (channel == 1) {
			foreach (Color32 color in colors) histogram[color.r]++;
		} else if (channel == 2) {
			foreach (Color32 color in colors) histogram[color.g]++;
		} else if (channel == 3) {
			foreach (Color32 color in colors) histogram[color.b]++;
		}
		return histogram;
	}

	public static void Grayscale(this Texture2D value){
		Color32[] colors = value.GetPixels32 ();
		for (int i = 0; i < colors.Length; i++) {
			byte grayscale = (byte)((colors[i].r + colors[i].g + colors[i].b) / 3);
			colors[i] = new Color32(grayscale, grayscale, grayscale, 255);
		}
		value.SetPixels32 (colors);
		value.Apply ();
	}

	public static int OtsuThreshold(this Texture2D value){
		Color32[] colors = value.GetPixels32 ();
		int[] histogram = value.Histogram ();
		// Total number of pixels
		int total = colors.Length;
		
		float sum = 0;
		for (int i=0 ; i<256 ; i++) sum += i * histogram[i];
		
		float sumB = 0;
		int wB = 0;
		int wF = 0;
		
		float varMax = 0;
		int threshold = 0;
		
		for (int i=0 ; i<256 ; i++) {
			wB += histogram[i];
			if (wB == 0) continue;
			
			wF = total - wB;
			if (wF == 0) break;
			
			sumB += (float) (i * histogram[i]);
			
			float mB = sumB / wB;
			float mF = (sum - sumB) / wF;
			
			// Calculate Between Class Variance
			float varBetween = (float)wB * (float)wF * (mB - mF) * (mB - mF);

			if (varBetween > varMax) {
				varMax = varBetween;
				threshold = i;
			}
		}
		for (int i = 0; i < colors.Length; i++) {
			byte grayscale = (byte)((colors[i].r + colors[i].g + colors[i].b) / 3);
			byte color = 255;
			if(grayscale < threshold) color = 0;
			colors[i] = new Color32(color, color, color, 255);
		}
		value.SetPixels32 (colors);
		value.Apply ();
		return threshold;
	}

	public static void ZhangSuenThinning(this Texture2D value){
		//value.OtsuThreshold ();
		Color32[] colors = value.GetPixels32 ();
		int w = value.width;
		int h = value.height;
		int s = colors.Length;
		bool[] p = new bool[9];
		bool step1 = true;
		bool step2 = true;
		while(step1 || step2) {
			step1 = false;
			step2 = false;
			for (int j = 0; j < 2; j++){
				for (int i = 0; i < s; i++) {
					int x = i%w;
					int y = i/w;
					p [0] = colors [i].r != 255;
					p [1] = y - 1 < 0 ? false : colors [i - w].r != 255;
					p [2] = y - 1 < 0 || x + 1 > w - 1 ? false : colors [i - w + 1].r != 255;
					p [3] = x + 1 > w - 1 ? false : colors [i + 1].r != 255;
					p [4] = y + 1 > h - 1 || x + 1 > w - 1 ? false : colors [i + w + 1].r != 255;
					p [5] = y + 1 > h - 1 ? false : colors [i + w].r != 255;
					p [6] = y + 1 > h - 1 || x - 1 < 0  ? false : colors [i + w - 1].r != 255;
					p [7] = x - 1 < 0 ? false : colors [i - 1].r != 255;
					p [8] = y - 1 < 0 || x - 1 < 0 ? false : colors [i - w - 1].r != 255;
					int np = 0;
					for (int k = 1; k < 9; k++) {
						np += (p [k] ? 1 : 0);
					}
					int sp = 0;
					for (int k = 1; k < 9; k++) {
						if (p [k] && !p [(k % 8) + 1])
							sp++;
					}
					if(j == 0){
						if (p [0] && np >= 2 && np <= 6 && sp == 1 && !(p [1] && p [3] && p [5]) && !(p [3] && p [5] && p [7])){
							colors [i].b = 255;
							step1 = true;
						}
					} else if(j == 1){
						if (p [0] && np >= 2 && np <= 6 && sp == 1 && !(p [1] && p [3] && p [7]) && !(p [1] && p [5] && p [7])){
							colors [i].b = 255;
							step2 = true;
						}
					}
				}
				for (int i = 0; i < s; i++) {
					colors [i].r = colors [i].b;
					colors [i].g = colors [i].b;
				}
			}
		}
		value.SetPixels32 (colors);
		value.Apply ();
	}

	public static List<int[,]> BlobDetection(this Texture2D value){
		Color32[] colors = value.GetPixels32 ();
		int w = value.width;
		int h = value.height;
		int s = colors.Length;
		bool[] p = new bool[5];
		int[] pL = new int[5]; 
		int[,] labels = new int[h, w];
		int[] linked = new int[w*h/4];
		int label = 1;
		for (int i = 0; i < s; i++) {
			int x = i%w;
			int y = i/w;

			p [0] = colors [i].r != 255;
			pL [0] = labels[y,x];

			if(y - 1 < 0 || x - 1 < 0){
				p[1] = false;
				pL[1] = 0;
			} else {
				p[1] = colors [i - w - 1].r != 255;
				pL[1] = linked[labels[y-1,x-1]];
			}
			if(y - 1 < 0){
				p[2] = false;
				pL[2] = 0;
			} else {
				p[2] = colors [i - w].r != 255;
				pL[2] = linked[labels[y-1,x]];
			}
			if(y - 1 < 0 || x + 1 > w - 1){
				p[3] = false;
				pL[3] = 0;
			} else {
				p[3] = colors [i - w + 1].r != 255;
				pL[3] = linked[labels[y-1,x+1]];
			}
			if(x - 1 < 0){
				p[4] = false;
				pL[4] = 0;
			} else {
				p[4] = colors [i - 1].r != 255;
				pL[4] = linked[labels[y,x-1]];
			}


			if(p[0]){
				if(pL[1]==0 && pL[2]==0 && pL[3]==0 && pL[4]==0){
					labels[y,x] = label;
					linked[label] = label;
					label++;
				} else {
					int minLabel = 999;
					for(int j = 1; j<5; j++){
						if(pL[j] != 0 && pL[j] < minLabel) minLabel = pL[j];  
					}
					labels[y,x] = minLabel;
					if(pL[1]!=0){
						linked[pL[1]] = minLabel;
						//labels[y-1,x-1] = minLabel;
					}
					if(pL[2]!=0){
						linked[pL[2]] = minLabel;
						//labels[y-1,x] = minLabel;
					}
					if(pL[3]!=0){
						linked[pL[3]] = minLabel;
						//labels[y-1,x+1] = minLabel;
					}
					if(pL[4]!=0){
						linked[pL[4]] = minLabel;
						//labels[y,x-1] = minLabel;
					}
				}
			}
		}
		int k = 1;
		while (linked[k]!=0) {
			int j = k;
			while(linked[j] != j){
				j = linked[j];
			}
			linked[k] = j;
			k++;
		}

		k = 1;
		int maxLabel = linked.Distinct ().Count ()-1;
		int[] newLabels = new int[maxLabel+1];
		int index = 1;
		while (linked[k]!=0) {
			bool isFound = false;
			for (int i = 1; i <= index-1; i++) {
				if(linked[k] == newLabels[i] && linked[k]!=0){
					linked[k] = i;
					isFound = true;
				}
			}
			if(!isFound) {
				newLabels[index] = linked[k];
				linked[k] = index;
				index++;
			}
			k++;
		}

		for (int j = 0; j < h; j++) {
			for(int i = 0; i < w; i++){
				labels[j,i] = linked[labels[j,i]];
			}
		}
		for (int j = 0; j < h; j++) {
			for(int i = 0; i < w; i++){
				if(labels[j,i] != 0){
					colors[j*w+i].r = (byte)((255 * labels[j,i] / maxLabel)%256);
					colors[j*w+i].b = (byte)(255-colors[j*w+i].r);
					colors[j*w+i].g = (byte)((colors[j*w+i].r+colors[j*w+i].b)/2);
				}
			}
		}
		Rect[] blobRect = new Rect[maxLabel];
		for (int i = 0; i < maxLabel; i++) {
			blobRect[i] = new Rect(w,h,0,0);
		}
		for (int j = 0; j < h; j++) {
			for(int i = 0; i < w; i++){
				if(labels[j,i] != 0){
					for(int lb = 0; lb < maxLabel; lb++){
						if(labels[j,i]-1 == lb){
							if(i < blobRect[lb].x) blobRect[lb].x = i;
							if(i > blobRect[lb].width) blobRect[lb].width = i;
							if(j < blobRect[lb].y) blobRect[lb].y = j;
							if(j > blobRect[lb].height) blobRect[lb].height = j;
						}
					}
				}
			}
		}

		List<int[,]> blobs = new List<int[,]> ();
		for (int i = 0; i < maxLabel; i++) {
			int blobX = (int)blobRect[i].x;
			int blobY = (int)blobRect[i].y;
			int blobW = (int)(blobRect[i].width - blobRect[i].x + 1);
			int blobH = (int)(blobRect[i].height - blobRect[i].y + 1);
			if(blobW*blobH>9){
				int[,] blob = new int[blobH, blobW];
				for(int yy=0; yy < blobH; yy++){
					for(int xx=0; xx<blobW; xx++){
						blob[yy,xx] = labels[blobY+yy, blobX+xx] == 0 ? 0 : 1;
					}
				}
				blobs.Add(blob);
			}
		}

		value.SetPixels32 (colors);
		value.Apply ();
		return blobs;
	}

	public static int[] Classify(this Texture2D value){
		List<int[,]> blobs = value.BlobDetection ();
		float[] blobSegment = new float[7];
		int[] blobsClass = new int[blobs.Count];
		int blobN = 0;
		foreach (int[,] blob in blobs) {
			int w = blob.GetLength(1);
			int h = blob.GetLength(0);
			int mW = w/2;
			int mH = h/2;
			int tH1 = h/4;
			int tH2 = h*3/4;
			for(int j = 0; j < mH; j++){
				for(int i = 0; i < mW; i++){
					if(blob[j,i]!=0){
						blobSegment[0]++;
						break;
					}
				}
			}
			blobSegment[0] /= (float)mH;
			for(int j = 0; j < mH; j++){
				for(int i = mW; i < w; i++){
					if(blob[j,i]!=0){
						blobSegment[1]++;
						break;
					}
				}
			}
			blobSegment[1] /= (float)mH;
			for(int j = mH; j < h; j++){
				for(int i = 0; i < mW; i++){
					if(blob[j,i]!=0){
						blobSegment[2]++;
						break;
					}
				}
			}
			blobSegment[2] /= (float)(h - mH);
			for(int j = mH; j < h; j++){
				for(int i = mW; i < w; i++){
					if(blob[j,i]!=0){
						blobSegment[3]++;
						break;
					}
				}
			}
			blobSegment[3] /= (float)(h - mH);
			for(int i = 0; i < w; i++){
				for(int j = 0; j < tH1; j++){
					if(blob[j,i]!=0){
						blobSegment[4]++;
						break;
					}
				}
			}
			blobSegment[4] /= (float)w;
			for(int i = 0; i < w; i++){
				for(int j = tH1; j < tH2; j++){
					if(blob[j,i]!=0){
						blobSegment[5]++;
						break;
					}
				}
			}
			blobSegment[5] /= (float)w;
			for(int i = 0; i < w; i++){
				for(int j = tH2; j < h; j++){
					if(blob[j,i]!=0){
						blobSegment[6]++;
						break;
					}
				}
			}
			blobSegment[6] /= (float)w;
			float[,] classifier = new float[10,7]{
				{1,1,1,1,1,0,1},
				{0,1,0,1,0,0,0},
				{1,0,0,1,1,1,1},
				{0,1,0,1,1,1,1},
				{0,1,1,1,0,1,0},
				{0,1,1,0,1,1,1},
				{1,1,1,0,1,1,1},
				{0,1,0,1,0,0,1},
				{1,1,1,1,1,1,1},
				{0,1,1,1,1,1,1}}
			;
			float[] lean = new float[10];
			float maxLean = 10;
			int blobClass = 0;
			if(h>5*w){
				blobClass = 1;
			} else {
				for(int j = 0; j < 10; j++){
					for(int i = 0; i < 7; i++){
						lean[j] += Mathf.Abs(classifier[j,i]-blobSegment[i]);
					}
					if(lean[j] < maxLean)
					{
						maxLean = lean[j];
						blobClass = j;
					}
				}
			}
			blobsClass[blobN] = blobClass;
			blobN++;
		}
		return blobsClass;
	}
	public static void Convolve(this Texture2D value, float[,] matrix){
		Color32[] colors = value.GetPixels32 ();
		Color32[] newColors = new Color32[colors.Length];
		int w = matrix.GetLength(1);
		int h = matrix.GetLength(0);
		for (int i = 0; i < colors.Length; i++) {

			Color32[,] neighbors = GetNeighbors(ref value, ref colors, i, w, h);
			float val = 0;
			for(int y = 0; y < h; y++){
				for(int x = 0; x < w; x++){
					val += (float)neighbors[y,x].r * matrix[y,x];
				}
			}
			val = Mathf.Abs(val);
			byte valByte = (byte)val;
			newColors[i] = new Color32(valByte, valByte, valByte, 255);
		}
		value.SetPixels32 (newColors);
		value.Apply ();
	}

	private static Color32[,] GetNeighbors(ref Texture2D value, ref Color32[] colors,int index, int w, int h){
		Color32[,] neighbors = new Color32[h,w];
		for (int j = 0; j < h; j++) {
			for (int i = 0; i < w; i++) {
				int xShift = i - w / 2;
				int yShift = j - h / 2;
				int x = index % value.width + xShift;
				int y = index / value.width + yShift;

				if (x >= 0 && x < value.width && y >= 0 && y < value.height) {
					int nIndex = index + (value.width * yShift) + xShift;
					neighbors [j,i] = colors [nIndex];
				} 
			}
		}
		return neighbors;
	}
}
