Shader "Map/MapLit" 
{
	Properties 
	{
		_MainTex ("Diffuse (RGB)", 2D) = "white" {}
	}

	SubShader 
	{ 
		Tags { "RenderType"="Opaque" }
		Fog { Mode Off }
		LOD 100
		Blend Off
		
		CGPROGRAM
		#pragma surface surf WrapLambert
		
		sampler2D _MainTex;
		
		struct Input 
		{
			half2 uv_MainTex;
			float4 color : COLOR; // Vertex Color
		};
		
		fixed4 LightingWrapLambert (SurfaceOutput s, half3 lightDir, fixed atten) 
		{
			fixed diff = dot(s.Normal, lightDir);
			
			fixed4 c;
			c.rgb = _LightColor0.rgb * (s.Albedo * diff);
			c.a = 0.0;
			return c;
  		}

		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 mainTex = tex2D(_MainTex,  IN.uv_MainTex);
			o.Albedo = mainTex.rgb * IN.color.rgb;
		}
		ENDCG
	}

	FallBack "Mobile/Diffuse"
}
