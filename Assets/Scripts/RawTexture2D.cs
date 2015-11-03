using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eppy;

public class RawTexture2D {
	public Color32[] pixels;
	public int width;
	public int height;
	public RawTexture2D(){
	}
	public RawTexture2D(int w, int h){
		width = w;
		height = h;
		pixels = new Color32[w * h];
	}
	public RawTexture2D(Texture2D texture){
		pixels = texture.GetPixels32();
		width = texture.width;
		height = texture.height;
	}
	public RawTexture2D(bool[,] matrix){
		width = matrix.GetLength (1);
		height = matrix.GetLength (0);
		pixels = new Color32[width * height];
		for (int j = 0; j < height; j++) {
			for(int i = 0; i < width; i++){
				pixels[j*width+i] = matrix[j,i] ? Color.black : Color.white;
			}
		}
	}
	public Texture2D ToTexture2D(){
		Texture2D texture = new Texture2D (width, height);
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = FilterMode.Point;
		texture.SetPixels32 (pixels);
		texture.Apply ();
		return texture;
	}
	public void Grayscale(){
		for (int i = 0; i < pixels.Length; i++) {
			byte grayscale = (byte)((pixels[i].r + pixels[i].g + pixels[i].b) / 3);
			pixels[i] = new Color32(grayscale, grayscale, grayscale, 255);
		}
	}
	public void Inverse(){
		for (int i = 0; i < pixels.Length; i++) {
			byte r = (byte)(255-pixels[i].r);
			byte g = (byte)(255-pixels[i].g);
			byte b = (byte)(255-pixels[i].b);
			pixels[i] = new Color32(r, g, b, 255);
		}
	}

	public Color32 GetPixel(int x, int y){
		if (x >= 0 && x < width && y >= 0 && y < height) {
			return pixels [width * y + x];
		} else {
			return Color.clear;
		}
	}

	public bool SetPixel(int x, int y, Color32 color){
		if (x >= 0 && x < width && y >= 0 && y < height) {
			pixels [width * y + x] = color;
			return true;
		}
		return false;
	}

	public Color32[,] GetNeighbors(int index, int w, int h){
		Color32[,] neighbors = new Color32[h,w];
		for (int j = 0; j < h; j++) {
			for (int i = 0; i < w; i++) {
				int xShift = i - w / 2;
				int yShift = j - h / 2;
				int x = index % width + xShift;
				int y = index / width + yShift;
				
				if (x >= 0 && x < width && y >= 0 && y < height) {
					int nIndex = index + (width * yShift) + xShift;
					neighbors [j,i] = pixels[nIndex];
				} else {
					neighbors [j,i] = Color.white;
				}
			}
		}
		return neighbors;
	}

	public int[,] GetNeighborsIndex(int index, int w, int h){
		int[,] neighbors = new int[h, w];
		for (int j = 0; j < h; j++) {
			for (int i = 0; i < w; i++) {
				int xShift = i - w / 2;
				int yShift = j - h / 2;
				int x = index % width + xShift;
				int y = index / width + yShift;
				
				if (x >= 0 && x < width && y >= 0 && y < height) {
					neighbors [j,i] = index + (width * yShift) + xShift;
				} else {
					neighbors [j,i] = -1;
				}
			}
		}
		return neighbors;
	}

	public void Scale(int scale){
		Color32[] oldPixels = pixels;
		pixels = new Color32[pixels.Length * scale * scale];
		int w = width;
		int h = height;
		width *= scale;
		height *= scale;
		for (int j = 0; j < h; j++){
			for(int i = 0; i < w; i++){
				for(int y = 0; y < scale; y++){
					for(int x = 0; x < scale; x++){
						SetPixel(i*scale+x, j*scale+y, oldPixels[j*w+i]);
					}
				}
			}
		}

	}

	public int[] VerticalProjection(){
		int[] projection = new int[height];
		for (int i = 0; i < pixels.Length; i++) {
			projection[i/width] += pixels[i].r;
		}
		return projection;
	}

