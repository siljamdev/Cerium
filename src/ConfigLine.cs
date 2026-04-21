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
	
	public Transition getTransitionAt(int n){
		if(!tryValueAt(n)){
			throw new Exception("Expected transition at position " + n);
		}
		
		return values[n].ToLower() switch{
			"none" => Transition.None,
			"fade" => Transition.Fade,
			"black" => Transition.Black,
			"white" => Transition.White,
			_ => throw new Exception("Expected transition name, found: " + values[n])
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
			"pixelize" => ImageFilter.Pixelize,
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
			"fast_bilinear" => ImageScaling.FastBilinear,
			"gauss" => ImageScaling.Gauss,
			_ => throw new Exception("Expected image scaling, found: " + values[n])
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