Shader "MirrorWorld/AvatarSkin"
{
    Properties
    {
        _MainTex ("Diffuse Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _SpecularMap ("Specular Map", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _NormalStrength ("Normal Strength", Range(0,2)) = 1.0
        
        // Subsurface Scattering Properties
        _SubsurfaceColor ("Subsurface Color", Color) = (1,0.4,0.25,1)
        _SubsurfaceRadius ("Subsurface Radius", Range(0,1)) = 0.5
        _SubsurfaceIntensity ("Subsurface Intensity", Range(0,1)) = 0.3
        
        // Skin-specific properties
        _SkinTint ("Skin Tint", Color) = (1,0.8,0.7,1)
        _PoreStrength ("Pore Strength", Range(0,1)) = 0.1
        _WetnessAmount ("Wetness Amount", Range(0,1)) = 0.0
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "Queue"="Geometry"
        }
        LOD 300
        
        Pass
        {
            Name "ForwardBase"
            Tags { "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            
            sampler2D _MainTex;
            sampler2D _NormalMap;
            sampler2D _SpecularMap;
            float4 _MainTex_ST;
            
            fixed4 _Color;
            fixed4 _SubsurfaceColor;
            fixed4 _SkinTint;
            half _Smoothness;
            half _Metallic;
            half _NormalStrength;
            half _SubsurfaceRadius;
            half _SubsurfaceIntensity;
            half _PoreStrength;
            half _WetnessAmount;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float3 worldTangent : TEXCOORD3;
                float3 worldBinormal : TEXCOORD4;
                SHADOW_COORDS(5)
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBinormal = cross(o.worldNormal, o.worldTangent) * v.tangent.w;
                TRANSFER_SHADOW(o);
                return o;
            }
            
            // Subsurface scattering approximation
            fixed3 SubsurfaceScattering(float3 lightDir, float3 normal, float3 viewDir, fixed3 subsurfaceColor, half intensity)
            {
                half VdotL = saturate(dot(viewDir, -lightDir));
                half backlight = saturate(dot(normal, -lightDir));
                half subsurface = pow(saturate(VdotL), 4) * backlight * intensity;
                return subsurfaceColor * subsurface;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample textures
                fixed4 albedo = tex2D(_MainTex, i.uv) * _Color * _SkinTint;
                fixed3 normalMap = UnpackNormal(tex2D(_NormalMap, i.uv));
                normalMap.xy *= _NormalStrength;
                fixed3 specular = tex2D(_SpecularMap, i.uv).rgb;
                
                // Calculate world space normal
                float3x3 worldToTangent = float3x3(i.worldTangent, i.worldBinormal, i.worldNormal);
                float3 worldNormal = normalize(mul(normalMap, worldToTangent));
                
                // Lighting vectors
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir = normalize(lightDir + viewDir);
                
                // Lighting calculations
                half NdotL = max(0, dot(worldNormal, lightDir));
                half NdotV = max(0, dot(worldNormal, viewDir));
                half NdotH = max(0, dot(worldNormal, halfDir));
                half VdotH = max(0, dot(viewDir, halfDir));
                
                // Fresnel term
                half fresnel = pow(1.0 - VdotH, 5.0);
                half3 F0 = lerp(0.04, albedo.rgb, _Metallic);
                half3 F = F0 + (1.0 - F0) * fresnel;
                
                // Roughness and specular
                half roughness = 1.0 - _Smoothness;
                half alpha = roughness * roughness;
                half D = alpha / (3.14159 * pow(NdotH * NdotH * (alpha - 1.0) + 1.0, 2.0));
                half3 specularColor = D * F * specular;
                
                // Diffuse with subsurface
                half3 kS = F;
                half3 kD = (1.0 - kS) * (1.0 - _Metallic);
                half3 diffuse = kD * albedo.rgb / 3.14159;
                
                // Add subsurface scattering
                half3 subsurface = SubsurfaceScattering(lightDir, worldNormal, viewDir, _SubsurfaceColor.rgb, _SubsurfaceIntensity);
                
                // Combine lighting
                half shadow = SHADOW_ATTENUATION(i);
                half3 lighting = (diffuse + specularColor + subsurface) * _LightColor0.rgb * NdotL * shadow;
                
                // Add ambient
                lighting += ShadeSH9(half4(worldNormal, 1.0)) * albedo.rgb * 0.3;
                
                // Wetness effect
                if (_WetnessAmount > 0.001)
                {
                    half wetnessFresnel = pow(1.0 - NdotV, 3.0);
                    lighting += wetnessFresnel * _WetnessAmount * 0.1;
                }
                
                return fixed4(lighting, albedo.a);
            }
            ENDCG
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On ZTest LEqual ColorMask 0
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #include "UnityCG.cginc"
            
            struct v2f
            {
                V2F_SHADOW_CASTER;
            };
            
            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    
    FallBack "Diffuse"
}