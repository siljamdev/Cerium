using System;

public struct Color3{
	public byte R;
	public byte G;
	public byte B;
	
	public Color3(byte r, byte g, byte b){
		this.R = r;
		this.G = g;
		this.B = b;
	}
	
	public Color3(string hex){
		this = Parse(hex);
	}
	
	public static Color3 Parse(string hex){
		if(hex == null){
			throw new ArgumentNullException(nameof(hex));
		}
		
		if(hex.StartsWith("#")){
            hex = hex.Substring(1);
        }
		
		if(hex.Length != 6){
            throw new Exception("Hexadecimal color string must be 6 or 7 characters long.");
        }
		
		byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
		
		return new Color3(r, g, b);
	}
	
	public static bool TryParse(string hex, out Color3 col){
		if(hex == null){
			col = new Color3(0, 0, 0);
            return false;
		}
		
		if(hex.StartsWith("#")){
            hex = hex.Substring(1);
        }
		
		if(hex.Length != 6){
			col = new Color3(0, 0, 0);
            return false;
        }
		
		try{
			byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(hex.Substring(2, 2).Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(hex.Substring(4, 2).Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			
			col = new Color3(r, g, b);
			return true;
		}catch{
			col = new Color3(0, 0, 0);
			return false;
		}
	}
	
	public static bool operator ==(Color3 a, Color3 b){
		if(a.R == b.R && a.G == b.G && a.B == b.B){
			return true;
		}
		return false;
	}
	
	public static bool operator !=(Color3 a, Color3 b){
		return !(a == b);
	}
	
	public override bool Equals(object obj){
        if (obj is Color3 other)
            return this == other;
        else
            return false;
    }
	
	public override string ToString(){
		return "#" + this.R.ToString("X2") + this.G.ToString("X2") + this.B.ToString("X2");
	}
}