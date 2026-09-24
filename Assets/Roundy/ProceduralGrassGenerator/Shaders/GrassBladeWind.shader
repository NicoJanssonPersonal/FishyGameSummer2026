Shader "Roundy/GrassBladeWind"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _WindStrength ("Wind Strength", Range(0,2)) = 1
        _WindSpeed ("Wind Speed", Range(0,5)) = 1
        _WindScale ("Wind Scale", Range(0.1,10)) = 1
        _BrightnessMultiplier ("Brightness Multiplier", Range(0.1,2)) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 2
    }
   
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Cull [_Cull]
       
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile * LOD_FADE_CROSSFADE
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setupGPUInstancing
            #pragma target 3.0
           
            #include "UnityCG.cginc"
           
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
           
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 screenPos : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_INPUT_INSTANCE_ID  // Required for GPU Instancing
                UNITY_VERTEX_OUTPUT_STEREO      // Required for VR
            };
           
            sampler2D _MainTex;
            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float, _WindStrength)
                UNITY_DEFINE_INSTANCED_PROP(float, _WindSpeed)
                UNITY_DEFINE_INSTANCED_PROP(float, _WindScale)
                UNITY_DEFINE_INSTANCED_PROP(float, _BrightnessMultiplier)
            UNITY_INSTANCING_BUFFER_END(Props)

            static const half4x4 bayerMatrix = half4x4(
                0.0h, 0.5h, 0.125h, 0.625h,
                0.75h, 0.25h, 0.875h, 0.375h,
                0.1875h, 0.6875h, 0.0625h, 0.5625h,
                0.9375h, 0.4375h, 0.8125h, 0.3125h
            );
           
            float fastSin(float x)
            {
                x = x * 0.159155f;
                x = frac(x);
                x = x * 2.0f - 1.0f;
                return -4.0f * (x - x * abs(x));
            }
           
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
               
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                float windTime = _Time.y * UNITY_ACCESS_INSTANCED_PROP(Props, _WindSpeed);
                float windScale = UNITY_ACCESS_INSTANCED_PROP(Props, _WindScale);
                float windOffset = fastSin(worldPos.x * windScale + windTime)
                                + fastSin(worldPos.z * windScale * 0.5 + windTime * 1.2);
               
                float windStrength = UNITY_ACCESS_INSTANCED_PROP(Props, _WindStrength);
                float windFactor = v.vertex.y * v.color.a * windStrength;
                v.vertex.x += windOffset * windFactor * 0.1;
               
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                float4 instanceColor = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                float brightnessMultiplier = UNITY_ACCESS_INSTANCED_PROP(Props, _BrightnessMultiplier);
                o.color = v.color * instanceColor * brightnessMultiplier;
               
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
           
            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                #if defined(LOD_FADE_CROSSFADE)
                    half2 screenPos = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy * 0.5h;
                    uint2 ditherCoord = uint2(fmod(screenPos, 4));
                    half dither = bayerMatrix[ditherCoord.x][ditherCoord.y];
                    half fadeValue = unity_LODFade.x > 0 ?
                        unity_LODFade.x - dither :
                        unity_LODFade.x + dither;
                   
                    clip(fadeValue);
                #endif
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDHLSL
        }
    }
}