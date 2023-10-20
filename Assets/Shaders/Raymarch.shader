Shader "Unlit/CloudShader"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _StepSize ("Step Size", Range(0.000001, 1)) = 1
        _Absorbtion("Absorbtion", Range(-1, 1)) = 0.1
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend One OneMinusSrcAlpha
        LOD 100
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
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

                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 objectVertex : TEXCOORD3;
            };

            sampler3D _MainTex;
            float4 _MainTex_ST;
            float _StepSize;
            float _Absorbtion;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.objectVertex = v.vertex;

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.viewDir = o.worldPos - _WorldSpaceCameraPos;

                return o;
            }
            
            float4 BlendUnder(float4 color, float4 newColor)
            {
                color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                color.a += (1.0 - color.a) * newColor.a;
                return color;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                int maxRays = 128;

                float3 rayDir = mul(unity_WorldToObject, float4(normalize(i.viewDir), 1));
                float3 rayPos = i.objectVertex;
                float4 col = 0;

                float distance = 0;

                for (int c = 0; c < maxRays; c++)
                {
                    
                    if(max(abs(rayPos.x), max(abs(rayPos.y), abs(rayPos.z))) < 0.5f + 0.00001f)
                    {
                        float4 sampledCol = tex3D(_MainTex, rayPos + float3(0.5f, 0.5f, 0.5f));
                        sampledCol.a *= 0.01;
                        col = BlendUnder(col, sampledCol);

                        distance += _StepSize;

                        rayPos += _StepSize * rayDir;
                    }
                }

                float density = exp(-distance * _Absorbtion);

                return float4(1, 1, 1, density);
            }
            ENDCG
        }
    }
}