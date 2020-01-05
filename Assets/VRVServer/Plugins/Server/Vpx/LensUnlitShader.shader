Shader "Unlit/LensUnlitShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
		Cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            sampler2D _MyTex;
            float4 _MyTex_ST;
			
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = v.vertex;
				o.vertex.x = o.vertex.x * 2 - 1;
				o.vertex.y = (o.vertex.y) * 2 - 1;
                o.uv = TRANSFORM_TEX(v.uv, _MyTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				// return float4(1.0, 0.0, 0.0, 1.0);
			
                // sample the texture
				fixed4 col = tex2D(_MyTex, i.uv);

				// rgb to yuv444

				float iR = col.r * 255.0;
				float iG = col.g * 255.0;
				float iB = col.b * 255.0;

				float iy =  0.257 * iR + 0.504 * iG + 0.098 * iB + 16.0;
				float iu = -0.148 * iR - 0.291 * iG + 0.439 * iB + 128.0;
				float iv =  0.439 * iR - 0.368 * iG - 0.071 * iB + 128.0;

				fixed4 c = float4(
					clamp(iy * (1.0 / 255.0), 0.0, 1.0),
					clamp(iu * (1.0 / 255.0), 0.0, 1.0),
					clamp(iv * (1.0 / 255.0), 0.0, 1.0),
					1.0);

				return c;
//				return col;
			}
            ENDCG
        }
    }
}
