Shader "UI/DirectionGlitch"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(State)]
        _Danger ("Danger (0 or 1)", Float) = 0
        _Selected ("Selected (0 or 1)", Float) = 0

        [Header(Danger Glitch)]
        _DangerColor0 ("Danger Color 0", Color) = (0.95, 0.10, 0.30, 1)
        _DangerColor1 ("Danger Color 1", Color) = (1.00, 0.20, 0.55, 1)
        _DangerColor2 ("Danger Color 2", Color) = (0.75, 0.05, 0.20, 1)
        _DangerColor3 ("Danger Color 3", Color) = (1.00, 0.45, 0.60, 1)
        _DangerColor4 ("Danger Color 4", Color) = (1.00, 0.10, 0.10, 1)
        _DangerColor5 ("Danger Color 5", Color) = (0.85, 0.15, 0.45, 1)
        _GlitchRate ("Glitch Rate (per sec)", Float) = 25
        _GlitchBands ("Glitch Bands", Float) = 24
        _GlitchShift ("Glitch Shift (UV)", Float) = 0.08
        _RgbSplit ("RGB Split (UV)", Float) = 0.01

        [Header(Selected Frame)]
        _FrameColorA ("Frame Color A", Color) = (0.70, 0.92, 1, 1)
        _FrameColorB ("Frame Color B", Color) = (0.92, 0.99, 1, 1)
        _FrameWidth ("Frame Width (UV)", Float) = 0.04
        _BlinkSpeed ("Blink Speed", Float) = 6
        _Brighten ("Brighten", Range(0, 1)) = 0.2

        [Header(Glow)]
        _GlowWidth ("Glow Width (UV)", Range(0, 0.3)) = 0.12
        _GlowIntensity ("Glow Intensity", Range(0, 4)) = 1.5
        _GlowPulse ("Glow Pulse (0 = steady)", Range(0, 1)) = 0.3

        // UI標準のStencil/クリップ設定（Mask・RectMask2D対応）
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;

            float _Danger;
            float _Selected;

            fixed4 _DangerColor0;
            fixed4 _DangerColor1;
            fixed4 _DangerColor2;
            fixed4 _DangerColor3;
            fixed4 _DangerColor4;
            fixed4 _DangerColor5;
            float _GlitchRate;
            float _GlitchBands;
            float _GlitchShift;
            float _RgbSplit;

            fixed4 _FrameColorA;
            fixed4 _FrameColorB;
            float _FrameWidth;
            float _BlinkSpeed;
            float _Brighten;

            float _GlowWidth;
            float _GlowIntensity;
            float _GlowPulse;

            static const float2 GLOW_DIRS[8] =
            {
                float2( 1.0000,  0.0000),
                float2( 0.7071,  0.7071),
                float2( 0.0000,  1.0000),
                float2(-0.7071,  0.7071),
                float2(-1.0000,  0.0000),
                float2(-0.7071, -0.7071),
                float2( 0.0000, -1.0000),
                float2( 0.7071, -0.7071)
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            float hash11(float n)
            {
                return frac(sin(n * 127.1) * 43758.5453);
            }

          float3 PickDangerColor(float rnd)
{
    float idx = floor(rnd * 6.0);

    if (idx < 1.0) return _DangerColor0.rgb;
    if (idx < 2.0) return _DangerColor1.rgb;
    if (idx < 3.0) return _DangerColor2.rgb;
    if (idx < 4.0) return _DangerColor3.rgb;
    if (idx < 5.0) return _DangerColor4.rgb;
    return _DangerColor5.rgb;
}

            // 図形の周囲のアルファを2重のリングでサンプリングし、光の強さ(0〜1)を返す
            float SampleGlow(float2 uv, float w)
            {
                float inner = 0.0;
                float outer = 0.0;

                [unroll]
                for (int j = 0; j < 8; j++)
                {
                    inner += tex2D(_MainTex, uv + GLOW_DIRS[j] * w * 0.5).a;
                    outer += tex2D(_MainTex, uv + GLOW_DIRS[j] * w).a;
                }

                return (inner * 1.0 + outer * 0.6) / (8.0 * 1.6);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                fixed4 baseCol = tex2D(_MainTex, uv) * i.color;
                float3 rgb = baseCol.rgb;
                float a = baseCol.a;
                float baseAlpha = baseCol.a;

                float3 glowColor = float3(0.0, 0.0, 0.0);
                float glowOn = 0.0;

                // 点滅（0〜1を往復）
                float k = abs(frac(_Time.y * _BlinkSpeed * 0.5) * 2.0 - 1.0);
                float3 fc = lerp(_FrameColorA.rgb, _FrameColorB.rgb, k);

                if (_Danger > 0.5)
                {
                    // ---------- 危険：赤系グリッチ ----------
                    float stepId = floor(_Time.y * _GlitchRate);
                    float band = floor(uv.y * _GlitchBands);

                    // 一部の横帯だけ左右にずらす
                    float r = hash11(band + stepId * 17.0);
                    float shift = 0.0;

                    if (r > 0.7)
                    {
                        shift = (hash11(band * 3.1 + stepId) - 0.5) * 2.0 * _GlitchShift;
                    }

                    float2 guv = uv + float2(shift, 0.0);

                    // RGBずらし
                    fixed4 sr = tex2D(_MainTex, guv + float2(_RgbSplit, 0.0));
                    fixed4 sg = tex2D(_MainTex, guv);
                    fixed4 sb = tex2D(_MainTex, guv - float2(_RgbSplit, 0.0));

                    float3 dc = PickDangerColor(hash11(stepId * 7.3));

                    a = max(sr.a, max(sg.a, sb.a)) * i.color.a;

                    rgb = dc * float3(sr.a, sg.a, sb.a);

                    // 帯ごとの明るさのばらつき
                    rgb *= 0.8 + 0.4 * hash11(band + stepId);

                    // たまに明るい筋を入れる
                    float streak = step(0.9, hash11(band * 5.7 + stepId));
                    rgb += streak * 0.35 * a;

                    // 危険方向の光は赤
                    glowColor = dc;
                    glowOn = 1.0;
                }
                else if (_Selected > 0.5)
                {
                    // ---------- 選択中（安全）：図形を少し明るく ----------
                    rgb = lerp(rgb, float3(1.0, 1.0, 1.0), _Brighten);

                    // 選択方向の光は水色
                    glowColor = fc;
                    glowOn = 1.0;
                }

                // ---------- 光（グロー） ----------
                if (glowOn > 0.5 && _GlowWidth > 0.0001 && _GlowIntensity > 0.0)
                {
                    float g = SampleGlow(uv, _GlowWidth);

                    // 光をゆっくり脈打たせる
                    float pulse = 1.0 - _GlowPulse * (1.0 - k);

                    // 図形の外側だけに光を乗せる
                    float ga = saturate(g * _GlowIntensity * pulse) * (1.0 - a);

                    float outA = a + ga;

                    rgb = (rgb * a + glowColor * ga) / max(outA, 0.0001);
                    a = outA;
                }

                if (_Selected > 0.5)
                {
                    // ---------- 選択枠線（水色、点滅） ----------
                    float w = _FrameWidth;
                    float n = 0.0;

                    n = max(n, tex2D(_MainTex, uv + float2( w, 0)).a);
                    n = max(n, tex2D(_MainTex, uv + float2(-w, 0)).a);
                    n = max(n, tex2D(_MainTex, uv + float2(0,  w)).a);
                    n = max(n, tex2D(_MainTex, uv + float2(0, -w)).a);
                    n = max(n, tex2D(_MainTex, uv + float2( w,  w) * 0.7071).a);
                    n = max(n, tex2D(_MainTex, uv + float2(-w,  w) * 0.7071).a);
                    n = max(n, tex2D(_MainTex, uv + float2( w, -w) * 0.7071).a);
                    n = max(n, tex2D(_MainTex, uv + float2(-w, -w) * 0.7071).a);

                    // 図形の外側で、すぐ隣に図形がある部分が枠線
                    float edge = saturate(n - baseAlpha);

                    rgb = lerp(rgb, fc, edge);
                    a = max(a, edge);
                }

                fixed4 color = fixed4(rgb, a);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
