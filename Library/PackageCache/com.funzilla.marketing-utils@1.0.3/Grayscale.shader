Shader "Unlit/Grayscale"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"RenderType"="Opaque"
		}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct Vertex
			{
				float4 position : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Interpolate
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;

			Interpolate vert(Vertex input)
			{
				Interpolate output;
				output.vertex = UnityObjectToClipPos(input.position);
				output.uv = input.uv;
				return output;
			}

			fixed4 frag(Interpolate input) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, input.uv);
				float i = c.r * 0.299 + c.g * 0.587 + c.b * 0.114; 
				return fixed4(i, i, i, 1.0);
			}
			ENDCG
		}
	}
}