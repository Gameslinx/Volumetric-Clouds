Shader "Unlit/PostProcessClouds"
{
    Properties
    {
        _CloudTex("Cloud Tex", 3D) = "white" {}
        _CloudScale("Cloud Scale", Range(0.0001, 5)) = 0.05
        _CoverageMap("Coverage Map", 2D) = "black" {}
        _CoverageScale("Coverage Scale", Range(0.0001, 1)) = 0.01
        _CloudTypesMap("Cloud Types Map", 2D) = "black" {}
        _DetailTex("Detail Texture", 3D) = "white" {}
        _DetailScale("Detail Scale", Range(0.0001, 1)) = 1
        _DensityFactor("Density Factor", Range(0.001, 10)) = 3
        _DensityThreshold("Density Threshold", Range(0, 1)) = 0
        _StepSize ("Step Size", Range(0.000001, 2)) = 1
        _Absorption("Absorbtion", Range(0, 2)) = 0.1
        _Scattering("Scattering", Range(0.001, 5)) = 3
        _InnerSphereRadius("Inner Sphere Radius", Range(-100, 100)) = 10
        _OuterSphereRadius("Outer Sphere Radius", Range(-100, 100)) = 1
        _CloudBottomFadeDist("Cloud Bottom Fade Dist", Range(0, 1)) = 0.2
        _MaxDistance("Max Distance", Range(10, 1000)) = 10
        _MinTransmittance("Minimum Transmittance", Range(0, 1)) = 0.05
        _SphereCenter("Sphere Center", vector) = (0,0,0)
        _MainTex("Main Texture", 2D) = "white" {}
        _BlueNoise("Blue Noise Texture", 2D) = "black" {}
        _Debug("Debug", Range(-100, 100)) = 0
    }
    SubShader
    {
        Tags { "LightMode" = "ForwardBase" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile fwdbase
            
            #include "UnityCG.cginc"
            #include "CloudUtils.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;

                float3 viewDir : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 objectVertex : TEXCOORD3;
                float3 normal : NORMAL;
            };

            float4x4 _FrustumCorners;   // Allows us to get the pixel coordinates in world space
            sampler2D _CameraDepthTexture;
            
            sampler2D _MainTex;
            float _MinTransmittance;
            float _MaxDistance;

            int _FrameCount;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.objectVertex = v.vertex;
                o.normal = mul(unity_ObjectToWorld, v.normal);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                int frustumIndex = v.uv.x + (2 * o.uv.y);
                o.viewDir = normalize(_FrustumCorners[frustumIndex].xyz);
                o.worldPos = _WorldSpaceCameraPos + o.viewDir;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //float4 depth = tex2D(_CameraDepthTexture, i.uv).r;

                //return float4(i.viewDir, 1);

                //return float4(i.worldPos, 1);

                float4 tex = tex2D(_MainTex, i.uv);
                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                depth = LinearEyeDepth(depth) * length(i.viewDir);

                float2 uv = ((i.uv * _ScreenParams.xy) % 256) / 256;
                // Offset the ray (once inside a cloud) by a random amount * the step size to eliminate cloud banding
                float blueNoise = tex2Dlod(_BlueNoise, float4(uv, 0, 0));
                // Animate noise by adding the golden ratio and clamp to 01 range. Blue noise texture must be linear
                blueNoise = (blueNoise + 1.61803398875 * (float)_FrameCount) % 1.0f;

                Ray ray;
                ray.pos = _WorldSpaceCameraPos;// + i.viewDir * _Debug;
                ray.dir = normalize(i.viewDir);
                ray.density = 0;

                float transmittance = 1;
                float distFromSphere = 0;
                float lightEnergy = 0;

                float cosAngle = dot(ray.dir, _WorldSpaceLightPos0.xyz);
                float phaseValue = 3.0f * PI / 16.0f * (1 + cosAngle * cosAngle); // Phase(cosAngle, _ForwardScatteringAmount, _BackScatteringAmount, _ScatteringBlendFactor);

                float extinction = max(_Scattering + _Absorption, 1e-6);
                float lightTransmittance = 1;
                int timesInsideSDF = 0;

                for (int x = 0; x < 128; x++)
                {
                    lightTransmittance = 1;
                    //if (distance(i.worldPos, ray.pos) > depth)
                    //{
                    //    break;
                    //}
                    distFromSphere = PlaneSDF(ray.pos, ray.dir);
                    if (distFromSphere > _MaxDistance) { break; }
                    if (distFromSphere < EPSILON)
                    {
                        timesInsideSDF++;
                        ray.pos += ray.dir * _StepSize * blueNoise * (timesInsideSDF < 2);
                        ray.density = SampleDensity(ray.pos);
                        ray.density *= SampleDetailedDensity(ray.pos);
                        Ray sunRay;
                        sunRay.pos = ray.pos;
                        sunRay.dir = _WorldSpaceLightPos0;
                        sunRay.density = 0;

                        lightTransmittance = LightMarch(sunRay, 8, extinction); // Works the same as transmittance - Amount of light that reaches this point
                        lightEnergy += ray.density * transmittance * lightTransmittance * phaseValue * _StepSize * 1;
                        transmittance *= exp(-ray.density * _StepSize * extinction);
                        if (transmittance < _MinTransmittance)
                        {
                            break;
                        }
                        // After calculating sun intensity at this position, move next
                        
                        ray.pos += ray.dir * _StepSize;
                    }
                    else
                    {
                        ray.pos += ray.dir * distFromSphere;
                    }
                    
                }
                //return transmittance;
                //float4 col = float4(ShadeSH9(float4(_WorldSpaceLightPos0.xyz, 1)), 1);
                //return col;
                half4 envColor = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, _WorldSpaceLightPos0.xyz);
                //return envColor;
                //return transmittance;
                float4 cloudCol = tex * transmittance + lightEnergy * _LightColor0 + envColor * lightEnergy + UNITY_LIGHTMODEL_AMBIENT * (1 - transmittance);

                float distToPlane = PlaneSDF(i.worldPos, i.viewDir);

                return lerp(cloudCol, tex, smoothstep(0, 1, saturate(distToPlane / _MaxDistance)));
            }
            ENDCG
        }
    }
}