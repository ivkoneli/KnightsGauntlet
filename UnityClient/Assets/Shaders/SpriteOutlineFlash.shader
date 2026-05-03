Shader "Custom/SpriteOutlineFlash"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color            ("Tint",             Color)       = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float)    = 0
        _OutlineColor     ("Outline Color",    Color)       = (1,1,1,1)
        _OutlineEnabled   ("Outline Enabled",  Float)       = 0
        _OutlineThickness ("Outline Thickness",Float)       = 1
        _FlashAmount      ("Flash Amount",     Range(0,1))  = 0
        _FlashColor       ("Flash Color",      Color)       = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _MainTex_TexelSize;   // (1/w, 1/h, w, h)
            fixed4    _Color;
            fixed4    _OutlineColor;
            float     _OutlineEnabled;
            float     _OutlineThickness;
            float     _FlashAmount;
            fixed4    _FlashColor;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color    = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Outline: paint transparent border pixels that touch an opaque neighbour
                if (_OutlineEnabled > 0.5 && c.a < 0.1)
                {
                    float ox = _OutlineThickness * _MainTex_TexelSize.x;
                    float oy = _OutlineThickness * _MainTex_TexelSize.y;
                    float n  = tex2D(_MainTex, IN.texcoord + float2( ox,  0)).a
                             + tex2D(_MainTex, IN.texcoord + float2(-ox,  0)).a
                             + tex2D(_MainTex, IN.texcoord + float2(  0, oy)).a
                             + tex2D(_MainTex, IN.texcoord + float2(  0,-oy)).a;
                    if (n > 0.0)
                    {
                        c   = _OutlineColor;
                        c.a = saturate(n);
                    }
                }

                // Flash to _FlashColor — only affects pixels that have alpha
                c.rgb = lerp(c.rgb, _FlashColor.rgb, _FlashAmount * step(0.01, c.a));

                // Premultiply alpha (required for Blend One OneMinusSrcAlpha)
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}
