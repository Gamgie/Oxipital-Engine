// Self-contained cube-to-fisheye warp for OxiSplatCubeCapture's splat-only cubemap, so it no
// longer depends on com.pfc.dome-tools' CubeToDome2 Shader Graph (whose exact pass index for
// HDRP and auto-generated property reference names proved impossible to get right blind, two
// attempts running). Standard equidistant fisheye / domemaster projection: output pixel
// distance from the image center maps linearly to angle away from "forward" (domeAngle at the
// very edge), azimuth around the center maps to azimuth around forward. This is the same
// convention effectively all dome-master/planetarium content uses.
Shader "Hidden/OxiSplat/CubeToDome"
{
    Properties { _CubeTex ("Cubemap", CUBE) = "" {} }
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off
            Blend One Zero

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            samplerCUBE _CubeTex;
            float _AngleDegrees; // full angular field of view mapped across the circle, e.g. 180
            float4x4 _DomeWorldTransform; // rotation: local +Z = "forward" = image center

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv * 2.0 - 1.0;
                float r = length(p);
                if (r > 1.0)
                    return fixed4(0, 0, 0, 0);

                float azimuth = atan2(p.y, p.x);
                float halfFovRad = radians(_AngleDegrees) * 0.5;
                float theta = r * halfFovRad; // angle away from forward, 0 at center

                // Direction in a local frame where +Z is "forward" (image center) and the
                // circle's edge is halfFovRad away from it, azimuth measured around +Z.
                float3 localDir = float3(sin(theta) * cos(azimuth), sin(theta) * sin(azimuth), cos(theta));
                float3 dir = mul((float3x3)_DomeWorldTransform, localDir);

                fixed4 col = texCUBE(_CubeTex, dir);
                return col;
            }
            ENDCG
        }
    }
}
