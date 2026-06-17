using System;
using System.Text;

class Resolved{
	public int width {get; init;}
	public int height {get; init;}
	
	public string title {get; init;}
	
	public int fps {get; init;}
	
	public int slideNum {get; init;}
	public float[] slideDuration {get; init;}
	
	public string[] imagePaths {get; init;}
	
	public VideoTransition startTransition {get; init;}
	public float startTransitionDuration {get; init;}
	
	public VideoTransition endTransition {get; init;}
	public float endTransitionDuration {get; init;}
	
	public Color3 fillColor {get; init;}
	
	public SlideTransition[] slideTransitions {get; init;}
	public float[] slideTransitionsDuration {get; init;}
	
	//public SlideMotion[] slideMotions {get; init;}
	
	public string audioPath {get; init;}
	
	public ImageFilter[] imageFilter {get; init;}
	public ImageScaling[] imageScaling {get; init;}
	
	public ImageFilter videoFilter {get; init;}
	public AudioFilter audioFilter {get; init;}
	
	//NOT ACCURATE
	public float totalDuration => slideDuration.Sum() + slideTransitionsDuration.Sum();
	
	string tempFile;
	
	public string getFfmpegArgs(){
        if(imagePaths.Length != slideDuration.Length || imagePaths.Length != slideNum){
			throw new Exception("Number of slides and durations must match");
		}
		
		if(slideTransitions.Length != slideTransitionsDuration.Length || slideTransitions.Length != slideNum - 1){
			throw new Exception("Number of slides and slide transitions must match");
		}
		
		StringBuilder inputArgs = new();
		StringBuilder filterComplex = new();
		
		bool hasAudio = audioPath != null;
		
		if(hasAudio){
			inputArgs.Append("-i \"" + audioPath + "\" ");
		}
		
		//Images
		for(int i = 0; i < slideNum; i++){
			string path = imagePaths[i].Replace("\\", "/"); //Ffmpeg works better or smth
			inputArgs.Append("-i \"" + path + "\" ");
		
			// Pad image to fixed resolution without scaling
			filterComplex.Append(
				"[" + (hasAudio ? (i + 1) : i) + ":v]" +
				(imageFilter[i] != ImageFilter.None ? toImageFilter(imageFilter[i]) + "," : "") +
				"scale=w=" + width + ":h=" + height + ":force_original_aspect_ratio=decrease:flags=" + toScalingName(imageScaling[i]) + "," + //"scale=w='if(gt(a," + width + "/" + height + ")," + width + ",-1)':h='if(gt(a," + width + "/" + height + "),-1," + height + ")':flags=" + toScalingName(imageScaling[i]) + "," +
				"pad=" + width + ":" + height + ":(" + width + "-iw)/2:(" + height + "-ih)/2:color=" + fillColor + "," +
				"setsar=1" +
				"[i" + i + "];"
			);
		}
		
		//Splitting
		if(slideTransitions[0] == SlideTransition.None){ //b not needed
			filterComplex.Append("[i0]null[i0_c];");
		}else{
			filterComplex.Append("[i0]split=2[i0_b][i0_c];");
		}
		
		//Splitting
		for(int i = 1; i < slideNum - 1; i++){
			if(slideTransitions[i] == SlideTransition.None){ //b not needed
				if(slideTransitions[i - 1] == SlideTransition.None){ //a not needed
					filterComplex.Append("[i" + i + "]null[i" + i + "_c];");
				}else{
					filterComplex.Append("[i" + i + "]split=2[i" + i + "_a][i" + i + "_c];");
				}
			}else{
				if(slideTransitions[i - 1] == SlideTransition.None){ //a not needed
					filterComplex.Append("[i" + i + "]split=2[i" + i + "_b][i" + i + "_c];");
				}else{
					filterComplex.Append("[i" + i + "]split=3[i" + i + "_a][i" + i + "_b][i" + i + "_c];");
				}
			}
		}
		
		//Splitting
		if(slideTransitions[slideNum - 2] == SlideTransition.None){ //a not needed
			filterComplex.Append("[i" + (slideNum - 1) + "]null[i" + (slideNum - 1) + "_c];");
		}else{
			filterComplex.Append("[i" + (slideNum - 1) + "]split=2[i" + (slideNum - 1) + "_a][i" + (slideNum - 1) + "_c];");
		}
		
		//Slide transitions
		generateTransitions(filterComplex);
		
		//Slide -> video (+motion)
		for(int i = 0; i < slideNum; i++){
			//filterComplex.Append("[v" + i + "c]" + toMotion(slideMotions[i]) + "[v" + i + "d];");
			filterComplex.Append("[i" + i + "_c]loop=loop=-1:size=1:start=0,trim=duration=" + slideDuration[i] + ",format=yuv420p[v" + i + "];");
			//filterComplex.Append("[i" + i + "_c]loop=loop=" + ((int) (slideDuration[i] * fps)) + ":size=1:start=0,format=yuv420p[v" + i + "];");
		}
		
		//Combining
		int y = slideNum;
		for(int i = 0; i < slideNum - 1; i++){
			filterComplex.Append("[v" + i + "]");
			if(slideTransitions[i] != SlideTransition.None){
				filterComplex.Append("[t" + i + "]");
				y++;
			}
		}
		filterComplex.Append("[v" + (slideNum - 1) + "]");
		
		filterComplex.Append("concat=n=" + y + ":v=1:a=0[pre];"); //Concat all image streams
		
		//In out
		addInOutTransitions(filterComplex);
		
		//Video filter
		if(videoFilter == ImageFilter.None){
			filterComplex.Append("[outf]fps=" + fps + "[out];");
		}else{
			filterComplex.Append("[outf]" + (videoFilter != ImageFilter.None ? (toImageFilter(videoFilter) + ",") : "") + "fps=" + fps + "[out];");
		}
		
		//Audio
		if(hasAudio){
			filterComplex.Append("[0:a]aresample=async=1,apad=whole_dur=" + (totalDuration + 100) + (audioFilter == AudioFilter.None ? "" : ("," + toAudioFilter(audioFilter))) + "[aout];");
		}
		
		//Because it gets too long
		tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, filterComplex.ToString());
		
