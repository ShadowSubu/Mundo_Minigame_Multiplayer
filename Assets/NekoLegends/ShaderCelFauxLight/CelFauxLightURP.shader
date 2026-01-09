Shader "Neko Legends/Cel Faux Light"
{
    Properties
    {
        _Main_Texture("Main Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)
        _LightAzimuth("Light Horizontal Angle", Range(0, 360)) = 230
        _LightElevation("Light Vertical Angle", Range(-90, 90)) = 65

        _ShadingFalloff("Shading Falloff", Range(0,1)) = 0.3
        _Brightness("Brightness", Range(0,1)) = 0
        _RimOutput("RimLight", Range(0,1)) = 0.3
        _RimColor("RimColor", Color) = (1,1,1,1)
        [NoScaleOffset]_NormalTex("Normal Texture", 2D) = "bump" {}
        _NormalIntensity("Normal Intensity", Range(0,2)) = 1


        _UseEmission("Use Emission", Range(0,1)) = 0
        _EmissionTex("Emission Texture", 2D) = "black" {}
        _EmissionColor("Emission Color", Color) = (1,1,1,1) // Emission color property
        _EmissionIntensity("Emission Intensity", Range(0,5)) = 1
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass
            {
                CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                    float2 uvBump : TEXCOORD1;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float2 emissionUv : TEXCOORD1;
                    float3 normal: NORMAL;
                    float4 vertex : SV_POSITION;
                    float3 viewDir : TEXCOORD2;
                    float2 uvBump : TEXCOORD3;
                };

                sampler2D _Main_Texture;
                sampler2D _EmissionTex;
                sampler2D _NormalTex;
                float4 _Main_Texture_ST;
                float4 _Color;
                float _ShadingFalloff;
                float _Brightness;
                float _RimOutput;
                float4 _RimColor;
                float _UseEmission;
                float _EmissionIntensity;
                float4 _EmissionColor; // Emission color variable
                float4 _EmissionTex_ST;
                float _NormalIntensity;
                float _LightAzimuth;
                float _LightElevation;

                float toon(float3 normal, float3 lightDir)
                {
                    float NdotL = max(0.35, dot(normalize(normal), normalize(lightDir)));
                    return floor(NdotL / _ShadingFalloff);
                }

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _Main_Texture);
                    o.normal = UnityObjectToWorldNormal(v.normal);
                    o.viewDir = WorldSpaceViewDir(v.vertex);
                    o.emissionUv = TRANSFORM_TEX(v.uv, _EmissionTex);
                    o.uvBump = v.uvBump;
                    UNITY_TRANSFER_FOG(o, o.vertex);
                    return o;
                }


                fixed4 frag(v2f i) : SV_Target
                {
                    float3 viewDir = normalize(i.viewDir);
                    float4 rimDot = 1 - dot(viewDir, i.normal);
                    float4 rimIntensity = floor(rimDot / _RimOutput);
                    fixed4 col = tex2D(_Main_Texture, i.uv) * _Color;

                    // Initialize bumpedNormal with the original normal
                    float3 bumpedNormal = i.normal;

                    // Modify bumpedNormal using the normal map
                    if (_NormalIntensity > 0)
                    {
                        fixed4 normalColor = tex2D(_NormalTex, i.uvBump);
                        bumpedNormal = normalize(i.normal + (_NormalIntensity - 1) * (2 * normalColor.rgb - 1));
                    }

                    float phi = radians(_LightElevation);
                    float theta = radians(360.0 - _LightAzimuth);
                    float3 fakeLightDir;
                    fakeLightDir.x = sin(phi) * cos(theta);
                    fakeLightDir.y = cos(phi);
                    fakeLightDir.z = sin(phi) * sin(theta);

                    col *= toon(bumpedNormal, fakeLightDir) + _Brightness + (rimIntensity * _RimColor); // Use fakeLightDir here

                    // Add Emission effect
                    if (_UseEmission > 0.5)
                    {
                        fixed4 emission = tex2D(_EmissionTex, i.emissionUv);
                        col.rgb += emission.rgb * _EmissionColor.rgb * _EmissionIntensity; // multiply the emission color by the intensity
                    }

                    return col;
                }



                ENDCG
            }
        }
            CustomEditorForRenderPipeline "NekoLegends.CelFauxLightInspector" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
}