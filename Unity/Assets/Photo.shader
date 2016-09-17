Shader "Custom/Photo" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)


		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Tex0("Tex0 (RGB)", 2D) = "white" {}
		_Tex1("Tex1 (RGB)", 2D) = "white" {}


	_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_Offset("Offset", Range(-1,1)) = 0

		_Mixer("Mixer", Range(0,1)) = 0
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		sampler2D _MainTex;
	sampler2D _Tex0;
	sampler2D _Tex1;

	struct Input {
		float2 uv_MainTex;
	};

	half _Mixer;
	half _Glossiness;
	half _Metallic;
	half _Offset;
	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		// Albedo comes from a texture tinted by color

		IN.uv_MainTex.x = 1 - IN.uv_MainTex;

		fixed4 t1 = tex2D(_Tex0, IN.uv_MainTex) * _Color;

		fixed4 t2 = tex2D(_Tex1, IN.uv_MainTex) * _Color;

		o.Albedo = half4(0, 0, 0, 1);
		// Metallic and smoothness come from slider variables
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Alpha = t1.a;

		o.Emission = t1.rgb;

		float offset = .5;

		float i = 1 - IN.uv_MainTex.y - _Offset;
		i = clamp(i, 0., 1.);

		o.Emission = lerp(t1, t2, _Mixer);
	}
	ENDCG
	}
		FallBack "Diffuse"
}