	public int[] HorizontalProjection(){
		int[] projection = new int[width];
		for (int i = 0; i < pixels.Length; i++) {
			projection[i%width] += pixels[i].r;
		}
		return projection;
	}

	public void Convolve(float[,] matrix){
		Color32[] newPixels = new Color32[pixels.Length];
		int w = matrix.GetLength(1);
		int h = matrix.GetLength(0);
		for (int i = 0; i < pixels.Length; i++) {
			
			Color32[,] neighbors = GetNeighbors(i, w, h);
			float val = 0;
			for(int y = 0; y < h; y++){
				for(int x = 0; x < w; x++){
					val += (float)neighbors[y,x].r * matrix[y,x];
				}
			}
			val = Mathf.Clamp(val, 0, 255);
			byte valByte = (byte)val;
			newPixels[i] = new Color32(valByte, valByte, valByte, 255);
		}
		pixels = newPixels;
	}

	public void Sobel(){
		Color32[] newPixels = new Color32[pixels.Length];
		int[,] xMatrix = new int[,]{
			{-1,0,1},
			{-2,0,2},
			{-1,0,1}
		};
		int[,] yMatrix = new int[,]{
			{1,2,1},
			{0,0,0},
			{-1,-2,-1}
		};
		for (int i = 0; i < pixels.Length; i++) {
			
			Color32[,] neighbors = GetNeighbors(i, 3, 3);
			int g = 0;
			int gx = 0;
			int gy = 0;
			for(int y = 0; y < 3; y++){
				for(int x = 0; x < 3; x++){
					gx += (int)neighbors[y,x].r * xMatrix[y,x];
					gy += (int)neighbors[y,x].r * yMatrix[y,x];
				}
			}
			g = Mathf.Clamp((int)Mathf.Sqrt(gx*gx+gy*gy),0,255);
			byte valByte = (byte)g;
			newPixels[i] = new Color32(valByte, valByte, valByte, 255);
		}
		pixels = newPixels;
	}

	public int[] Histogram(int channel = 0){	
		int[] histogram = new int[256];
		if (channel == 0) {
			foreach (Color32 color in pixels) {
				int grayscale = (color.r + color.g + color.b) / 3;
				histogram[grayscale]++;
			}
		} else if (channel == 1) {
			foreach (Color32 color in pixels) histogram[color.r]++;
		} else if (channel == 2) {
			foreach (Color32 color in pixels) histogram[color.g]++;
		} else if (channel == 3) {
			foreach (Color32 color in pixels) histogram[color.b]++;
		}
		return histogram;
	}

	public List<int>[] HistogramPixel(){
		List<int>[] histogram = new List<int>[256];
		for (int i = 0; i < 256; i++) {
			histogram[i] = new List<int>();
		}
		int x = 0;
		foreach (Color32 color in pixels) {
			int grayscale = (color.r + color.g + color.b)/3;
			histogram[grayscale].Add(x);
			x++;
		}
		
		return histogram;
	}
	
	public int[] CumulativeHistogram(){
		int[] histogram = this.Histogram ();
		int[] cumulativeHistogram = new int[256];
		cumulativeHistogram [0] = histogram [0];
		for (int i = 1; i < 256; i++) {
			cumulativeHistogram[i] = cumulativeHistogram[i-1] + histogram[i];
		}
		
		return cumulativeHistogram;
	}
	
	public float[] CalculateCDF(){
		int[] cumulativeHistogram = this.CumulativeHistogram ();
		float[] cdf = new float[256];
		for (int i = 0; i<256; i++) {
			cdf[i] = (float)cumulativeHistogram[i]/(float)cumulativeHistogram[255];
		}
		
		return cdf;
	}

	public void Equalize(){
		List<int>[] histogram = HistogramPixel ();
		float[] cdf = CalculateCDF ();
		Color32[] newColors = new Color32[width*height];
		for (int i=0; i<256; i++) {
			foreach(int index in histogram[i]){
				byte g = (byte)(cdf[i]*255f);
				Color32 color = new Color32(g,g,g,255);
				newColors[index] = color;
			}
		}
		pixels = newColors;
	}

