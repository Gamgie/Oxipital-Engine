// Draws one screen-aligned quad per splat, reading precomputed screen position / conic /
// radius / color from OxiSplatCompute.compute's output buffers, in the order produced by
// its bitonic sort (back-to-front, so plain alpha blending composites correctly).
//
// This is a normal HDRP transparent-queue shader on a normal draw call - not an HDRP
// CustomPass - specifically so depth testing against the rest of the scene, and compositing
// order relative to any post-process (like the dome's fisheye warp, which runs after all
// normal camera rendering), both fall out of Unity's ordinary rendering order instead of
// depending on CustomPass injection-point timing.
Shader "Oxipital/OxiSplatRender"
{
    SubShader
    {
        Tags { "RenderPipeline" = "HighDefinitionRenderPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }

        Pass
        {
            Name "OxiSplatRender"
            ZWrite Off
            ZTest LEqual
            Cull Off
            Blend One OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            struct ViewData
            {
                float2 screenPos;
                float3 conic;
                float2 radius;
                float4 color;
                float4 clipPos;
            };

            StructuredBuffer<uint> _SortedIndices;
            StructuredBuffer<ViewData> _ViewData;
            float2 _ScreenSize;

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
                float3 conic : TEXCOORD0;
                noperspective float2 pixelOffset : TEXCOORD1;
            };

            static const float2 kCorners[4] = {
                float2(-1, -1), float2(1, -1), float2(-1, 1), float2(1, 1)
            };

            v2f vert(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
            {
                v2f o = (v2f)0;

                uint splatIndex = _SortedIndices[instanceID];
                ViewData vd = _ViewData[splatIndex];

                if (vd.color.a <= 0 || vd.radius.x <= 0 || vd.clipPos.w <= 0)
                {
                    o.pos = float4(2, 2, 2, 1); // push off-screen, cheaper than a branch-out
                    return o;
                }

                float2 corner = kCorners[vertexID];
                float2 pixelOffset = corner * vd.radius;
                float2 cornerScreenPos = vd.screenPos + pixelOffset;

                float2 ndc = (cornerScreenPos / _ScreenSize) * 2.0 - 1.0;
                o.pos = float4(ndc.xy * vd.clipPos.w, vd.clipPos.z, vd.clipPos.w);

                o.color = vd.color;
                o.conic = vd.conic;
                o.pixelOffset = pixelOffset;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 d = i.pixelOffset;
                float power = -0.5 * (i.conic.x * d.x * d.x + i.conic.z * d.y * d.y) - i.conic.y * d.x * d.y;
                if (power > 0)
                    discard;

                float alpha = saturate(i.color.a * exp(power));
                if (alpha < 1.0 / 255.0)
                    discard;

                return float4(i.color.rgb * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