		string args = inputArgs + " -hide_banner -strict experimental -filter_complex_script \"" + tempFile + "\" -map [out] " + (hasAudio ? " -map [aout]" : "") + " -shortest -color_range pc -pix_fmt yuv420p -r " + fps + " \"" + title + ".mp4\"";
		
		Console.WriteLine(args);
		Console.WriteLine();
		Console.WriteLine(filterComplex.ToString());
		Console.WriteLine();
		Console.WriteLine("Expected duration: " + totalDuration);
		Console.WriteLine("\n\n\n");
		
		return args;
    }
	
	public void cleanup(){
		if(File.Exists(tempFile)){
			File.Delete(tempFile);
		}
	}
	
	void generateTransitions(StringBuilder filterComplex){
		//For slide i and i + 1
		for(int i = 0; i < slideNum - 1; i++){
			if(slideTransitions[i] != SlideTransition.None){
				//Generate videos
				filterComplex.Append("[i" + i + "_b]loop=loop=-1:size=1:start=0,trim=duration=" + slideTransitionsDuration[i] + ",format=yuv420p[t" + i + "_1];");
				filterComplex.Append("[i" + (i + 1) + "_a]loop=loop=-1:size=1:start=0,trim=duration=" + slideTransitionsDuration[i] + ",format=yuv420p[t" + i + "_2];");
				
				filterComplex.Append("[t" + i + "_1][t" + i + "_2]xfade=transition=" + toXfadeName(slideTransitions[i]) + ":duration=" + slideTransitionsDuration[i] + ":offset=0[t" + i + "];");
			}
		}
	}
	
	void addInOutTransitions(StringBuilder filterComplex){
		
		//Add needed white stream
		if(startTransition == VideoTransition.White || endTransition == VideoTransition.White){
			filterComplex.Append("color=white:size=" + width + "x" + height + ":d=" + Math.Max(startTransitionDuration, endTransitionDuration) + "[whitebg];");
		}
		
		if(startTransition == VideoTransition.White && endTransition == VideoTransition.White){
			filterComplex.Append("[pre]format=rgba,fade=t=in:st=0:d=" + startTransitionDuration + ":alpha=1,fade=t=out:st=" + (totalDuration - endTransitionDuration) + ":d=" + endTransitionDuration + ":alpha=1[pre_in];");
			filterComplex.Append("[whitebg][pre_in]overlay[outf];");
		}else{
			switch(startTransition){
				case VideoTransition.None:
					filterComplex.Append("[pre]null[pre_in];");
					break;
				
				case VideoTransition.Black:
					//Add needed black stream
					filterComplex.Append("color=black:size=" + width + "x" + height + ":d=" + Math.Max(startTransitionDuration, endTransitionDuration) + "[blackbg];");
					
					filterComplex.Append("[pre]format=rgba,fade=t=in:st=0:d=" + startTransitionDuration + ":alpha=1[pre_m];");
					filterComplex.Append("[blackbg][pre_m]overlay[pre_in];");
					break;
				
				case VideoTransition.White:
					filterComplex.Append("[pre]format=rgba,fade=t=in:st=0:d=" + startTransitionDuration + ":alpha=1[pre_m];");
					filterComplex.Append("[whitebg][pre_m]overlay[pre_in];");
					break;
			}
			
			switch(endTransition){
				case VideoTransition.None:
					filterComplex.Append("[pre_in]null[outf];");
					break;
				
				case VideoTransition.Black:
					filterComplex.Append("[pre_in]reverse,fade=t=in:st=0:d=" + endTransitionDuration + ",reverse[outf];");
					break;
				
				case VideoTransition.White:
					filterComplex.Append("[pre_in]reverse,format=rgba,fade=t=in:st=0:d=" + endTransitionDuration + ":alpha=1,reverse[pre_out];");
					filterComplex.Append("[whitebg][pre_out]overlay[outf];");
					break;
			}
		}
	}
	
	static string toImageFilter(ImageFilter f){
		return f switch{
			ImageFilter.None => "",
			ImageFilter.GrayScale => "hue=s=0",
			
			ImageFilter.Pixelize16 => "scale=w=16:h=16' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize32 => "scale=w=32:h=32' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize64 => "scale=w=64:h=64' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize128 => "scale=w=128:h=128' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize256 => "scale=w=256:h=256' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize512 => "scale=w=512:h=512' * (ih/iw)':flags=neighbor",
			
			ImageFilter.Pixelize16GrayScale => "hue=s=0,scale=w=16:h=16' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize32GrayScale => "hue=s=0,scale=w=32:h=32' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize64GrayScale => "hue=s=0,scale=w=64:h=64' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize128GrayScale => "hue=s=0,scale=w=128:h=128' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize256GrayScale => "hue=s=0,scale=w=256:h=256' * (ih/iw)':flags=neighbor",
			ImageFilter.Pixelize512GrayScale => "hue=s=0,scale=w=512:h=512' * (ih/iw)':flags=neighbor",
			ImageFilter.Sepia => "colorchannelmixer=.393:.769:.189:0:.349:.686:.168:0:.272:.534:.131",
			
			ImageFilter.Invert => "negate",
			ImageFilter.Warm => "colorbalance=rs=0.2:gs=0.1:bs=-0.1",
			ImageFilter.Cool => "colorbalance=rs=-0.1:gs=0.1:bs=0.3",
			
			ImageFilter.Blur => "gblur=sigma=5",
			ImageFilter.BlurStrong => "boxblur=12:1",
			ImageFilter.BlurSubtle => "gblur=sigma=2",
			ImageFilter.BlurGrayScale => "hue=s=0,gblur=sigma=5",
			ImageFilter.BlurStrongGrayScale => "hue=s=0,boxblur=12:1",
			ImageFilter.BlurSubtleGrayScale => "hue=s=0,gblur=sigma=2",
			
			ImageFilter.Sharp => "unsharp=9:9:2.0",
			ImageFilter.SharpGrayScale => "hue=s=0,unsharp=9:9:2.0",
			
			ImageFilter.Edge => "edgedetect=low=0.1:high=0.3",
			ImageFilter.EdgeInvert => "edgedetect=low=0.1:high=0.3,negate",
			
			ImageFilter.Posterize => "format=rgb24,lutrgb=r='trunc(val/16)*16':g='trunc(val/16)*16':b='trunc(val/16)*16'",
			ImageFilter.PosterizeStrong => "format=rgb24,lutrgb=r='trunc(val/64)*64':g='trunc(val/64)*64':b='trunc(val/64)*64'",
			ImageFilter.PosterizeGrayScale => "hue=s=0,format=rgb24,lutyuv=y='trunc(val/8)*8'",
			ImageFilter.PosterizeStrongGrayScale => "hue=s=0,format=rgb24,lutyuv=y='trunc(val/64)*64'",
			
			ImageFilter.Glitch => "rgbashift=rh=2:gh=-2:bh=1",
			ImageFilter.Noise => "noise=c0s=20:c0f=t",
			ImageFilter.NoiseColor => "noise=alls=20:allf=t",
			ImageFilter.Vibrant => "eq=saturation=2",
			ImageFilter.Vhs => "gblur=sigma=1.5,convolution=\"-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2:-2 -1 0 -1 1 1 0 1 2\",noise=alls=8:allf=t,eq=saturation=1.2:contrast=1.1,format=yuv420p",
			_ => ""
		};
	}
	
	static string toXfadeName(SlideTransition t){
		return t switch{
			SlideTransition.Fade => "fade",
			SlideTransition.Black => "fadeblack",
			SlideTransition.White => "fadewhite",
			SlideTransition.SlideDown => "slidedown",
			SlideTransition.SlideUp => "slideup",
			SlideTransition.SlideLeft => "slideleft",
			SlideTransition.SlideRight => "slideright",
			SlideTransition.WipeDown => "wipedown",
			SlideTransition.WipeUp => "wipeup",
			SlideTransition.WipeLeft => "wipeleft",
			SlideTransition.WipeRight => "wiperight",
			SlideTransition.ZoomIn => "zoomin",
			SlideTransition.ZoomOut => "zoomout",
			SlideTransition.Distance => "distance",
			SlideTransition.Burn => "fadegrays",
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
			ImageScaling.Bicublin => "bicublin",
			ImageScaling.Sinc => "sinc",
			_ => ""
		};
	}
	
	static string toAudioFilter(AudioFilter f){
		return f switch{
			AudioFilter.BassBoost => "bass=g=10",
			AudioFilter.Tremble => "treble=g=8",
			AudioFilter.Echo => "aecho=0.8:0.9:1000:0.3",
			AudioFilter.Reverb => "aecho=0.8:0.88:60:0.4",
			AudioFilter.SoftClip => "asoftclip",
			AudioFilter.BitCrush => "acrusher=bits=6:samples=8",
			AudioFilter.Radio => "highpass=f=300,lowpass=f=3000",
			AudioFilter.Vhs => "acrusher=bits=8:mix=0.8,highpass=f=100,lowpass=f=4000",
			AudioFilter.Vinyl => "highpass=f=100,lowpass=f=5000,acrusher=bits=8,aecho=0.8:0.9:40:0.2",
			AudioFilter.UnderWater => "lowpass=f=500,aecho=0.8:0.9:100|180:0.3|0.25",
			AudioFilter.Dreamy => "aecho=0.8:0.9:500:0.3",
			AudioFilter.LowQuality => "acrusher=bits=4:samples=6:mode=log",
			_ => ""
		};
	}
	
	string toMotion(SlideMotion m){
		return m switch{
			SlideMotion.ZoomIn => "zoompan=z='1+0.002*on':x='(iw-iw/zoom)/2':y='(ih-ih/zoom)/2':s=" + width + "x" + height + ":d=1",
			SlideMotion.ZoomOut => "zoompan=z='1.5-0.002*on':x='(iw-iw/zoom)/2':y='(ih-ih/zoom)/2':s=" + width + "x" + height + ":d=1",
			//SlideMotion.Pad => "scale=" + width + ":" + (height + 100) + ",crop=" + width + ":" + height + ":x=0:y='max(0\\,100-t*20)',setsar=1",
			_ => "null"
		};
	}
}