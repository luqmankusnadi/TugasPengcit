using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RawTexture2D {
	public Color32[] pixels;
	public int width;
	public int height;
	public RawTexture2D(Texture2D texture){
		pixels = texture.GetPixels32();
		width = texture.width;
		height = texture.height;
	}
	public Texture2D ToTexture2D(){
		Texture2D texture = new Texture2D (width, height);
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
				} 
			}
		}
		return neighbors;
	}

	public int[] VerticalProjection(){
		int[] projection = new int[height];
		for (int i = 0; i < pixels.Length; i++) {
			projection[i/width] += pixels[i].r;
		}
		return projection;
	}

	public void BandDetection(){
		int[] vProjection = VerticalProjection ();
		int peak = 0;
		int ybm = 0;
		for (int i = 0; i < vProjection.Length; i++) {
			int newVal = 0;
			for(int j = 0; j<9; j++){
				int k = i-4+j;
				if(k >= 0 && k < vProjection.Length){
					newVal += (int)((float)vProjection[k]*0.111f);
				}
			}
		}
		for (int i = 0; i < vProjection.Length; i++) {
			for(int j = 0; j<3; j++){
				int k = i-1+j;
			}
		}
		for (int i = 0; i < vProjection.Length; i++) {
			if(vProjection[i] > peak){ 
				peak = vProjection[i];
				ybm = i;
			}
		}
		for(int i = 0; i < width; i++){
			pixels[ybm*width+i].r = 255;
		}
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
}