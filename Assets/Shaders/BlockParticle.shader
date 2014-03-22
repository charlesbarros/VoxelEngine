Shader "Map/BlockParticle" {
	Properties {
		_Color ("Color Tint", Color) = (1,1,1,1)   
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
			}
			
			SetTexture [_MainTex] {
                ConstantColor [_Color]
                Combine Texture * constant
            }
		}
	}
	FallBack "Unlit/Texture"
}
