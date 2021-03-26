Shader "CS_Consume"
{
	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalRenderPipeline"
		}

		Pass
		{
			Tags
			{
				"LightMode" = "UniversalForward"
				"Queue" = "Geometry"
				"RenderType" = "Opaque"
			}

			ZTest Always
			Cull Off
			ZWrite Off

			HLSLPROGRAM
			#pragma target 5.0

			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			StructuredBuffer<float3> _Buffer;
			float3 _Color;

			struct VStoFS
			{
				float4 positionCS	: SV_POSITION;
			};

			VStoFS vert(uint id : SV_VertexID)
			{
				VStoFS OUT;
				ZERO_INITIALIZE(VStoFS, OUT);

				OUT.positionCS = TransformObjectToHClip(_Buffer[id]);
				return OUT;
			}
			
			half4 frag(VStoFS IN) : SV_Target
			{
				return half4(_Color, 1);
			}
			ENDHLSL
		}
	}
}
