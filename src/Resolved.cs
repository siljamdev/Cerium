using System;
using System.Text;

class Resolved{
	public int width {get; init;}
	public int height {get; init;}
	
	public string title {get; init;}
	
	public int slideNum {get; init;}
	public float[] slideDuration {get; init;}
	
	public string[] imagePaths {get; init;}
	
	public Transition startTransition {get; init;}
	public float startTransitionDuration {get; init;}
	
	public Transition endTransition {get; init;}
	public float endTransitionDuration {get; init;}
	
	public Color3 fillColor {get; init;}
	
	public Transition[] slideTransitions {get; init;}
	public float[] slideTransitionsDuration {get; init;}
	
	public string audioPath {get; init;}
	
	public ImageFilter[] imageFilter {get; init;}
	public ImageScaling[] imageScaling {get; init;}
	
	//NOT ACCURATE
	public float totalDuration => slideDuration.Sum() + slideTransitionsDuration.Sum();
	
	public string getFfmpegArgs(){
        if(imagePaths.Length != slideDuration.Length || imagePaths.Length != slideNum){
			throw new Exception("Number of slides and durations must match");
		}
		
		if(slideTransitions.Length != slideTransitionsDuration.Length || slideTransitions.Length != slideNum - 1){
			throw new Exception("Number of slides and slide transitions must match");
		}
		
		StringBuilder inputArgs = new();
		StringBuilder filterComplex = new();
		
		if(audioPath != null){
			inputArgs.Append("-i \"" + audioPath + "\" ");
		}
		
		bool b = audioPath != null;
		
		for(int i = 0; i < slideNum; i++){
			string path = imagePaths[i].Replace("\\", "/"); //Ffmpeg works better or smth
			inputArgs.Append("-loop 1 -t " + slideDuration[i] + " -i \"" + path + "\" ");
		
			// Pad image to fixed resolution without scaling
			filterComplex.Append(
				"[" + (b ? (i + 1) : i) + ":v]" +
				toImageFilter(imageFilter[i]) +
				"scale=w='if(gt(a," + width + "/" + height + ")," + width + ",-1)':h='if(gt(a," + width + "/" + height + "),-1," + height + ")':flags=" + toScalingName(imageScaling[i]) + "," +
				"pad=" + width + ":" + height + ":(" + width + "-iw)/2:(" + height + "-ih)/2:color=" + fillColor + ",setsar=1,format=yuv420p[v" + i + "];"
			);
		}
		
		filterComplex.Append("[v0]split=2[v0b][v0c];");
		
		for(int i = 1; i < slideNum - 1; i++){
			filterComplex.Append("[v" + i + "]split=3[v" + i + "a][v" + i + "b][v" + i + "c];");
		}
		
		filterComplex.Append("[v" + (slideNum - 1) + "]split=2[v" + (slideNum - 1) + "a][v" + (slideNum - 1) + "c];");
		
		generateTransitions(filterComplex);
		
		//Input streams
		for(int i = 0; i < slideNum - 1; i++){
			filterComplex.Append("[v" + i + "c]");
			filterComplex.Append("[t" + i + (i + 1) + "]");
		}
		filterComplex.Append("[v" + (slideNum - 1) + "c]");
		
		filterComplex.Append("concat=n=" + (slideNum * 2 - 1) + ":v=1:a=0[pre];"); //Concat all image streams
		
		addInOutTransitions(filterComplex);
		
		if(audioPath != null){
			filterComplex.Append("[0:a]aresample=async=1,apad=whole_dur=" + (totalDuration + 100) + "[aout];");
		}
		
		//Because it gets too long
		string tempFile = Path.GetTempFileName();
		
		File.WriteAllText(tempFile, filterComplex.ToString());
		
		string args = inputArgs + " -strict experimental -filter_complex_script \"" + tempFile + "\" -map [out] -map [aout] -shortest -color_range pc -pix_fmt yuv420p \"" + title + ".mp4\"";
		
		return args;
    }
	