	public int OtsuThreshold(){
		int[] histogram = Histogram ();
		// Total number of pixels
		int total = pixels.Length;
		
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
		for (int i = 0; i < pixels.Length; i++) {
			byte grayscale = (byte)((pixels[i].r + pixels[i].g + pixels[i].b) / 3);
			byte color = 255;
			if(grayscale < threshold) color = 0;
			pixels[i] = new Color32(color, color, color, 255);
		}
		return threshold;
	}

	public void ZhangSuenThinning(){
		//value.OtsuThreshold ();
		int w = width;
		int h = height;
		int s = pixels.Length;
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
					p [0] = pixels [i].r != 255;
					p [1] = y - 1 < 0 ? false : pixels [i - w].r != 255;
					p [2] = y - 1 < 0 || x + 1 > w - 1 ? false : pixels [i - w + 1].r != 255;
					p [3] = x + 1 > w - 1 ? false : pixels [i + 1].r != 255;
					p [4] = y + 1 > h - 1 || x + 1 > w - 1 ? false : pixels [i + w + 1].r != 255;
					p [5] = y + 1 > h - 1 ? false : pixels [i + w].r != 255;
					p [6] = y + 1 > h - 1 || x - 1 < 0  ? false : pixels [i + w - 1].r != 255;
					p [7] = x - 1 < 0 ? false : pixels [i - 1].r != 255;
					p [8] = y - 1 < 0 || x - 1 < 0 ? false : pixels [i - w - 1].r != 255;
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
							pixels [i].b = 255;
							step1 = true;
						}
					} else if(j == 1){
						if (p [0] && np >= 2 && np <= 6 && sp == 1 && !(p [1] && p [3] && p [7]) && !(p [1] && p [5] && p [7])){
							pixels [i].b = 255;
							step2 = true;
						}
					}
				}
				for (int i = 0; i < s; i++) {
					pixels [i].r = pixels [i].b;
					pixels [i].g = pixels [i].b;
				}
			}
		}
	}
	public int[] GetChainCode4(){
		int p0 = -1;
		int pN;
		int pP = 4;
		for (int i = 0; i < pixels.Length; i++) {
			if(pixels[i].r < 255){
				p0 = i;
				break;
			}
		}
		pN = p0;
		List<int> chainCode = new List<int>();
		
		int breaker = 0;
		if (p0 != -1) {
			do {
				breaker++;
				int[,] neighborsIndex = GetNeighborsIndex (pN, 3, 3);
				int[] dirIndex = new int[8];
				dirIndex [0] = neighborsIndex [1, 2];
				dirIndex [1] = neighborsIndex [2, 2];
				dirIndex [2] = neighborsIndex [2, 1];
				dirIndex [3] = neighborsIndex [2, 0];
				dirIndex [4] = neighborsIndex [1, 0];
				dirIndex [5] = neighborsIndex [0, 0];
				dirIndex [6] = neighborsIndex [0, 1];
				dirIndex [7] = neighborsIndex [0, 2];
				for (int i = 0; i < 8; i++) {
					int temp0 = (i + pP) % 8;
					int temp1 = (temp0 + 1) % 8;
					byte color0 = dirIndex [temp0] == -1 ? (byte)255 : pixels [dirIndex [temp0]].r;
					byte color1 = dirIndex [temp1] == -1 ? (byte)255 : pixels [dirIndex [temp1]].r;
					if (color0 == 255 && color1 < 255) {
						pN = dirIndex [temp1];
						chainCode.Add (temp1);
						pP = (temp1 + 4)%8;
						break;
					}
				}
			} while (pN != p0 || breaker>10000);
		}
		Debug.Log ("break " + breaker);
		return chainCode.ToArray();
	}
	public int[] GetChainCode(){
		int p0 = -1;
		int pN;
		int pP = 4;
		for (int i = 0; i < pixels.Length; i++) {
			if(pixels[i].r < 255){
				p0 = i;
				break;
			}
		}
		pN = p0;
		List<int> chainCode = new List<int>();

		int breaker = 0;
		if (p0 != -1) {
			do {
				breaker++;
				int[,] neighborsIndex = GetNeighborsIndex (pN, 3, 3);
				int[] dirIndex = new int[8];
				dirIndex [0] = neighborsIndex [1, 2];
				dirIndex [1] = neighborsIndex [2, 2];
				dirIndex [2] = neighborsIndex [2, 1];
				dirIndex [3] = neighborsIndex [2, 0];
				dirIndex [4] = neighborsIndex [1, 0];
				dirIndex [5] = neighborsIndex [0, 0];
				dirIndex [6] = neighborsIndex [0, 1];
				dirIndex [7] = neighborsIndex [0, 2];
				for (int i = 0; i < 8; i++) {
					int temp0 = (i + pP) % 8;
					int temp1 = (temp0 + 1) % 8;
					byte color0 = dirIndex [temp0] == -1 ? (byte)255 : pixels [dirIndex [temp0]].r;
					byte color1 = dirIndex [temp1] == -1 ? (byte)255 : pixels [dirIndex [temp1]].r;
					if (color0 == 255 && color1 < 255) {
						pN = dirIndex [temp1];
						chainCode.Add (temp1);
						pP = (temp1 + 4)%8;
						break;
					}
				}
			} while (pN != p0 || breaker>10000);
		}
		Debug.Log ("break " + breaker);
		return chainCode.ToArray();
	}

	public List<RawTexture2D> BlobDetection(){
		int w = width;
		int h = height;
		int s = pixels.Length;
		bool[] p = new bool[5];
		int[] pL = new int[5]; 
		int[,] labels = new int[h, w];
		int[] linked = new int[w*h/4];
		int label = 1;
		for (int i = 0; i < s; i++) {
			int x = i%w;
			int y = i/w;
			
			p [0] = pixels [i].r != 255;
			pL [0] = labels[y,x];
			
			if(y - 1 < 0 || x - 1 < 0){
				p[1] = false;
				pL[1] = 0;
			} else {
				p[1] = pixels [i - w - 1].r != 255;
				pL[1] = linked[labels[y-1,x-1]];
			}
			if(y - 1 < 0){
				p[2] = false;
				pL[2] = 0;
			} else {
				p[2] = pixels [i - w].r != 255;
				pL[2] = linked[labels[y-1,x]];
			}
			if(y - 1 < 0 || x + 1 > w - 1){
				p[3] = false;
				pL[3] = 0;
			} else {
				p[3] = pixels [i - w + 1].r != 255;
				pL[3] = linked[labels[y-1,x+1]];
			}
			if(x - 1 < 0){
				p[4] = false;
				pL[4] = 0;
			} else {
				p[4] = pixels [i - 1].r != 255;
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
		/*
		for (int j = 0; j < h; j++) {
			for(int i = 0; i < w; i++){
				if(labels[j,i] != 0){
					pixels[j*w+i].r = (byte)((255 * labels[j,i] / maxLabel)%256);
					pixels[j*w+i].b = (byte)(255-pixels[j*w+i].r);
					pixels[j*w+i].g = (byte)((pixels[j*w+i].r+pixels[j*w+i].b)/2);
				}
			}
		}
		*/
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

		List<Tuple<int,int>> order = new List<Tuple<int, int>> ();
		for (int i = 0; i < blobRect.Length; i++) {
			order.Add(Tuple.Create(i+1, (int)blobRect[i].x));
		}
		order = order.OrderBy (i => i.Item2).ToList();
		blobRect = blobRect.OrderBy(i => i.x).ToArray();
		List<RawTexture2D> blobs = new List<RawTexture2D> ();
		for (int i = 0; i < maxLabel; i++) {

			int blobX = (int)blobRect[i].x;
			int blobY = (int)blobRect[i].y;
			int blobW = (int)(blobRect[i].width - blobRect[i].x + 1);
			int blobH = (int)(blobRect[i].height - blobRect[i].y + 1);

			if(blobW*blobH>9){
				RawTexture2D rawTexture = new RawTexture2D(blobW, blobH);
				for(int yy=0; yy < blobH; yy++){
					for(int xx=0; xx<blobW; xx++){
						rawTexture.pixels[yy*blobW+xx] = labels[blobY+yy, blobX+xx] == order[i].Item1 ? Color.black : Color.white;
					}
				}
				blobs.Add(rawTexture);
			}
		}
		return blobs;
	}
	/**
	public int[] Classify(){
		List<int[,]> blobs = BlobDetection ();
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
**/

	public int ColorCount(){
		int count = 0;;
		BitArray colors = new BitArray (256 * 256 * 256, false);
		foreach(Color32 pixel in pixels)
		{
			int index = pixel.r*65536+pixel.g*256+pixel.b;
			if(colors[index] == false)
			{
				count++;
				colors[index] = true;
			}
		}
		return count;
	}

	
	public Tuple<int,int>[] BandDetection(){
		int[] vProjection = VerticalProjection ();
		int[] vProjectionTemp = new int[vProjection.Length];
		List<Tuple<int,int>> peakList = new List<Tuple<int, int>> ();
		for (int i = 0; i < vProjection.Length; i++) {
			int newVal = 0;
			for(int j = 0; j<9; j++){
				int k = i-4+j;
				if(k >= 0 && k < vProjection.Length){
					newVal += (int)((float)vProjection[k]*0.111f);
				}
			}
			vProjectionTemp[i] = newVal;
		}
		vProjection = vProjectionTemp;
		for (int i = 1; i < vProjection.Length-1; i++) {
			if(vProjection[i] > vProjection[i-1] && vProjection[i] > vProjection[i+1]){
				peakList.Add(Tuple.Create(i, vProjection[i]));
			}
		}
		peakList = peakList.OrderByDescending (i => i.Item2).ToList();
		
		int bandNum = peakList.Count >= 3 ? 3 : peakList.Count;
		float c = 0.55f;
		Tuple<int,int>[] bandList = new Tuple<int,int>[bandNum];
		for (int i = 0; i < bandNum; i++) {
			int valley = (int)(c * (float)peakList[i].Item2);
			int v0 = 0;
			int v1 = height-1;
			for(int j = peakList[i].Item1-1; j>=0; j--){
				if(vProjection[j] <= valley){
					v0 = j;
					break;
				}
			}
			for(int j = peakList[i].Item1+1; j<vProjection.Length; j++){
				if(vProjection[j] <= valley){
					v1 = j;
					break;
				}
			}
			bandList[i] = Tuple.Create(v0, v1);
			//Debug.Log(bandList[i]);
		}
		/*
		for(int j = 0; j <bandNum; j++){
			for(int i = 0; i < width; i++){
				pixels[bandList[j].Item1*width+i].r = (byte)(80*(j+1));
			}
			for(int i = 0; i < width; i++){
				pixels[bandList[j].Item2*width+i].r = (byte)(80*(j+1));
			}
			for(int i = 0; i < width; i++){
				pixels[peakList[j].Item1*width+i].g = 255;
			}
		}
		*/
		return bandList;
	}
	
	public RawTexture2D BandClipping(Tuple<int,int> band){
		RawTexture2D bandTexture = new RawTexture2D();
		bandTexture = new RawTexture2D();
		int bandWidth = width;
		int bandHeight = band.Item2-band.Item1+1;
		int bandSize = bandWidth * bandHeight;
		bandTexture.pixels = new Color32[bandSize];
		bandTexture.width = bandWidth;
		bandTexture.height = bandHeight;
		
		for(int j = 0; j < bandSize; j++){
			bandTexture.pixels[j] = pixels[band.Item1*width+j];
		}
		return bandTexture;
	}
	
	public Tuple<int,int> PlateDetection(){
		int[] hProjection = HorizontalProjection ();
		int[] hProjectionTemp = new int[hProjection.Length];
		List<Tuple<int,int>> peakList = new List<Tuple<int, int>> ();
		int estimatedWidth = hProjection.Length / 3;
		for (int i = 0; i < hProjection.Length; i++) {
			int newVal = 0;
			for(int j = 0; j<estimatedWidth; j++){
				int k = i-estimatedWidth/2+j;
				if(k >= 0 && k < hProjection.Length){
					newVal += (int)((float)hProjection[k]*(1f/(float)estimatedWidth));
				}
			}
			hProjectionTemp[i] = newVal;
		}
		hProjection = hProjectionTemp;
		for (int i = 1; i < hProjection.Length-1; i++) {
			if(hProjection[i] > hProjection[i-1] && hProjection[i] > hProjection[i+1]){
				peakList.Add(Tuple.Create(i, hProjection[i]));
			}
		}
		peakList = peakList.OrderByDescending (i => i.Item2).ToList();
		
		int bandNum = 1;
		float c = 0.55f;
		Tuple<int,int>[] bandList = new Tuple<int,int>[bandNum];
		for (int i = 0; i < bandNum; i++) {
			int valley = (int)(c * (float)peakList[i].Item2);
			int v0 = 0;
			int v1 = width-1;
			for(int j = peakList[i].Item1-1; j>=0; j--){
				if(hProjection[j] <= valley){
					v0 = j;
					break;
				}
			}
			for(int j = peakList[i].Item1+1; j<hProjection.Length; j++){
				if(hProjection[j] <= valley){
					v1 = j;
					break;
				}
			}
			bandList[i] = Tuple.Create(v0, v1);
			//Debug.Log(bandList[i]);
		}
		//Debug.Log (width + "=+=" + height);
		return bandList[0];
	}
	
	public RawTexture2D PlateClipping(Tuple<int,int> plate){
		Debug.Log (plate.Item1 + " " + plate.Item2);
		RawTexture2D bandTexture = new RawTexture2D();
		int bandWidth = plate.Item2 - plate.Item1 +1;
		int bandHeight = height;
		int bandSize = bandWidth * bandHeight;
		bandTexture.pixels = new Color32[bandSize];
		bandTexture.width = bandWidth;
		bandTexture.height = bandHeight;
		//Debug.Log (bandTexture.width + "==" + bandTexture.height);
		for(int j = 0; j < bandSize; j++){
			bandTexture.pixels[j] = pixels[plate.Item1+(j%bandWidth)+(j/bandWidth)*width];
		}
		
		return bandTexture;
	}
	public RawTexture2D PlateClipping2nd(){
		int[] px = HorizontalProjection ();
		int[] px2 = new int[px.Length];
		
		int h = 4;
		for (int i = h; i < px.Length; i++) {
			px2[i] = (px[i]-px[i-h])/h;
			//Debug.Log("-> "+px2[i]);
		}
		int max = px2.Max ();
		int min = px2.Min ();
		int p0 = 0;
		int p1 = width-1;
		float c = 0.1f;
		for (int i = 0; i < px2.Length/2; i++) {
			if(px2[i]<c*(float)min){
				p0 = i;
				break;
			}
		}
		for (int i =px2.Length-1; i>=px2.Length/2; i--) {
			if(px2[i]>c*(float)max){
				p1 = i;
				break;
			}
		}
		int plateWidth = p1 - p0 +1;
		int plateHeight = height;
		int plateSize = plateWidth * plateHeight;
		RawTexture2D plateTexture = new RawTexture2D ();
		plateTexture.width = plateWidth;
		plateTexture.height = plateHeight;
		plateTexture.pixels = new Color32[plateSize];
		//Debug.Log ("=>" + p0 + " " + p1); 
		for(int j = 0; j < plateSize; j++){
			plateTexture.pixels[j] = pixels[p0+(j%plateWidth)+(j/plateWidth)*width];
		}
		return plateTexture;
	}


}