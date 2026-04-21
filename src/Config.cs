using System;
using System.Text;

class Config{
	static Random rand = new();
	
	static readonly string[] imageExt = {".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff", ".tga", ".webp", ".avif"};
	static readonly string[] audioExt = {".mp3", ".wav", ".m4a", ".aac", ".ogg", ".wma", ".flac"};
	
	
	int? width = null;
	int? height = null;
	
	int? slideNumMin = null;
	int? slideNumMax = null;
	
	float? defaultSlideDurationMin = null;
	float? defaultSlideDurationMax = null;
	
	string title = null;
	
	Transition? startTransition = null;
	float? startTransitionDurationMin = null;
	float? startTransitionDurationMax = null;
	
	Transition? endTransition = null;
	float? endTransitionDurationMin = null;
	float? endTransitionDurationMax = null;
	
	Color3? fillColor = null;
	
	ImSelectionMode? selMode = null;
	
	Transition? defaultSlideTransition = null;
	float? defaultSlideTransitionDurationMin = null;
	float? defaultSlideTransitionDurationMax = null;
	
	ImageFilter? defaultImageFilter = null;
	ImageScaling? defaultImageScaling = null;
	
	Dictionary<int, float> slideDurationMin = new();
	Dictionary<int, float> slideDurationMax = new();
	
	Dictionary<int, Transition> slideTransitionEffect = new();
	Dictionary<int, float> slideTransitionDurationMin = new();
	Dictionary<int, float> slideTransitionDurationMax = new();
	
	Dictionary<int, ImageFilter> imageFilter = new();
	Dictionary<int, ImageScaling> imageScaling = new();
	
	string[] imagePool = new string[0];
	
	Dictionary<int, string> images = new();
	
	string[] audioPool = new string[0];
	
	Priority priority = Priority.Merge;
	
	List<string> imagePoolUnique;
	
	bool hadParsingError = false;
	
	//Default values
	void FixNull(){
		slideNumMin ??= 1;
		slideNumMax ??= 10;
		
		defaultSlideDurationMin ??= 1f;
		defaultSlideDurationMax ??= 1f;
		
		title ??= "%d_%h";
		
		fillColor ??= new Color3(0, 0, 0);
		
		selMode ??= ImSelectionMode.Random;
		
		width ??= 1920;
		height ??= 1080;
		
		startTransition ??= Transition.None;
		startTransitionDurationMin ??= 1f;
		startTransitionDurationMax ??= 1f;
		
		endTransition ??= Transition.None;
		endTransitionDurationMin ??= 1f;
		endTransitionDurationMax ??= 1f;
		
		defaultSlideTransition ??= Transition.None;
		defaultSlideTransitionDurationMin ??= 0f;
		defaultSlideTransitionDurationMax ??= 0f;
		
		defaultImageFilter ??= ImageFilter.None;
		defaultImageScaling ??= ImageScaling.Neighbor;
	}
	
	public Resolved Resolve(){
		FixNull();
		
		string t = title.Replace("%d", DateTime.Now.ToString("yyyy_MM_dd")).Replace("%h", DateTime.Now.ToString("HH_mm_ss"));
		
		int num = randomInclusive((int) slideNumMin, (int) slideNumMax);
		
		if(num <= 0){
			throw new Exception("Slide count must be greater than 0, it was: " + num);
		}
		
		float[] durations = new float[num];
		string[] images = new string[num];
		Transition[] slideTrans = new Transition[num - 1];
		float[] slideTransDur = new float[num - 1];
		ImageFilter[] imFilter = new ImageFilter[num];
		ImageScaling[] imScaling = new ImageScaling[num];
		
		string audioPath = audioPool.Length == 0 ? null : audioPool[rand.Next(audioPool.Length)];
		
		imagePoolUnique = imagePool.ToList();
		
		for(int i = 1; i <= num; i++){
			try{
				durations[i - 1] = determineDuration(i);
				images[i - 1] = determineImage(i);
				imFilter[i - 1] = determineImageFilter(i);
				imScaling[i - 1] = determineImageScaling(i);
				
				if(i != num){
					slideTrans[i - 1] = determineSlideTransition(i);
					slideTransDur[i - 1] = determineSlideTransitionDuration(i);
				}
				
			}catch(Exception e){
				throw new Exception("Error happened determining slide " + i, e);
			}
		}
		
		float stTD = randomRange((float) startTransitionDurationMin, (float) startTransitionDurationMax);
		float enTD = randomRange((float) endTransitionDurationMin, (float) endTransitionDurationMax);
		
		return new Resolved{
			title = t,
			slideNum = num,
			slideDuration = durations,
			imagePaths = images,
			startTransition = (Transition) startTransition,
			startTransitionDuration = stTD,
			endTransition = (Transition) endTransition,
			endTransitionDuration = enTD,
			fillColor = (Color3) fillColor,
			width = (int) width,
			height = (int) height,
			slideTransitions = slideTrans,
			slideTransitionsDuration = slideTransDur,
			audioPath = audioPath,
			imageFilter = imFilter,
			imageScaling = imScaling,
		};
	}
	
