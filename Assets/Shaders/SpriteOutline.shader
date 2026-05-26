Shader "Sprites/Outline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineSize ("Outline Size", Range(0, 10)) = 1
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif

                return OUT;
            }

            sampler2D _MainTex;
            sampler2D _AlphaTex;
            float4 _MainTex_TexelSize;

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord);

                // Si el pixel tiene alfa, renderizar normal
                if (c.a > 0.01)
                {
                    c.rgb *= c.a;
                    return c * IN.color;
                }

                // Si no tiene alfa, buscar vecinos para outline
                float outline = 0.0;
                float pixelSize = _OutlineSize * 0.001;

                // Samplear en 8 direcciones
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0) continue;

                        float2 offset = float2(x, y) * _MainTex_TexelSize.xy * _OutlineSize;
                        fixed4 neighbor = tex2D(_MainTex, IN.texcoord + offset);

                        if (neighbor.a > 0.01)
                        {
                            outline = 1.0;
                        }
                    }
                }

                // Renderizar outline si hay vecinos con alpha
                if (outline > 0.0)
                {
                    fixed4 outlineColor = _OutlineColor;
                    outlineColor.rgb *= outlineColor.a;
                    return outlineColor;
                }

                // Pixel vacío
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
