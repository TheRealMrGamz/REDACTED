Shader "Custom/TransparentCRTEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (0.2, 0.8, 0.4, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 5)) = 1.5
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.1
        _AberrationIntensity ("Aberration Intensity", Range(0, 0.1)) = 0.005
        _FlickerSpeed ("Flicker Speed", Float) = 5
        _FlickerIntensity ("Flicker Intensity", Range(0, 0.1)) = 0.02
        _Transparency ("Transparency", Range(0, 1)) = 0.8
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        // Enable transparency
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _EmissionColor;
            float _EmissionIntensity;
            float _ScanlineIntensity;
            float _AberrationIntensity;
            float _FlickerSpeed;
            float _FlickerIntensity;
            float _Transparency;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Screen curvature
                float2 uv = i.uv - 0.5;
                float dist = length(uv);
                uv *= 1.0 + dist * 0.1;
                uv += 0.5;

                // Chromatic aberration
                float2 uvR = uv + float2(_AberrationIntensity, 0);
                float2 uvG = uv;
                float2 uvB = uv - float2(_AberrationIntensity, 0);

                fixed4 texR = tex2D(_MainTex, uvR);
                fixed4 texG = tex2D(_MainTex, uvG);
                fixed4 texB = tex2D(_MainTex, uvB);

                // Combine color channels
                fixed4 col;
                col.r = texR.r;
                col.g = texG.g;
                col.b = texB.b;
                col.a = (texR.a + texG.a + texB.a) / 3;

                // Scanline effect
                float scanline = sin(uv.y * 300.0) * _ScanlineIntensity;

                // Screen flicker
                float flicker = sin(_Time.y * _FlickerSpeed) * _FlickerIntensity;

                // Apply effects and emission
                fixed4 finalColor = col * _EmissionColor * _EmissionIntensity;
                finalColor.rgb -= scanline + flicker;
                
                // Apply transparency
                finalColor.a *= _Transparency;
                
                // Add subtle glow
                float glow = max(0, 1 - dist * 2) * 0.2;
                finalColor.rgb += _EmissionColor.rgb * glow;

                return finalColor;
            }
            ENDCG
        }
    }
}