	float determineDuration(int n){
		float m;
		float x;
		
		if(slideDurationMin.ContainsKey(n)){
			m = slideDurationMin[n];
		}else{
			m = (float) defaultSlideDurationMin;
		}
		
		if(slideDurationMax.ContainsKey(n)){
			x = slideDurationMax[n];
		}else{
			x = (float) defaultSlideDurationMax;
		}
		
		return randomRange(m, x);
	}
	
	string determineImage(int n){
		if(images.ContainsKey(n)){
			return images[n];
		}
		
		if(imagePool.Length == 0){
			throw new Exception("No images in image pool");
		}
		
		switch(selMode){
			case ImSelectionMode.Order:
				return imagePool[(n - 1) % imagePool.Length];
			
			case ImSelectionMode.Random:
				return imagePool[rand.Next(imagePool.Length)];
			
			case ImSelectionMode.Unique:
				if(imagePoolUnique.Count == 0){
					imagePoolUnique = imagePool.ToList();
				}
				int c = rand.Next(imagePoolUnique.Count);
				
				string ret = imagePoolUnique[c];
				imagePoolUnique.RemoveAt(c);
				
				return ret;
		}
		
		return imagePool[rand.Next(imagePool.Length)];
	}
	
	Transition determineSlideTransition(int n){
		if(slideTransitionEffect.ContainsKey(n)){
			return slideTransitionEffect[n];
		}
		
		return (Transition) defaultSlideTransition;
	}
	
	ImageFilter determineImageFilter(int n){
		if(imageFilter.ContainsKey(n)){
			return imageFilter[n];
		}
		
		return (ImageFilter) defaultImageFilter;
	}
	
	ImageScaling determineImageScaling(int n){
		if(imageScaling.ContainsKey(n)){
			return imageScaling[n];
		}
		
		return (ImageScaling) defaultImageScaling;
	}
	