	void generateTransitions(StringBuilder filterComplex){
		for(int i = 0; i < slideNum - 1; i++){
			if(slideTransitionsDuration[i] > slideDuration[i]){
				filterComplex.Append("[v" + i + "b]tpad=start_mode=clone:start_duration=" + (slideTransitionsDuration[i] - slideDuration[i]) + "[tv" + i + "];");
			}else{
				filterComplex.Append("[v" + i + "b]reverse,trim=start=0:end=" + slideTransitionsDuration[i] + ",reverse[tv" + i + "];");
			}
			
			if(slideTransitionsDuration[i] > slideDuration[i + 1]){
				filterComplex.Append("[v" + (i + 1) + "a]tpad=start_mode=clone:start_duration=" + (slideTransitionsDuration[i] - slideDuration[i + 1]) + "[ev" + (i + 1) + "];");
			}else{
				filterComplex.Append("[v" + (i + 1) + "a]trim=start=0:end=" + slideTransitionsDuration[i] + "[ev" + (i + 1) + "];");
			}
			
			if(slideTransitions[i] == Transition.None){
				filterComplex.Append("[tv" + i + "][ev" + (i + 1) + "]xfade=transition=fade:duration=0:offset=" + (slideTransitionsDuration[i] / 2) + "[t" + i + (i + 1) + "];");
			}else{
				filterComplex.Append("[tv" + i + "][ev" + (i + 1) + "]xfade=transition=" + toXfadeName(slideTransitions[i]) + ":duration=" + slideTransitionsDuration[i] + ":offset=0[t" + i + (i + 1) + "];");
			}
		}
	}
	
	void addInOutTransitions(StringBuilder filterComplex){
		
		//Add needed white stream
		if(startTransition == Transition.White || endTransition == Transition.White){
			filterComplex.Append("color=white:size=" + width + "x" + height + ":d=" + Math.Max(startTransitionDuration, endTransitionDuration) + "[whitebg];");
		}
		
		//Add needed black stream
		if(startTransition == Transition.Black || startTransition == Transition.Fade){
			filterComplex.Append("color=black:size=" + width + "x" + height + ":d=" + Math.Max(startTransitionDuration, endTransitionDuration) + "[blackbg];");
		}
		
		if(startTransition == Transition.White && endTransition == Transition.White){
			filterComplex.Append("[pre]format=rgba,fade=t=in:st=0:d=" + startTransitionDuration + ":alpha=1,fade=t=out:st=" + (totalDuration - endTransitionDuration) + ":d=" + endTransitionDuration + ":alpha=1[pre2];");
			filterComplex.Append("[whitebg][pre2]overlay[out];");
		}else{
			switch(startTransition){
				case Transition.None:
					filterComplex.Append("[pre]null[pre_in];");
					break;
				
				case Transition.Black:
				case Transition.Fade:
					filterComplex.Append("[pre]format=rgba,fade=t=in:st=0:d=" + startTransitionDuration + ":alpha=1[pre05];");
					filterComplex.Append("[blackbg][pre05]overlay[pre_in];");
					break;
				
				case Transition.White:
					filterComplex.Append("[pre]format=rgba,fade=t=in:st=0:d=" + startTransitionDuration + ":alpha=1[pre05];");
					filterComplex.Append("[whitebg][pre05]overlay[pre_in];");
					break;
			}
			
			switch(endTransition){
				case Transition.None:
					filterComplex.Append("[pre_in]null[out];");
					break;
				
				case Transition.Black:
				case Transition.Fade:
					filterComplex.Append("[pre_in]reverse,fade=t=in:st=0:d=" + endTransitionDuration + ",reverse[out];");
					break;
				
				case Transition.White:
					filterComplex.Append("[pre_in]reverse,format=rgba,fade=t=in:st=0:d=" + endTransitionDuration + ":alpha=1,reverse[pre_out];");
					filterComplex.Append("[whitebg][pre_out]overlay[out];");
					break;
			}
		}
	}
	
	static string toImageFilter(ImageFilter f){
		return f switch{
			ImageFilter.None => "",
			ImageFilter.GrayScale => "hue=s=0,",
			ImageFilter.Pixelize => "scale=w=256:h='256 * (ih/iw)':flags=neighbor,",
			_ => ""
		};
	}
	
	static string toXfadeName(Transition t){
		return t switch{
			Transition.Fade => "fade",
			Transition.Black => "fadeblack",
			Transition.White => "fadewhite",
			_ => ""
		};
	}
	
	static string toScalingName(ImageScaling s){
		return s switch{
			ImageScaling.Neighbor => "neighbor",
			ImageScaling.Bilinear => "bilinear",
			ImageScaling.Area => "area",
			ImageScaling.Bicubic => "bicubic",
			ImageScaling.Spline => "spline",
			ImageScaling.Lanczos => "lanczos",
			ImageScaling.FastBilinear => "fast_bilinear",
			ImageScaling.Gauss => "gauss",
			_ => ""
		};
	}
}