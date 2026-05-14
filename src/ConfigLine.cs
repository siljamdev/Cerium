using System.Text;

class ConfigLine{
	public readonly string keyword;
	public readonly string[] values;
	
	public int valueNum => values.Length;
	
	public ConfigLine(string line){
		
		string[] parts = line.Split(":");
		
		if(parts.Length < 1){
			keyword = "";
			values = new string[0];
			return;
		}
		
		keyword = parts[0].Trim().ToLower();
		
		string[] sp = splitLine(string.Join(":", parts.Skip(1)));
		
		values = sp.ToArray();
	}
	
	public bool tryValueAt(int n){
		return n >= 0 && n < valueNum;
	}
	
	public string getValAt(int n){
		if(!tryValueAt(n)){
			return "";
		}
		
		return values[n];
	}
	
	public float getNumAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected floating point number at position " + n);
		}
		
		if(!float.TryParse(values[n], out float f)){
			throw new Exception("Expected floating point number, found: " + values[n]);
		}
		
		return f;
	}
	
	public float getUnumAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected positive floating point number at position " + n);
		}
		
		if(!float.TryParse(values[n], out float f) || f < 0f){
			throw new Exception("Expected positive floating point number, found: " + values[n]);
		}
		
		return f;
	}
	
	public int getIntAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected integer number at position " + n);
		}
		
		if(!int.TryParse(values[n], out int f)){
			throw new Exception("Expected integer number, found: " + values[n]);
		}
		
		return f;
	}
	
	public int getUintAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected positive integer number at position " + n);
		}
		
		if(!uint.TryParse(values[n], out uint f)){
			throw new Exception("Expected positive integer number, found: " + values[n]);
		}
		
		return (int) f;
	}
	
	public Priority getPriorityAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected priority at position " + n);
		}
		
		return values[n].ToLower() switch{
			"ignore" => Priority.Ignore,
			"override" => Priority.Override,
			"merge" => Priority.Merge,
			"mergeReverse" => Priority.MergeReverse,
			_ => throw new Exception("Expected priority name, found: " + values[n])
		};
	}
	
	public VideoTransition getVideoTransitionAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected video transition effect at position " + n);
		}
		
		return values[n].ToLower() switch{
			"none" => VideoTransition.None,
			"black" => VideoTransition.Black,
			"white" => VideoTransition.White,
			_ => throw new Exception("Expected video transition effect, found: " + values[n])
		};
	}
	
	public SlideTransition getSlideTransitionAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected slide transition effect at position " + n);
		}
		
		return values[n].ToLower() switch{
			"none" => SlideTransition.None,
			"fade" => SlideTransition.Fade,
			"black" => SlideTransition.Black,
			"white" => SlideTransition.White,
			"slidedown" => SlideTransition.SlideDown,
			"slideup" => SlideTransition.SlideUp,
			"slideleft" => SlideTransition.SlideLeft,
			"slideright" => SlideTransition.SlideRight,
			"sliderandom" => SlideTransition.SlideRandom,
			"wipedown" => SlideTransition.WipeDown,
			"wipeup" => SlideTransition.WipeUp,
			"wipeleft" => SlideTransition.WipeLeft,
			"wiperight" => SlideTransition.WipeRight,
			"wiperandom" => SlideTransition.WipeRandom,
			"zoomin" => SlideTransition.ZoomIn,
			"zoomout" => SlideTransition.ZoomOut,
			"distance" => SlideTransition.Distance,
			"burn" => SlideTransition.Burn,
			_ => throw new Exception("Expected slide transition effect, found: " + values[n])
		};
	}
	
	public SlideMotion getSlideMotionAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected slide motion effect at position " + n);
		}
		
		return values[n].ToLower() switch{
			"none" => SlideMotion.None,
			"zoomin" => SlideMotion.ZoomIn,
			"zoomout" => SlideMotion.ZoomOut,
			_ => throw new Exception("Expected slide motion effect, found: " + values[n])
		};
	}
	
	public Color3 getColorAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected color at position " + n);
		}
		
		return values[n].ToLower() switch{
			"white" => new Color3(255, 255, 255),
			"black" => new Color3(0, 0, 0),
			_ => Color3.Parse(values[n])
		};
	}
	
	public ImSelectionMode getImSelModeAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected image selection mode at position " + n);
		}
		
		return values[n].ToLower() switch{
			"random" => ImSelectionMode.Random,
			"unique" => ImSelectionMode.Unique,
			"order" => ImSelectionMode.Order,
			_ => throw new Exception("Expected image selection mode, found: " + values[n])
		};
	}
	
	public ImageFilter getImFilterAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected image filter at position " + n);
		}
		
		return values[n].ToLower() switch{
			"none" => ImageFilter.None,
			"grayscale" => ImageFilter.GrayScale,
			
			"pixelize16" => ImageFilter.Pixelize16,
			"pixelize32" => ImageFilter.Pixelize32,
			"pixelize64" => ImageFilter.Pixelize64,
			"pixelize128" => ImageFilter.Pixelize128,
			"pixelize256" => ImageFilter.Pixelize256,
			"pixelize512" => ImageFilter.Pixelize512,
			
			"pixelize16grayscale" => ImageFilter.Pixelize16GrayScale,
			"pixelize32grayscale" => ImageFilter.Pixelize32GrayScale,
			"pixelize64grayscale" => ImageFilter.Pixelize64GrayScale,
			"pixelize128grayscale" => ImageFilter.Pixelize128GrayScale,
			"pixelize256grayscale" => ImageFilter.Pixelize256GrayScale,
			"pixelize512grayscale" => ImageFilter.Pixelize512GrayScale,
			
			"sepia" => ImageFilter.Sepia,
			"invert" => ImageFilter.Invert,
			"warm" => ImageFilter.Warm,
			"cool" => ImageFilter.Cool,
			
			"blur" => ImageFilter.Blur,
			"blurstrong" => ImageFilter.BlurStrong,
			"blursubtle" => ImageFilter.BlurSubtle,
			"blurgrayscale" => ImageFilter.BlurGrayScale,
			"blurstronggrayscale" => ImageFilter.BlurStrongGrayScale,
			"blursubtlegrayscale" => ImageFilter.BlurSubtleGrayScale,
			
			"sharp" => ImageFilter.Sharp,
			"sharpgrayscale" => ImageFilter.SharpGrayScale,
			
			"edge" => ImageFilter.Edge,
			"edgeinvert" => ImageFilter.EdgeInvert,
			
			"posterize" => ImageFilter.Posterize,
			"posterizestrong" => ImageFilter.PosterizeStrong,
			"posterizegrayscale" => ImageFilter.PosterizeGrayScale,
			"posterizestronggrayscale" => ImageFilter.PosterizeStrongGrayScale,
			
			"glitch" => ImageFilter.Glitch,
			"noise" => ImageFilter.Noise,
			"noisecolor" => ImageFilter.NoiseColor,
			"vibrant" => ImageFilter.Vibrant,
			"vhs" => ImageFilter.Vhs,
			_ => throw new Exception("Expected image filter, found: " + values[n])
		};
	}
	
	public ImageScaling getImScalingAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected image scaling at position " + n);
		}
		
		return values[n].ToLower() switch{
			"neighbor" => ImageScaling.Neighbor,
			"bilinear" => ImageScaling.Bilinear,
			"area" => ImageScaling.Area,
			"bicubic" => ImageScaling.Bicubic,
			"spline" => ImageScaling.Spline,
			"lanczos" => ImageScaling.Lanczos,
			"fastbilinear" => ImageScaling.FastBilinear,
			"gauss" => ImageScaling.Gauss,
			"bicublin" => ImageScaling.Bicublin,
			"sinc" => ImageScaling.Sinc,
			_ => throw new Exception("Expected image scaling, found: " + values[n])
		};
	}
	
	public AudioFilter getAFilterAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected audio filter at position " + n);
		}
		
		return values[n].ToLower() switch{
			"none" => AudioFilter.None,
			"bassboost" => AudioFilter.BassBoost,
			"tremble" => AudioFilter.Tremble,
			"echo" => AudioFilter.Echo,
			"reverb" => AudioFilter.Reverb,
			"softclip" => AudioFilter.SoftClip,
			"bitcrush" => AudioFilter.BitCrush,
			"radio" => AudioFilter.Radio,
			"vhs" => AudioFilter.Vhs,
			"vinyl" => AudioFilter.Vinyl,
			"underwater" => AudioFilter.UnderWater,
			"dreamy" => AudioFilter.Dreamy,
			"lowquality" => AudioFilter.LowQuality,
			_ => throw new Exception("Expected audio filter, found: " + values[n])
		};
	}
	
	public override string ToString(){
		return keyword + ": " + string.Join(" ", values.Select(h => "'" + h + "'"));
	}
	
	//split string like cli does it
	static string[] splitLine(string l){
		bool stringOpened = false;
		bool previousEscapeCode = false;
		
		StringBuilder c = new StringBuilder();
		
		List<string> a = new List<string>();
		
		for(int i = 0; i < l.Length; i++){
			if(!previousEscapeCode){
				if(l[i] == '\"'){
					stringOpened = !stringOpened;
					continue;
				}
				
				if(l[i] == '\\'){
					previousEscapeCode = true;
					continue;
				}
			}else if(!(l[i] == '\"' || l[i] == '\\')){
				c.Append('\\');
			}
			
			previousEscapeCode = false;
			
			if(char.IsWhiteSpace(l[i]) && !stringOpened){
				if(c.Length > 0){
					a.Add(c.ToString());
					c.Clear();
				}
				continue;
			}
			
			c.Append(l[i]);
		}
		
		if(c.Length > 0){
			a.Add(c.ToString());
		}
		
		return a.ToArray();
	}
}