	float determineSlideTransitionDuration(int n){
		float m;
		float x;
		
		if(slideTransitionDurationMin.ContainsKey(n)){
			m = slideTransitionDurationMin[n];
		}else{
			m = (float) defaultSlideTransitionDurationMin;
		}
		
		if(slideTransitionDurationMax.ContainsKey(n)){
			x = slideTransitionDurationMax[n];
		}else{
			x = (float) defaultSlideTransitionDurationMax;
		}
		
		return randomRange(m, x);
	}
	
	
	public void Include(Config g){
		switch(g.priority){
			case Priority.Ignore:
				slideNumMin ??= g.slideNumMin;
				slideNumMax ??= g.slideNumMax;
				
				defaultSlideDurationMin ??= g.defaultSlideDurationMin;
				defaultSlideDurationMax ??= g.defaultSlideDurationMax;
				
				title ??= g.title;
				
				startTransition ??= g.startTransition;
				startTransitionDurationMin ??= g.startTransitionDurationMin;
				startTransitionDurationMax ??= g.startTransitionDurationMax;
				
				endTransition ??= g.endTransition;
				endTransitionDurationMin ??= g.endTransitionDurationMin;
				endTransitionDurationMax ??= g.endTransitionDurationMax;
				
				fillColor ??= g.fillColor;
				
				selMode ??= g.selMode;
				
				width ??= g.width;
				height ??= g.height;
				
				defaultSlideTransition ??= g.defaultSlideTransition;
				defaultSlideTransitionDurationMin ??= g.defaultSlideTransitionDurationMin;
				defaultSlideTransitionDurationMax ??= g.defaultSlideTransitionDurationMax;
				
				defaultImageFilter ??= g.defaultImageFilter;
				defaultImageScaling ??= g.defaultImageScaling;
				
				//####
				if(slideDurationMin.Count == 0){
					slideDurationMin = g.slideDurationMin;
				}
				
				if(slideDurationMax.Count == 0){
					slideDurationMax = g.slideDurationMax;
				}
				
				if(imagePool.Length == 0){
					imagePool = g.imagePool;
				}
				
				if(images.Count == 0){
					images = g.images;
				}
				
				if(slideTransitionEffect.Count == 0){
					slideTransitionEffect = g.slideTransitionEffect;
				}
				
				if(slideTransitionDurationMin.Count == 0){
					slideTransitionDurationMin = g.slideTransitionDurationMin;
				}
				
				if(slideTransitionDurationMax.Count == 0){
					slideTransitionDurationMax = g.slideTransitionDurationMax;
				}
				
				if(imageFilter.Count == 0){
					imageFilter = g.imageFilter;
				}
				
				if(imageScaling.Count == 0){
					imageScaling = g.imageScaling;
				}
				
				if(audioPool.Length == 0){
					audioPool = g.audioPool;
				}
				
				break;
			
			case Priority.Override:
				slideNumMin = g.slideNumMin ?? slideNumMin;
				slideNumMax = g.slideNumMax ?? slideNumMax;
				
				defaultSlideDurationMin = g.defaultSlideDurationMin ?? defaultSlideDurationMin;
				defaultSlideDurationMax = g.defaultSlideDurationMax ?? defaultSlideDurationMax;
				
				title = g.title ?? title;
				
				startTransition = g.startTransition ?? startTransition;
				startTransitionDurationMin = g.startTransitionDurationMin ?? startTransitionDurationMin;
				startTransitionDurationMax = g.startTransitionDurationMax ?? startTransitionDurationMax;
				
				endTransition = g.endTransition ?? endTransition;
				endTransitionDurationMin = g.endTransitionDurationMin ?? endTransitionDurationMin;
				endTransitionDurationMax = g.endTransitionDurationMax ?? endTransitionDurationMax;
				
				fillColor = g.fillColor ?? fillColor;
				
				selMode = g.selMode ?? selMode;
				
				width = g.width ?? width;
				height = g.height ?? height;
				
				defaultSlideTransition = g.defaultSlideTransition ?? defaultSlideTransition;
				defaultSlideTransitionDurationMin = g.defaultSlideTransitionDurationMin ?? defaultSlideTransitionDurationMin;
				defaultSlideTransitionDurationMax = g.defaultSlideTransitionDurationMax ?? defaultSlideTransitionDurationMax;
				
				defaultImageFilter = g.defaultImageFilter ?? defaultImageFilter;
				defaultImageScaling = g.defaultImageScaling ?? defaultImageScaling;
				
				//####
				if(g.slideDurationMin.Count != 0){
					slideDurationMin = g.slideDurationMin;
				}
				
				if(g.slideDurationMax.Count != 0){
					slideDurationMax = g.slideDurationMax;
				}
				
				if(g.imagePool.Length != 0){
					imagePool = g.imagePool;
				}
				
				if(g.images.Count != 0){
					images = g.images;
				}
				
				if(g.slideTransitionEffect.Count != 0){
					slideTransitionEffect = g.slideTransitionEffect;
				}
				
				if(g.slideTransitionDurationMin.Count != 0){
					slideTransitionDurationMin = g.slideTransitionDurationMin;
				}
				
				if(g.slideTransitionDurationMax.Count != 0){
					slideTransitionDurationMax = g.slideTransitionDurationMax;
				}
				
				if(imageFilter.Count != 0){
					imageFilter = g.imageFilter;
				}
				
				if(imageScaling.Count != 0){
					imageScaling = g.imageScaling;
				}
				
				if(g.audioPool.Length != 0){
					audioPool = g.audioPool;
				}
				
				break;
			
			case Priority.MergeReverse:
				slideNumMin = g.slideNumMin ?? slideNumMin;
				slideNumMax = g.slideNumMax ?? slideNumMax;
				
				defaultSlideDurationMin = g.defaultSlideDurationMin ?? defaultSlideDurationMin;
				defaultSlideDurationMax = g.defaultSlideDurationMax ?? defaultSlideDurationMax;
				
				title = g.title ?? title;
				
				startTransition = g.startTransition ?? startTransition;
				startTransitionDurationMin = g.startTransitionDurationMin ?? startTransitionDurationMin;
				startTransitionDurationMax = g.startTransitionDurationMax ?? startTransitionDurationMax;
				
				endTransition = g.endTransition ?? endTransition;
				endTransitionDurationMin = g.endTransitionDurationMin ?? endTransitionDurationMin;
				endTransitionDurationMax = g.endTransitionDurationMax ?? endTransitionDurationMax;
				
				fillColor = g.fillColor ?? fillColor;
				
				selMode = g.selMode ?? selMode;
				
				width = g.width ?? width;
				height = g.height ?? height;
				
				defaultSlideTransition = g.defaultSlideTransition ?? defaultSlideTransition;
				defaultSlideTransitionDurationMin = g.defaultSlideTransitionDurationMin ?? defaultSlideTransitionDurationMin;
				defaultSlideTransitionDurationMax = g.defaultSlideTransitionDurationMax ?? defaultSlideTransitionDurationMax;
				
				defaultImageFilter = g.defaultImageFilter ?? defaultImageFilter;
				defaultImageScaling = g.defaultImageScaling ?? defaultImageScaling;
				
				//####
				slideDurationMin = MergeDicts(g.slideDurationMin, slideDurationMin);
				slideDurationMax = MergeDicts(g.slideDurationMax, slideDurationMax);
				
				imagePool = g.imagePool.Concat(imagePool).ToArray();
				
				images = MergeDicts(g.images, images);
				
				slideTransitionEffect = MergeDicts(g.slideTransitionEffect, slideTransitionEffect);
				slideTransitionDurationMin = MergeDicts(g.slideTransitionDurationMin, slideTransitionDurationMin);
				slideTransitionDurationMax = MergeDicts(g.slideTransitionDurationMax, slideTransitionDurationMax);
				
				imageFilter = MergeDicts(g.imageFilter, imageFilter);
				imageScaling = MergeDicts(g.imageScaling, imageScaling);
				
				audioPool = g.audioPool.Concat(audioPool).ToArray();
				
				break;
			
			case Priority.Merge:
				slideNumMin ??= g.slideNumMin;
				slideNumMax ??= g.slideNumMax;
				
				defaultSlideDurationMin ??= g.defaultSlideDurationMin;
				defaultSlideDurationMax ??= g.defaultSlideDurationMax;
				
				title ??= g.title;
				
				startTransition ??= g.startTransition;
				startTransitionDurationMin ??= g.startTransitionDurationMin;
				startTransitionDurationMax ??= g.startTransitionDurationMax;
				
				endTransition ??= g.endTransition;
				endTransitionDurationMin ??= g.endTransitionDurationMin;
				endTransitionDurationMax ??= g.endTransitionDurationMax;
				
				fillColor ??= g.fillColor;
				
				selMode ??= g.selMode;
				
				width ??= g.width;
				height ??= g.height;
				
				defaultSlideTransition ??= g.defaultSlideTransition;
				defaultSlideTransitionDurationMin ??= g.defaultSlideTransitionDurationMin;
				defaultSlideTransitionDurationMax ??= g.defaultSlideTransitionDurationMax;
				
				defaultImageFilter ??= g.defaultImageFilter;
				defaultImageScaling ??= g.defaultImageScaling;
				
				//####
				slideDurationMin = MergeDicts(slideDurationMin, g.slideDurationMin);
				slideDurationMax = MergeDicts(slideDurationMax, g.slideDurationMax);
				
				imagePool = imagePool.Concat(g.imagePool).ToArray();
				
				images = MergeDicts(images, g.images);
				
				slideTransitionEffect = MergeDicts(slideTransitionEffect, g.slideTransitionEffect);
				slideTransitionDurationMin = MergeDicts(slideTransitionDurationMin, g.slideTransitionDurationMin);
				slideTransitionDurationMax = MergeDicts(slideTransitionDurationMax, g.slideTransitionDurationMax);
				
				imageFilter = MergeDicts(imageFilter, g.imageFilter);
				imageScaling = MergeDicts(imageScaling, g.imageScaling);
				
				audioPool = audioPool.Concat(g.audioPool).ToArray();
				
				break;
		}
	}
	
	
	public static Config Parse(string path){
		
		string directory = Path.GetDirectoryName(path);
		
		string[] srcLines = File.ReadAllLines(path);
		
		srcLines = srcLines.Select(h => h.Trim()) //Trim
							.Where(h => !string.IsNullOrEmpty(h))
							.Where(h => !(h.StartsWith("#") || h.StartsWith("//"))).ToArray();
		
		ConfigLine[] lines = srcLines.Select(h => new ConfigLine(h)).ToArray();
		
		Config g = new();
		
		List<string> imageFolders = new();
		List<string> audioFolders = new();
		Dictionary<int, string> images = new();
		List<string> toInclude = new();
		
		foreach(ConfigLine l in lines){
			try{
				switch(l.keyword){
					case "include":
						if(l.valueNum < 1){
							parseError("Expected at least 1 values", l);
							break;
						}
						
						//Choose only one randomly. Doing it here makes it so we dont have to store anything else
						toInclude.Add(l.values[rand.Next(l.values.Length)]);
						
						break;
					
					case "priority":
						if(!testValLen(1, l)){
							break;
						}
						
						g.priority = l.getPriorityAt(0);
						break;
					
					case "seed":
						if(!testValLen(1, l)){
							break;
						}
						
						rand = new Random(l.getIntAt(0));
						break;
					
					case "video_out":
						if(!testValLen(1, l)){
							break;
						}
						
						g.title = l.getValAt(0);
						break;
					
					case "video_width":
						if(!testValLen(1, l)){
							break;
						}
						
						g.width = l.getUintAt(0);
						break;
					
					case "video_height":
						if(!testValLen(1, l)){
							break;
						}
						
						g.height = l.getUintAt(0);
						break;
					
					case "video_dims":
						if(!testValLen(2, l)){
							break;
						}
						
						g.width = l.getUintAt(0);
						g.height = l.getUintAt(1);
						break;
					
					case "in_transition":
						if(l.valueNum == 2){
							g.startTransition = l.getTransitionAt(0);
							float n = l.getUnumAt(1);
							g.startTransitionDurationMin = n;
							g.startTransitionDurationMax = n;
						}else if(l.valueNum == 3){
							g.startTransition = l.getTransitionAt(0);
							g.startTransitionDurationMin = l.getUnumAt(1);
							g.startTransitionDurationMax = l.getUnumAt(2);
						}else{
							parseError("Expected either 2 or 3 values", l);
						}
						
						break;
					
					case "in_transition_dur":
						if(l.valueNum == 1){
							float n = l.getUnumAt(0);
							g.startTransitionDurationMin = n;
							g.startTransitionDurationMax = n;
						}else if(l.valueNum == 2){
							g.startTransitionDurationMin = l.getUnumAt(0);
							g.startTransitionDurationMax = l.getUnumAt(1);
						}else{
							parseError("Expected either 1 or 2 values", l);
						}
						
						break;
					
					case "in_transition_dur_min":
						if(!testValLen(1, l)){
							break;
						}
						
						g.startTransitionDurationMin = l.getUnumAt(0);
						break;
					
					case "in_transition_dur_max":
						if(!testValLen(1, l)){
							break;
						}
						
						g.startTransitionDurationMax = l.getUnumAt(0);
						break;
					
					case "in_transition_effect":
						if(!testValLen(1, l)){
							break;
						}
						
						g.startTransition = l.getTransitionAt(0);
						break;
					
					case "out_transition":
						if(l.valueNum == 2){
							g.endTransition = l.getTransitionAt(0);
							float n = l.getUnumAt(1);
							g.endTransitionDurationMin = n;
							g.endTransitionDurationMax = n;
						}else if(l.valueNum == 3){
							g.endTransition = l.getTransitionAt(0);
							g.endTransitionDurationMin = l.getUnumAt(1);
							g.endTransitionDurationMax = l.getUnumAt(2);
						}else{
							parseError("Expected either 2 or 3 values", l);
						}
						break;
					
					case "out_transition_dur":
						if(l.valueNum == 1){
							float n = l.getUnumAt(0);
							g.endTransitionDurationMin = n;
							g.endTransitionDurationMax = n;
						}else if(l.valueNum == 2){
							g.endTransitionDurationMin = l.getUnumAt(0);
							g.endTransitionDurationMax = l.getUnumAt(1);
						}else{
							parseError("Expected either 1 or 2 values", l);
						}
						
						break;
					
					case "out_transition_dur_min":
						if(!testValLen(1, l)){
							break;
						}
						
						g.endTransitionDurationMin = l.getUnumAt(0);
						break;
					
					case "out_transition_dur_max":
						if(!testValLen(1, l)){
							break;
						}
						
						g.endTransitionDurationMax = l.getUnumAt(0);
						break;
					
					case "out_transition_effect":
						if(!testValLen(1, l)){
							break;
						}
						
						g.startTransition = l.getTransitionAt(0);
						break;
					
					case "slide_count":
						if(l.valueNum == 1){
							int n = l.getUintAt(0);
							g.slideNumMin = n;
							g.slideNumMax = n;
						}else if(l.valueNum == 2){
							g.slideNumMin = l.getUintAt(0);
							g.slideNumMax = l.getUintAt(1);
						}else{
							parseError("Expected either 1 or 2 values", l);
						}
						
						break;
					
					case "slide_count_min":
						if(!testValLen(1, l)){
							break;
						}
						
						g.slideNumMin = l.getUintAt(0);
						break;
					
					case "slide_count_max":
						if(!testValLen(1, l)){
							break;
						}
						
						g.slideNumMax = l.getUintAt(0);
						break;
					
					case "def_slide_dur":
						if(l.valueNum == 1){
							float n = l.getUnumAt(0);
							g.defaultSlideDurationMin = n;
							g.defaultSlideDurationMax = n;
						}else if(l.valueNum == 2){
							g.defaultSlideDurationMin = l.getUnumAt(0);
							g.defaultSlideDurationMax = l.getUnumAt(1);
						}else{
							parseError("Expected either 1 or 2 values", l);
						}
						
						break;
					
					case "def_slide_dur_min":
						if(!testValLen(1, l)){
							break;
						}
						
						g.defaultSlideDurationMin = l.getUnumAt(0);
						break;
					
					case "def_slide_dur_max":
						if(!testValLen(1, l)){
							break;
						}
						
						g.defaultSlideDurationMax = l.getUnumAt(0);
						break;
					
					case "slide_dur":
						if(l.valueNum == 2){
							int s = l.getIntAt(0);
							if(s <= 0){
								parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
								break;
							}
							
							float n = l.getUnumAt(1);
							g.slideDurationMin[s] = n;
							g.slideDurationMax[s] = n;
						}else if(l.valueNum == 3){
							int s = l.getIntAt(0);
							if(s <= 0){
								parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
								break;
							}
							
							g.slideDurationMin[s] = l.getUnumAt(1);
							g.slideDurationMax[s] = l.getUnumAt(2);
						}else{
							parseError("Expected either 2 or 3 values", l);
						}
						break;
					
					case "slide_dur_min":
						if(!testValLen(2, l)){
							break;
						}
						
						int s2 = l.getIntAt(0);
						if(s2 <= 0){
							parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
							break;
						}
						
						g.slideDurationMin[s2] = l.getUnumAt(1);
						break;
					
					case "slide_dur_max":
						if(!testValLen(2, l)){
							break;
						}
						
						s2 = l.getIntAt(0);
						if(s2 <= 0){
							parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
							break;
						}
						
						g.slideDurationMax[s2] = l.getUnumAt(1);
						break;
					
					case "slide_fill_color":
						if(!testValLen(1, l)){
							break;
						}
						
						g.fillColor = l.getColorAt(0);
						break;
					
					case "def_slide_transition":
						if(l.valueNum == 2){
							g.defaultSlideTransition = l.getTransitionAt(0);
							float n = l.getUnumAt(1);
							g.defaultSlideTransitionDurationMin = n;
							g.defaultSlideTransitionDurationMax = n;
						}else if(l.valueNum == 3){
							g.defaultSlideTransition = l.getTransitionAt(0);
							g.defaultSlideTransitionDurationMin = l.getUnumAt(1);
							g.defaultSlideTransitionDurationMax = l.getUnumAt(2);
						}else{
							parseError("Expected either 2 or 3 values", l);
						}
						
						break;
					
					case "def_slide_transition_dur":
						if(l.valueNum == 1){
							float n = l.getUnumAt(0);
							g.defaultSlideTransitionDurationMin = n;
							g.defaultSlideTransitionDurationMax = n;
						}else if(l.valueNum == 2){
							g.defaultSlideTransitionDurationMin = l.getUnumAt(0);
							g.defaultSlideTransitionDurationMax = l.getUnumAt(1);
						}else{
							parseError("Expected either 1 or 2 values", l);
						}
						
						break;
					
					case "def_slide_transition_dur_min":
						if(!testValLen(1, l)){
							break;
						}
						
						g.defaultSlideTransitionDurationMin = l.getUnumAt(0);
						break;
					
					case "def_slide_transition_dur_max":
						if(!testValLen(1, l)){
							break;
						}
						
						g.defaultSlideTransitionDurationMax = l.getUnumAt(0);
						break;
					
					case "def_slide_transition_effect":
						if(!testValLen(1, l)){
							break;
						}
						
						g.defaultSlideTransition = l.getTransitionAt(0);
						break;
					
					case "slide_transition":
						if(l.valueNum == 3){
							int s = l.getIntAt(0);
							if(s <= 0){
								parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
								break;
							}
							
							g.slideTransitionEffect[s] = l.getTransitionAt(1);
							float n = l.getUnumAt(2);
							g.slideTransitionDurationMin[s] = n;
							g.slideTransitionDurationMax[s] = n;
						}else if(l.valueNum == 4){
							int s = l.getIntAt(0);
							if(s <= 0){
								parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
								break;
							}
							
							g.slideTransitionEffect[s] = l.getTransitionAt(1);
							g.slideTransitionDurationMin[s] = l.getUnumAt(2);
							g.slideTransitionDurationMax[s] = l.getUnumAt(3);
						}else{
							parseError("Expected either 3 or 4 values", l);
						}
						break;
					
					case "slide_transition_dur":
						if(l.valueNum == 2){
							int s = l.getIntAt(0);
							if(s <= 0){
								parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
								break;
							}
							
							float n = l.getUnumAt(1);
							g.slideTransitionDurationMin[s] = n;
							g.slideTransitionDurationMax[s] = n;
						}else if(l.valueNum == 3){
							int s = l.getIntAt(0);
							if(s <= 0){
								parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
								break;
							}
							
							g.slideTransitionDurationMin[s] = l.getUnumAt(1);
							g.slideTransitionDurationMax[s] = l.getUnumAt(2);
						}else{
							parseError("Expected either 2 or 3 values", l);
						}
						break;
					
					case "slide_transition_dur_min":
						if(!testValLen(2, l)){
							break;
						}
						
						s2 = l.getIntAt(0);
						if(s2 <= 0){
							parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
							break;
						}
						
						g.slideTransitionDurationMin[s2] = l.getUnumAt(1);
						break;
					
					case "slide_transition_dur_max":
						if(!testValLen(2, l)){
							break;
						}
						
						s2 = l.getIntAt(0);
						if(s2 <= 0){
							parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
							break;
						}
						
						g.slideTransitionDurationMax[s2] = l.getUnumAt(1);
						break;
					
					case "slide_transition_effect":
						if(!testValLen(2, l)){
							break;
						}
						
						s2 = l.getIntAt(0);
						if(s2 <= 0){
							parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
							break;
						}
						
						g.slideTransitionEffect[s2] = l.getTransitionAt(1);
						break;
					
					case "slide_image":
						if(!testValLen(2, l)){
							break;
						}
						
						s2 = l.getIntAt(0);
						if(s2 <= 0){
							parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
							break;
						}
						
						images[s2] = l.getValAt(1);
						break;
					
					case "def_slide_filter":
						if(!testValLen(1, l)){
							break;
						}
						
						g.defaultImageFilter = l.getImFilterAt(0);
						
						break;
					
					case "slide_filter":
						if(!testValLen(2, l)){
							break;
						}
						
						s2 = l.getIntAt(0);
						if(s2 <= 0){
							parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
							break;
						}
						
						g.imageFilter[s2] = l.getImFilterAt(1);
						break;
					
					case "def_slide_scaling":
						if(!testValLen(1, l)){
							break;
						}
						
						g.defaultImageScaling = l.getImScalingAt(0);
						
						break;
					
					case "slide_scaling":
						if(!testValLen(2, l)){
							break;
						}
						
						s2 = l.getIntAt(0);
						if(s2 <= 0){
							parseError("Expected a slide number bigger than 0 (no 0 indexing!)", l);
							break;
						}
						
						g.imageScaling[s2] = l.getImScalingAt(1);
						break;
					
					case "image_selection_mode":
						if(!testValLen(1, l)){
							break;
						}
						
						g.selMode = l.getImSelModeAt(0);
						break;
					
					case "image_folder":
						if(!testValLen(1, l)){
							break;
						}
						
						//Just for parsing
						imageFolders.Add(l.getValAt(0));
						break;
					
					case "image_pool":
						g.imagePool = g.imagePool.Concat(l.values).ToArray();
						break;
					
					case "audio_folder":
						if(!testValLen(1, l)){
							break;
						}
						
						//Just for parsing
						audioFolders.Add(l.getValAt(0));
						break;
					
					case "audio_pool":
						g.audioPool = g.audioPool.Concat(l.values).ToArray();
						break;
					
					default:
						parseError("Unknown option", l);
						break;
				}
			}catch(ParseException pe){
				g.hadParsingError = true;
				parseErrorNoThrow(pe.ToString());
			}catch(Exception e){
				g.hadParsingError = true;
				parseErrorNoThrow(new ParseException(e.Message, l).ToString());
			}
		}
		
		//Precompute
		
		g.imagePool = g.imagePool.Select(h => getGlobalPath(h, directory)).ToArray();
		g.audioPool = g.audioPool.Select(h => getGlobalPath(h, directory)).ToArray();
		
		foreach(string imFolder in imageFolders){
			string actualPath = getGlobalPath(imFolder, directory);
			
			string[] pngFiles = Directory.GetFiles(actualPath, "*.*").Where(h => imageExt.Contains(Path.GetExtension(h).ToLower())).ToArray(); //Is global always
			
			g.imagePool = g.imagePool.Concat(pngFiles).ToArray();
		}
		
		foreach(string auFolder in audioFolders){
			string actualPath = getGlobalPath(auFolder, directory);
			
			string[] files = Directory.GetFiles(actualPath, "*.*").Where(h => audioExt.Contains(Path.GetExtension(h).ToLower())).ToArray(); //Is global always
			
			g.audioPool = g.audioPool.Concat(files).ToArray();
		}
		
		g.imagePool = g.imagePool.Distinct().ToArray();
		g.audioPool = g.audioPool.Distinct().ToArray();
		
		foreach(KeyValuePair<int, string> kvp in images){
			g.images[kvp.Key] = getGlobalPath(kvp.Value, directory);
		}
		
		if(g.title != null){
			g.title = getGlobalPath(g.title, directory);
		}
		
		//Include
		foreach(string s in toInclude){
			try{
				string h = getGlobalPath(s, directory);
				
				if(!File.Exists(h)){
					throw new Exception("Include not found: '" + h + "'");
					continue;
				}
				
				Config t = Config.Parse(h);
				
				g.Include(t);
			}catch(Exception e){
				g.hadParsingError = true;
				parseErrorNoThrow(e.ToString());
			}
		}
		
		if(g.hadParsingError){
			throw new Exception("Unable to finish: config had error");
		}
		
		return g;
	}
	
