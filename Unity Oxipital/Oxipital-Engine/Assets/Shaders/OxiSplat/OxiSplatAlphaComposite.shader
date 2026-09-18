// Alpha-blends a source texture (the splat cubemap's fisheye warp, via a plain Blit) on top
// of whatever is already in the destination render target - used by OxiSplatCubeCapture to
// composite splats over the dome's already-rendered opaque/other-geometry output without
// overwriting it (a default Blit copies pixels directly, ignoring alpha).
Shader "Hidden/OxiSplat/AlphaComposite"
{
    Properties { _MainTex ("Texture", 2D) = "black" {} }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
