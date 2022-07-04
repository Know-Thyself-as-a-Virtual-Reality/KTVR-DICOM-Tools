// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Ray Casting" {
	
	Properties {
		[NoScaleOffset] _Data ("Data Texture", 3D) = "" {}
		_ScalarMin ("Scalar threshold: min", Range(0,1)) = 0.
		_ScalarMax ("Scalar threshold: max", Range(0,1)) = 1.

	}

	SubShader {

		Pass {
			Cull Off
			ZTest Always
			ZWrite Off
			Fog { Mode off }

			CGPROGRAM
	        #pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler3D _Data;
			//float _ClipDim1Min, _ClipDim1Max;
			//float _ClipDim2Min, _ClipDim2Max;
			//float _ClipDim3Min, _ClipDim3Max;
			float _ScalarMin, _ScalarMax;
			float _Brightness;
			float4 _ClipDimMin, _ClipDimMax;

			bool IntersectBox(float3 r_org, float3 r_dir, float3 boxMin, float3 boxMax, out float tNear, out float tFar)
			{

			    float3 invR = 1.0 / r_dir;
			    float3 tBot = invR * (boxMin.xyz - r_org);
			    float3 tTop = invR * (boxMax.xyz - r_org);

			    float3 tMin = min (tTop, tBot);
			    float3 tMax = max (tTop, tBot);

			    float2 t0 = max (tMin.xx, tMin.yz);
			    float largest_tMin = max (t0.x, t0.y);
			    t0 = min (tMax.xx, tMax.yz);
			    float smallest_tMax = min (t0.x, t0.y);

			    bool hit = (largest_tMin <= smallest_tMax);
			    tNear = largest_tMin;
			    tFar = smallest_tMax;
			    return hit;
			}

			struct vert_input {
			    float4 pos : POSITION;
			};

			struct frag_input {
			    float4 pos : SV_POSITION;
			    float3 r_org : TEXCOORD1;
			    float3 r_dir : TEXCOORD2;
			};


			frag_input vert(vert_input i)
			{
				frag_input o;

				o.r_dir = -ObjSpaceViewDir(i.pos);
				o.r_org = i.pos.xyz - o.r_dir;

				o.pos = UnityObjectToClipPos(i.pos);

				return o;
			}


			float4 get_data(float3 pos) {

				float3 pos_righthanded = float3(pos.x,pos.z,pos.y);

				float4 data_ = tex3Dlod(_Data, float4(pos_righthanded,0));

				float data = step(_ClipDimMin.x, pos.x);
				data *= step(_ClipDimMin.y, pos.y);
				data *= step(_ClipDimMin.z, pos.z);
				data *= step(pos.x, _ClipDimMax.x);
				data *= step(pos.y, _ClipDimMax.y);
				data *= step(pos.z, _ClipDimMax.z);
				data *= step(_ScalarMin, data);
				data *= step(data, _ScalarMax);

				data_.a *= saturate(data);

				float4 col = float4(data_.rgb, data_.a);
				return col;
			}

#define FRONT_TO_BACK 
#define STEP_CNT 512 
			
			float4 frag(frag_input i) : COLOR
			{
			    i.r_dir = normalize(i.r_dir);

				float3 boxMin = { -0.5, -0.5, -0.5 };
				float3 boxMax = {  0.5,  0.5,  0.5 };

			    float tNear, tFar;
			    bool hit = IntersectBox(i.r_org, i.r_dir, boxMin, boxMax, tNear, tFar);

			    if (!hit) discard;
			    if (tNear < 0.0) tNear = 0.0;

			    float3 pNear = i.r_org + i.r_dir*tNear;
			    float3 pFar  = i.r_org + i.r_dir*tFar;

				pNear = pNear + 0.5;
				pFar  = pFar  + 0.5;
				
#ifdef FRONT_TO_BACK
				float3 r_pos = pNear;
				float3 r_dir_ = pFar - pNear;
#else
				float3 r_pos = pFar;
				float3 r_dir_ = pNear - pFar;
#endif
				float3 r_step = normalize(r_dir_) * sqrt(3) / STEP_CNT;
				float4 r_col = 0;
				for(int k = 0; k < STEP_CNT; k++)
				{
					float4 voxel_col = get_data(r_pos);
#ifdef FRONT_TO_BACK
					//r_col.rgb = r_col.rgb + (1 - r_col.a) * voxel_col.a * voxel_col.rgb;
					r_col.rgb += (1 - r_col.a) * voxel_col.a * voxel_col.rgb;
					r_col.a   += (1 - r_col.a) * voxel_col.a;
#else
			        r_col = (1-voxel_col.a)*r_col + voxel_col.a*voxel_col;
#endif
					r_pos += r_step;
					if (r_pos.x < 0 || r_pos.y < 0 || r_pos.z < 0) break;
					if (r_pos.x > 1 || r_pos.y > 1 || r_pos.z > 1) break;
				}
		    	return r_col*_Brightness;
			}

			ENDCG

		}

	}

	FallBack Off
}