	static string getGlobalPath(string path, string directory){
		return Path.IsPathRooted(path) ? path : Path.GetFullPath(Path.Combine(directory, path));
	}
	
	static bool testValLen(int l, ConfigLine c){
		if(c.valueNum != l){
			parseError("Expected " + l + " values", c);
			return false;
		}
		return true;
	}
	
	static void parseError(string message, ConfigLine line){
		throw new ParseException(message, line);
	}
	
	static void parseErrorNoThrow(string message){
		Console.Error.WriteLine(message);
	}
	
	static int randomInclusive(int min, int max){
		if(min == max){
			return min;
		}
		
		if(min > max){
			throw new Exception("Minimum is greater than maximum: [" + min + "," + max + "]");
		}
		
		return rand.Next(min, max + 1);
	}
	
	static float randomRange(float min, float max){
		if(min == max){
			return min;
		}
		
		if(min > max){
			throw new Exception("Minimum is greater than maximum: [" + min + "," + max + "]");
		}
		
		return (float)(rand.NextDouble() * (max - min) + min);
	}
	
	static Dictionary<TKey, TValue> MergeDicts<TKey, TValue>(Dictionary<TKey, TValue> parent, Dictionary<TKey, TValue> child){
		Dictionary<TKey, TValue> o = new Dictionary<TKey, TValue>(parent);
		
		foreach(KeyValuePair<TKey, TValue> kvp in child){
			if(!o.ContainsKey(kvp.Key)){
				o[kvp.Key] = kvp.Value;
			}
		}
		
		return o;
	}
}

class ParseException : Exception{
	public ParseException() { }
	
	public ParseException(string message, ConfigLine line)
		: base("Error on \"" + line + "\":\n\t" + message) { }
	
	public ParseException(string message)
		: base(message) { }
	
	public ParseException(string message, Exception inner)
		: base(message, inner) { }
}

enum Priority{
	Ignore, Override, Merge, MergeReverse
}

enum Transition{
	None, Fade, Black, White
}

enum ImSelectionMode{
	Random, Unique, Order
}

//hue=s=0, scale=w=256:h='256 * (ih/iw)':flags=neighbor,
enum ImageFilter{
	None, GrayScale, Pixelize
}

enum ImageScaling{
	Neighbor, Bilinear, Area, Bicubic, Spline, Lanczos, FastBilinear, Gauss
}