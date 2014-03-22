Shader "Map/Map" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 80
		
		Pass {
			Lighting Off
			
			Material {
				Diffuse (1,1,1,1)
				Ambient (1,1,1,1)
			}
			
			BindChannels {
				Bind "Vertex", vertex
   				Bind "texcoord", texcoord
   				Bind "Color", color
			}
			
			SetTexture [_MainTex] {
				Combine texture * primary
			} 
		}
	}
	FallBack "Unlit/Texture"
}
