// Made with Amplify Shader Editor v1.9.6.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FX12_A2_CarPaint"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		_CarPaintColor("CarPaintColor", Color) = (1,1,1,0)
		_CarPaintHSV_R("CarPaintHSV_R", Range( 0 , 1)) = 0
		_Contrast("Contrast", Range( 0.25 , 1)) = 0.5
		_MDarkScale("DarkScale", Range( 0 , 1)) = 1
		_TopDarkScale("TopDarkScale", Range( 0 , 0.5)) = 0
		_DarkOffset1("DarkOffset", Range( 0 , 0.7)) = 0.6
		_Emergence("Emergence", Range( 0.1 , 0.3)) = 0.15
		_ShadowColor("ShadowColor", Color) = (0,0,0,0)
		_MatteScale("MatteScale", Range( 0 , 5)) = 1
		_AuxLitScale("AuxLitScale", Range( 0 , 2)) = 0
		_GISaturation("MatteSaturation", Range( 0.5 , 0.95)) = 0.75
		[Toggle]_Coat_IO("Coat_IO", Float) = 0
		_CoatScale("CoatScale", Range( 0 , 5)) = 0.5
		_CoatSaturation("CoatSaturation", Range( 0.5 , 0.9)) = 0.75
		_Horizontal("Horizontal", Range( 0 , 360)) = 0
		_SunScale("SunScale", Range( 0 , 5)) = 0
		_EnvironmentColor("EnvironmentColor", Color) = (1,1,1,0)
		[NoScaleOffset]_HDRLitTex("HDRLitTex", CUBE) = "white" {}
		[Toggle(_OCCLUSIONIO_ON)] _OcclusionIO("OcclusionIO", Float) = 0
		_OcclusionScale("OcclusionScale", Range( 0 , 1)) = 0
		_SunShadow("SunShadow", Range( 0 , 0.9)) = 0
		[NoScaleOffset]_OcclusionTex("OcclusionTex", 2D) = "white" {}
		[Toggle(_NORMALIOREF_ON)] _NormalIORef("NormalIORef", Float) = 0
		[Toggle(_NORMALIOLIT_ON)] _NormalIOLit("NormalIOLit", Float) = 0
		_NormalScale("NormalScale", Range( 0 , 1)) = 0
		[NoScaleOffset]_NormalTex("NormalTex", 2D) = "bump" {}
		[Toggle]_NoiseIO("NoiseIO", Float) = 0
		_NoiseScale("NoiseScale", Range( 0 , 100)) = 1
		_NosieTiling("NosieTiling", Range( 1 , 30)) = 10
		[NoScaleOffset]_NosieTex("NosieTex", 2D) = "black" {}
		[Toggle]_SecrecyIO("SecrecyIO", Float) = 0
		_SecrecyTex("SecrecyTex", 2D) = "white" {}
		_FlowTex("FlowTex", 2D) = "white" {}
		[Toggle]_FlowToggle("FlowToggle", Range( 0 , 1)) = 0
		_FlowTilling("FlowTilling", Vector) = (1,1,0,0)
		_FlowSpeed("FlowSpeed", Vector) = (0,0,0,0)
		_FlowEmission("FlowEmission", Float) = 1
		_FlowColor("FlowColor", Color) = (0,0,0,0)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}


		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25

		[HideInInspector] _QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector] _QueueControl("_QueueControl", Float) = -1

        [HideInInspector][NoScaleOffset] unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset] unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}

		[HideInInspector][ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" "UniversalMaterialType"="Unlit" }

		Cull Back
		AlphaToMask Off

		

		HLSLINCLUDE
		#pragma target 4.5
		#pragma prefer_hlslcc gles
		// ensure rendering platforms toggle list is visible

		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}

		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
							(( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS
		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForwardOnly" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			

			HLSLPROGRAM

			
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #define ASE_SRP_VERSION 140100


			
            #pragma multi_compile _ DOTS_INSTANCING_ON
		

			#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3

			
            #pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS
		

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
			#pragma multi_compile_fragment _ DEBUG_DISPLAY

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_UNLIT

			

			

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			

			
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Debug/Debugging3D.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceData.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_COLOR
			#pragma shader_feature_local _NORMALIOLIT_ON
			#pragma shader_feature_local _NORMALIOREF_ON
			#pragma shader_feature_local _OCCLUSIONIO_ON


			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					float4 shadowCoord : TEXCOORD2;
				#endif
				#ifdef ASE_FOG
					float fogFactor : TEXCOORD3;
				#endif
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				float4 ase_texcoord6 : TEXCOORD6;
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _CarPaintColor;
			float4 _SecrecyTex_ST;
			float4 _EnvironmentColor;
			float4 _ShadowColor;
			float4 _FlowColor;
			float2 _FlowTilling;
			float2 _FlowSpeed;
			float _FlowEmission;
			float _CarPaintHSV_R;
			float _SecrecyIO;
			float _OcclusionScale;
			float _TopDarkScale;
			float _MDarkScale;
			float _Emergence;
			float _DarkOffset1;
			float _NoiseIO;
			float _NoiseScale;
			float _Coat_IO;
			float _CoatSaturation;
			float _SunScale;
			float _CoatScale;
			float _GISaturation;
			float _MatteScale;
			float _AuxLitScale;
			float _NormalScale;
			float _Horizontal;
			float _SunShadow;
			float _Contrast;
			float _NosieTiling;
			float _FlowToggle;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			sampler2D _OcclusionTex;
			samplerCUBE _HDRLitTex;
			sampler2D _NormalTex;
			sampler2D _NosieTex;
			sampler2D _SecrecyTex;
			sampler2D _FlowTex;


			float3 HSVToRGB( float3 c )
			{
				float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
				float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
				return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
			}
			
			float3 RGBToHSV(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
				float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
				float d = q.x - min( q.w, q.y );
				float e = 1.0e-10;
				return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldNormal = TransformObjectToWorldNormal(v.normalOS);
				o.ase_texcoord4.xyz = ase_worldNormal;
				float3 ase_worldTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				o.ase_texcoord6.xyz = ase_worldTangent;
				float ase_vertexTangentSign = v.ase_tangent.w * ( unity_WorldTransformParams.w >= 0.0 ? 1.0 : -1.0 );
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord7.xyz = ase_worldBitangent;
				
				o.ase_texcoord5.xy = v.ase_texcoord.xy;
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.zw = 0;
				o.ase_texcoord6.w = 0;
				o.ase_texcoord7.w = 0;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#ifdef ASE_FOG
					o.fogFactor = ComputeFogFactor( vertexInput.positionCS.z );
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				float4 ase_color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_tangent = v.ase_tangent;
				o.ase_color = v.ase_color;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.ase_color = patch[0].ase_color * bary.x + patch[1].ase_color * bary.y + patch[2].ase_color * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag ( VertexOutput IN
				#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
				#endif
				 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float3 hsvTorgb789 = RGBToHSV( _CarPaintColor.rgb );
				float Hue887 = hsvTorgb789.x;
				float Saturation888 = hsvTorgb789.y;
				float Value889 = hsvTorgb789.z;
				float temp_output_983_0 = (0.01 + (Value889 - 0.0) * (0.8 - 0.01) / (1.0 - 0.0));
				float smoothstepResult1217 = smoothstep( 0.0 , 1.0 , (0.15 + (( (0.0 + (Value889 - 0.0) * (1.2 - 0.0) / (1.0 - 0.0)) + Saturation888 ) - 0.0) * (1.0 - 0.15) / (2.0 - 0.0)));
				float LitValueControl990 = smoothstepResult1217;
				float3 hsvTorgb1057 = HSVToRGB( float3(Hue887,( Saturation888 * 0.95 ),( temp_output_983_0 * temp_output_983_0 * _Contrast * LitValueControl990 )) );
				float3 DarkColor600 = hsvTorgb1057;
				float3 hsvTorgb861 = HSVToRGB( float3(Hue887,Saturation888,temp_output_983_0) );
				float3 PaintColor797 = hsvTorgb861;
				float3 ase_worldNormal = IN.ase_texcoord4.xyz;
				float2 uv_OcclusionTex381 = IN.ase_texcoord5.xy;
				float4 tex2DNode381 = tex2D( _OcclusionTex, uv_OcclusionTex381 );
				float lerpResult1233 = lerp( 1.0 , tex2DNode381.g , _SunShadow);
				float SunShadow1245 = lerpResult1233;
				float3 lerpResult1100 = lerp( DarkColor600 , PaintColor797 , ( ( (ase_worldNormal.y*0.5 + 0.5) + 0.0 ) * SunShadow1245 ));
				float3 normalizedWorldNormal = normalize( ase_worldNormal );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = SafeNormalize( ase_worldViewDir );
				float dotResult346 = dot( normalizedWorldNormal , ase_worldViewDir );
				float DiyFresnelA343 = ( 1.0 - dotResult346 );
				float saferPower348 = abs( DiyFresnelA343 );
				float lerpResult432 = lerp( 1.0 , 0.0 , ( pow( saferPower348 , 2.0 ) * 0.5 ));
				float AlbedoColor433 = lerpResult432;
				float temp_output_745_0 = cos( ( ( _Horizontal / 180.0 ) * PI ) );
				float temp_output_749_0 = sin( ( ( _Horizontal / 180.0 ) * PI ) );
				float3 appendResult750 = (float3(temp_output_745_0 , 0.0 , temp_output_749_0));
				float3 appendResult746 = (float3(0.0 , 1.0 , 0.0));
				float3 appendResult747 = (float3(( temp_output_749_0 * -1.0 ) , 0.0 , temp_output_745_0));
				float2 uv_NormalTex380 = IN.ase_texcoord5.xy;
				float3 unpack380 = UnpackNormalScale( tex2D( _NormalTex, uv_NormalTex380 ), _NormalScale );
				unpack380.z = lerp( 1, unpack380.z, saturate(_NormalScale) );
				float3 tex2DNode380 = unpack380;
				float3 Normal375 = tex2DNode380;
				float3 ase_worldTangent = IN.ase_texcoord6.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord7.xyz;
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 tanNormal1075 = Normal375;
				float3 worldNormal1075 = float3(dot(tanToWorld0,tanNormal1075), dot(tanToWorld1,tanNormal1075), dot(tanToWorld2,tanNormal1075));
				#ifdef _NORMALIOLIT_ON
				float3 staticSwitch1078 = worldNormal1075;
				#else
				float3 staticSwitch1078 = ase_worldNormal;
				#endif
				float3 NormalSwitchLit1079 = staticSwitch1078;
				float3 RotatorLit754 = mul( float3x3(appendResult750, appendResult746, appendResult747), NormalSwitchLit1079 );
				float4 texCUBENode1072 = texCUBE( _HDRLitTex, RotatorLit754 );
				float temp_output_363_0 = cos( ( ( _Horizontal / 180.0 ) * PI ) );
				float temp_output_367_0 = sin( ( ( _Horizontal / 180.0 ) * PI ) );
				float3 appendResult368 = (float3(temp_output_363_0 , 0.0 , temp_output_367_0));
				float3 appendResult364 = (float3(0.0 , 1.0 , 0.0));
				float3 appendResult365 = (float3(( temp_output_367_0 * -1.0 ) , 0.0 , temp_output_363_0));
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 ase_worldReflection = reflect(-ase_worldViewDir, ase_worldNormal);
				float3 normalizedWorldRefl = normalize( ase_worldReflection );
				float3 worldRefl829 = normalize( reflect( -ase_worldViewDir, float3( dot( tanToWorld0, Normal375 ), dot( tanToWorld1, Normal375 ), dot( tanToWorld2, Normal375 ) ) ) );
				#ifdef _NORMALIOREF_ON
				float3 staticSwitch826 = worldRefl829;
				#else
				float3 staticSwitch826 = normalizedWorldRefl;
				#endif
				float3 NormalSwitchRef830 = staticSwitch826;
				float3 RotatorRef374 = mul( float3x3(appendResult368, appendResult364, appendResult365), NormalSwitchRef830 );
				float4 texCUBENode1071 = texCUBE( _HDRLitTex, RotatorRef374 );
				float3 hsvTorgb790 = HSVToRGB( float3(Hue887,( Saturation888 * _GISaturation ),1.0) );
				float3 LitColor773 = hsvTorgb790;
				float Gamma1181 = 0.25;
				float3 hsvTorgb806 = HSVToRGB( float3(Hue887,( Saturation888 * _CoatSaturation ),1.0) );
				float3 CubeColor808 = hsvTorgb806;
				float3 lerpResult1159 = lerp( float3( 0,0,0 ) , ( ( ( texCUBENode1071.g * _CoatScale * Gamma1181 ) + ( texCUBENode1071.b * _SunScale * Gamma1181 ) ) * CubeColor808 * IN.ase_color.r * SunShadow1245 ) , _Coat_IO);
				float2 temp_cast_0 = (_NosieTiling).xx;
				float2 texCoord385 = IN.ase_texcoord5.xy * temp_cast_0 + float2( 0,0 );
				float NoiseMask1203 = ( texCUBENode1071.r + texCUBENode1071.r );
				float3 hsvTorgb1139 = HSVToRGB( float3(Hue887,Saturation888,1.0) );
				float3 NoiseColor1140 = hsvTorgb1139;
				float DiyFresnelB342 = dotResult346;
				float3 lerpResult1157 = lerp( float3( 0,0,0 ) , ( _NoiseScale * tex2D( _NosieTex, texCoord385 ).r * IN.ase_color.r * NoiseMask1203 * NoiseColor1140 * DiyFresnelB342 * LitValueControl990 * SunShadow1245 ) , _NoiseIO);
				float3 Noise391 = lerpResult1157;
				float dotResult998 = dot( float3(0,1,0) , normalizedWorldNormal );
				float DotNor1004 = (dotResult998*0.5 + 0.5);
				float smoothstepResult1009 = smoothstep( ( _DarkOffset1 - _Emergence ) , ( _DarkOffset1 + _Emergence ) , ( 1.0 - DotNor1004 ));
				float smoothstepResult1014 = smoothstep( ( 0.85 - 0.2 ) , ( 0.85 + 0.2 ) , DotNor1004);
				float bbbb1252 = tex2DNode381.b;
				float DarkMask573 = ( ( ( smoothstepResult1009 * _MDarkScale ) + ( smoothstepResult1014 * _TopDarkScale ) ) - bbbb1252 );
				float3 lerpResult1124 = lerp( ( ( lerpResult1100 * AlbedoColor433 ) + ( ( ( texCUBENode1072.r * _AuxLitScale * IN.ase_color.r * SunShadow1245 ) + texCUBENode1071.r ) * _MatteScale * LitColor773 * Gamma1181 * LitValueControl990 * SunShadow1245 ) + lerpResult1159 + Noise391 ) , _ShadowColor.rgb , DarkMask573);
				float lerpResult378 = lerp( 1.0 , tex2DNode381.r , _OcclusionScale);
				#ifdef _OCCLUSIONIO_ON
				float staticSwitch867 = lerpResult378;
				#else
				float staticSwitch867 = 1.0;
				#endif
				float Occlusion382 = staticSwitch867;
				float3 base1094 = ( lerpResult1124 * Occlusion382 * _EnvironmentColor.rgb );
				float2 uv_SecrecyTex = IN.ase_texcoord5.xy * _SecrecyTex_ST.xy + _SecrecyTex_ST.zw;
				float3 Secrecy1276 = ( (tex2D( _SecrecyTex, uv_SecrecyTex )).rgb * AlbedoColor433 );
				float3 lerpResult837 = lerp( base1094 , Secrecy1276 , _SecrecyIO);
				float3 hsvTorgb1284 = HSVToRGB( float3(0.0,1.0,(0.5 + (_CarPaintHSV_R - 0.0) * (1.0 - 0.5) / (1.0 - 0.0))) );
				float3 objToWorld1291 = mul( GetObjectToWorldMatrix(), float4( float3( 0,0,0 ), 1 ) ).xyz;
				float2 panner1296 = ( 1.0 * _Time.y * _FlowSpeed + ( (( WorldPosition - objToWorld1291 )).xz * _FlowTilling ));
				float4 tex2DNode1299 = tex2D( _FlowTex, panner1296 );
				float4 FlowRGB1303 = ( tex2DNode1299 * _FlowEmission * _FlowColor * _FlowToggle );
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = ( float4( ( lerpResult837 * hsvTorgb1284.x ) , 0.0 ) + FlowRGB1303 ).rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;

				#ifdef _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#if defined(_DBUFFER)
					ApplyDecalToBaseColor(IN.positionCS, Color);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4( EncodeMeshRenderingLayer( renderingLayers ), 0, 0, 0 );
				#endif

				return half4( Color, Alpha );
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask R
			AlphaToMask Off

			HLSLPROGRAM

			
            #pragma multi_compile_instancing
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #define ASE_SRP_VERSION 140100


			
            #pragma multi_compile _ DOTS_INSTANCING_ON
		

			#pragma vertex vert
			#pragma fragment frag

			

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 positionWS : TEXCOORD1;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD2;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _CarPaintColor;
			float4 _SecrecyTex_ST;
			float4 _EnvironmentColor;
			float4 _ShadowColor;
			float4 _FlowColor;
			float2 _FlowTilling;
			float2 _FlowSpeed;
			float _FlowEmission;
			float _CarPaintHSV_R;
			float _SecrecyIO;
			float _OcclusionScale;
			float _TopDarkScale;
			float _MDarkScale;
			float _Emergence;
			float _DarkOffset1;
			float _NoiseIO;
			float _NoiseScale;
			float _Coat_IO;
			float _CoatSaturation;
			float _SunScale;
			float _CoatScale;
			float _GISaturation;
			float _MatteScale;
			float _AuxLitScale;
			float _NormalScale;
			float _Horizontal;
			float _SunShadow;
			float _Contrast;
			float _NosieTiling;
			float _FlowToggle;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
					o.positionWS = vertexInput.positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.positionWS;
				#endif

				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				

				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif
				return 0;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "SceneSelectionPass"
			Tags { "LightMode"="SceneSelectionPass" }

			Cull Off
			AlphaToMask Off

			HLSLPROGRAM

			
            #define ASE_SRP_VERSION 140100


			
            #pragma multi_compile _ DOTS_INSTANCING_ON
		

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define SHADERPASS SHADERPASS_DEPTHONLY

			

			

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			

			
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _CarPaintColor;
			float4 _SecrecyTex_ST;
			float4 _EnvironmentColor;
			float4 _ShadowColor;
			float4 _FlowColor;
			float2 _FlowTilling;
			float2 _FlowSpeed;
			float _FlowEmission;
			float _CarPaintHSV_R;
			float _SecrecyIO;
			float _OcclusionScale;
			float _TopDarkScale;
			float _MDarkScale;
			float _Emergence;
			float _DarkOffset1;
			float _NoiseIO;
			float _NoiseScale;
			float _Coat_IO;
			float _CoatSaturation;
			float _SunScale;
			float _CoatScale;
			float _GISaturation;
			float _MatteScale;
			float _AuxLitScale;
			float _NormalScale;
			float _Horizontal;
			float _SunShadow;
			float _Contrast;
			float _NosieTiling;
			float _FlowToggle;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			int _ObjectId;
			int _PassValue;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );

				o.positionCS = TransformWorldToHClip(positionWS);

				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = half4(_ObjectId, _PassValue, 1.0, 1.0);
				return outColor;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "ScenePickingPass"
			Tags { "LightMode"="Picking" }

			AlphaToMask Off

			HLSLPROGRAM

			
            #define ASE_SRP_VERSION 140100


			
            #pragma multi_compile _ DOTS_INSTANCING_ON
		

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT

			#define SHADERPASS SHADERPASS_DEPTHONLY

			

			

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			

			
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

			#if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _CarPaintColor;
			float4 _SecrecyTex_ST;
			float4 _EnvironmentColor;
			float4 _ShadowColor;
			float4 _FlowColor;
			float2 _FlowTilling;
			float2 _FlowSpeed;
			float _FlowEmission;
			float _CarPaintHSV_R;
			float _SecrecyIO;
			float _OcclusionScale;
			float _TopDarkScale;
			float _MDarkScale;
			float _Emergence;
			float _DarkOffset1;
			float _NoiseIO;
			float _NoiseScale;
			float _Coat_IO;
			float _CoatSaturation;
			float _SunScale;
			float _CoatScale;
			float _GISaturation;
			float _MatteScale;
			float _AuxLitScale;
			float _NormalScale;
			float _Horizontal;
			float _SunShadow;
			float _Contrast;
			float _NosieTiling;
			float _FlowToggle;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			float4 _SelectionID;

			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				float3 positionWS = TransformObjectToWorld( v.positionOS.xyz );
				o.positionCS = TransformWorldToHClip(positionWS);
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN ) : SV_TARGET
			{
				SurfaceDescription surfaceDescription = (SurfaceDescription)0;

				

				surfaceDescription.Alpha = 1;
				surfaceDescription.AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					float alphaClipThreshold = 0.01f;
					#if ALPHA_CLIP_THRESHOLD
						alphaClipThreshold = surfaceDescription.AlphaClipThreshold;
					#endif
					clip(surfaceDescription.Alpha - alphaClipThreshold);
				#endif

				half4 outColor = 0;
				outColor = _SelectionID;

				return outColor;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormalsOnly" }

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM

			
            #pragma multi_compile_instancing
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #define ASE_SRP_VERSION 140100


			
            #pragma multi_compile _ DOTS_INSTANCING_ON
		

        	#pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

			
            #pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS
		

			#pragma vertex vert
			#pragma fragment frag

			#define ATTRIBUTES_NEED_NORMAL
			#define ATTRIBUTES_NEED_TANGENT
			#define VARYINGS_NEED_NORMAL_WS

			#define SHADERPASS SHADERPASS_DEPTHNORMALSONLY

			

			

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"

			

			
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
		

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"

            #if defined(LOD_FADE_CROSSFADE)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
            #endif

			

			struct VertexInput
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 positionCS : SV_POSITION;
				float4 clipPosV : TEXCOORD0;
				float3 normalWS : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _CarPaintColor;
			float4 _SecrecyTex_ST;
			float4 _EnvironmentColor;
			float4 _ShadowColor;
			float4 _FlowColor;
			float2 _FlowTilling;
			float2 _FlowSpeed;
			float _FlowEmission;
			float _CarPaintHSV_R;
			float _SecrecyIO;
			float _OcclusionScale;
			float _TopDarkScale;
			float _MDarkScale;
			float _Emergence;
			float _DarkOffset1;
			float _NoiseIO;
			float _NoiseScale;
			float _Coat_IO;
			float _CoatSaturation;
			float _SunScale;
			float _CoatScale;
			float _GISaturation;
			float _MatteScale;
			float _AuxLitScale;
			float _NormalScale;
			float _Horizontal;
			float _SunShadow;
			float _Contrast;
			float _NosieTiling;
			float _FlowToggle;
			#ifdef ASE_TESSELLATION
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END

			

			
			struct SurfaceDescription
			{
				float Alpha;
				float AlphaClipThreshold;
			};

			VertexOutput VertexFunction(VertexInput v  )
			{
				VertexOutput o;
				ZERO_INITIALIZE(VertexOutput, o);

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.positionOS.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif

				float3 vertexValue = defaultVertexValue;

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.positionOS.xyz = vertexValue;
				#else
					v.positionOS.xyz += vertexValue;
				#endif

				v.normalOS = v.normalOS;

				VertexPositionInputs vertexInput = GetVertexPositionInputs( v.positionOS.xyz );

				o.positionCS = vertexInput.positionCS;
				o.clipPosV = vertexInput.positionCS;
				o.normalWS = TransformObjectToWorldNormal( v.normalOS );
				return o;
			}

			#if defined(ASE_TESSELLATION)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 normalOS : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.positionOS;
				o.normalOS = v.normalOS;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
				return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.positionOS = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.normalOS = patch[0].normalOS * bary.x + patch[1].normalOS * bary.y + patch[2].normalOS * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.positionOS.xyz - patch[i].normalOS * (dot(o.positionOS.xyz, patch[i].normalOS) - dot(patch[i].vertex.xyz, patch[i].normalOS));
				float phongStrength = _TessPhongStrength;
				o.positionOS.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.positionOS.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			void frag( VertexOutput IN
				, out half4 outNormalWS : SV_Target0
			#ifdef _WRITE_RENDERING_LAYERS
				, out float4 outRenderingLayers : SV_Target1
			#endif
				 )
			{
				float4 ClipPos = IN.clipPosV;
				float4 ScreenPos = ComputeScreenPos( IN.clipPosV );

				

				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#if _ALPHATEST_ON
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODFadeCrossFade( IN.positionCS );
				#endif

				#if defined(_GBUFFER_NORMALS_OCT)
					float3 normalWS = normalize(IN.normalWS);
					float2 octNormalWS = PackNormalOctQuadEncode(normalWS);           // values between [-1, +1], must use fp32 on some platforms
					float2 remappedOctNormalWS = saturate(octNormalWS * 0.5 + 0.5);   // values between [ 0,  1]
					half3 packedNormalWS = PackFloat2To888(remappedOctNormalWS);      // values between [ 0,  1]
					outNormalWS = half4(packedNormalWS, 0.0);
				#else
					float3 normalWS = IN.normalWS;
					outNormalWS = half4(NormalizeNormalPerPixel(normalWS), 0.0);
				#endif

				#ifdef _WRITE_RENDERING_LAYERS
					uint renderingLayers = GetMeshRenderingLayer();
					outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
				#endif
			}

			ENDHLSL
		}

	
	}
	
	CustomEditor "ASEMaterialInspector"
	FallBack "Hidden/Shader Graph/FallbackError"
	
	Fallback Off
}
/*ASEBEGIN
Version=19603
Node;AmplifyShaderEditor.CommentaryNode;804;-6272,3328;Inherit;False;2284.333;3463.293;Comment;8;723;887;888;889;789;894;946;886;颜色关联;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;340;-2560,-1024;Inherit;False;1390.509;1079.399;固有色;8;433;432;347;349;348;354;350;341;固有色;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;946;-6224,3936;Inherit;False;2025.24;725.6348;Comment;6;990;1217;1215;1216;1068;1069;控制不同颜色明度下的不同灯光强度;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1187;1280,1664;Inherit;False;2700.129;1229.465;Comment;16;1114;1087;1250;973;1105;1170;1171;1174;1184;1172;1183;403;1190;1181;1180;1162;清漆部分反射;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;351;-2560,1280;Inherit;False;2429.729;1194.415;珠光漆层;16;1221;623;1141;1143;1251;654;423;387;385;390;687;386;391;425;424;1163;珠光漆层;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1289;1312,3376;Inherit;False;2246.576;797.9893;Comment;16;1299;1298;1296;1295;1294;1293;1292;1291;1288;1300;1301;1302;1303;1304;1307;1308;Flow;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1275;1280,-896;Inherit;False;804;277;Comment;2;1070;1272;环境光贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;352;-2560,-1408;Inherit;False;506;165.0001;Comment;2;356;353;采样器;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;339;-6272,-768;Inherit;False;2202.051;1041.981;法线贴图;17;1079;1078;827;830;826;829;828;375;380;379;377;1075;1081;1228;1229;1230;1231;法线贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;333;-2560,128;Inherit;False;2773.629;1035.325;反射旋转矩阵;25;372;405;374;373;370;369;368;367;366;365;364;363;754;831;1082;753;752;750;747;746;748;745;749;751;757;反射旋转矩阵;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;341;-2496,-912;Inherit;False;1234.58;473.9742;菲湦尔;6;342;345;346;343;344;462;菲湦尔;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1069;-6000,3984;Inherit;False;670.7803;297.4448;;2;1212;1218;明度越高，强度越小;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;997;-6272,368;Inherit;False;2205.377;2096.159;Comment;31;573;1254;1253;1042;1016;1014;1011;1013;1012;1037;1036;1010;1007;1017;1005;1003;1002;1030;1001;1032;1009;1015;1018;1008;1006;1026;1004;1000;998;1029;999;暗部遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;338;-6272,-2176;Inherit;False;2542.209;1323.328;AO贴图;16;382;867;868;406;378;1245;1233;1252;1237;1239;1234;1224;1226;381;1222;376;AO贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1068;-5696,4336;Inherit;False;367.772;191.4368;Comment;1;1064;饱和度越高，强度越大;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;886;-6112,5824;Inherit;False;1585.946;835.209;限制了明度的上限和下限;13;1102;1101;1120;600;797;1057;861;895;892;984;1130;983;890;车漆颜色;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;894;-6112,4752;Inherit;False;1513.934;967.4226;降低了饱和度，固定了明度;14;773;790;808;806;930;891;817;927;931;928;869;1139;1140;1164;GI和Cube颜色转换;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1188;1280,768;Inherit;False;2508.546;808.0787;Comment;14;1178;1249;1185;1182;1154;1203;1204;1135;1085;1207;1196;1071;1191;1274;底漆哑光部分反射;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1186;1280,-512;Inherit;False;1621.796;509.7427;Comment;10;1149;1148;1113;1112;1100;1104;1103;1195;1246;1247;基础的明暗;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1200;1280,128;Inherit;False;1336.178;610.3448;Comment;8;1248;1205;1198;1197;1199;1083;1072;1273;直接光补充;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1163;-976,1472;Inherit;False;436;418.8;自定义普通车漆时需要动态切换，不能用StaticSwitch;2;1157;1156;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1162;3456,1792;Inherit;False;434.667;318.8;自定义亚光车漆时需要动态切换，不能用StaticSwitch;2;1158;1159;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;801;6064,-752;Inherit;False;1514.002;526;Comment;5;762;763;835;764;1276;车模保密贴图;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;1051;-6272,2560;Inherit;False;2593.133;691.3862;Comment;15;1039;1045;1046;1044;1038;1022;1043;1020;1019;1024;1021;1023;1052;1054;1055;反光遮罩;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;1229;-5472,-592;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;999;-6176,512;Inherit;False;Constant;_Vector4;Vector 4;35;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;1029;-6176,656;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;998;-5888,512;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;1000;-5616,512;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1004;-5344,512;Inherit;False;DotNor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1072;1664,176;Inherit;True;Property;_TextureSample1;Texture Sample 1;38;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;797;-4896,5952;Inherit;False;PaintColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;403;2256,1808;Inherit;False;Property;_CoatScale;CoatScale;12;0;Create;True;1;Coat;0;0;False;0;False;0.5;5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1183;2352,1904;Inherit;False;1181;Gamma;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1172;2272,2128;Inherit;False;Property;_SunScale;SunScale;15;0;Create;True;0;0;0;False;0;False;0;5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1184;2368,2208;Inherit;False;1181;Gamma;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1174;2624,1792;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1171;2656,2064;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1103;2208,-368;Inherit;False;797;PaintColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1104;2208,-464;Inherit;False;600;DarkColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1170;2928,1888;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1087;3248,1888;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1114;2848,2096;Inherit;False;990;LitValueControl;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.StickyNoteNode;690;7936,-2176;Inherit;False;1479.85;866.5207;Unlit_CarPaint_Final v1.2;Shader 更新说明;0.5471698,0,0.005253441,1;Unlit_CarPaint_Final v1.2$$Mino车漆Shader;0;0
Node;AmplifyShaderEditor.Vector3Node;1001;-6176,800;Inherit;False;Constant;_Vector5;Vector 5;35;0;Create;True;0;0;0;False;0;False;0,-1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;1030;-6176,960;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;1002;-5888,800;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;1003;-5616,800;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1005;-5344,800;Inherit;False;DotPos;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;764;6688,-688;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;1100;2512,-464;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;763;7072,-688;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;377;-6192,-528;Inherit;False;353;SS;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.GetLocalVarNode;827;-5424,-192;Inherit;False;375;Normal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;1148;1360,-416;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ScaleAndOffsetNode;1149;1616,-368;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1195;1872,-368;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1246;1872,-160;Inherit;False;1245;SunShadow;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1247;2208,-208;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1112;2768,-464;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1248;2096,672;Inherit;False;1245;SunShadow;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;1205;2096,464;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;1198;2000,352;Inherit;False;Property;_AuxLitScale;AuxLitScale;9;0;Create;True;0;0;0;False;0;False;0;1.586;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.GammaToLinearNode;1199;2112,176;Inherit;False;0;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1197;2384,224;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1180;1536,2304;Inherit;False;Constant;_Float2;Float 2;33;0;Create;False;0;0;0;False;0;False;0.25;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1181;1952,2304;Inherit;False;Gamma;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1105;2848,2192;Inherit;False;808;CubeColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;973;2848,2288;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;1250;2848,2496;Inherit;False;1245;SunShadow;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;1190;2080,1824;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1158;3488,2016;Inherit;False;Property;_Coat_IO;Coat_IO;11;1;[Toggle];Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1159;3744,1872;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;1228;-5872,-448;Inherit;True;Property;_NormalTex2;NormalTex2;29;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;74231f2c1390d534d802dad2459c094a;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;380;-5872,-720;Inherit;True;Property;_NormalTex;NormalTex;27;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;-1;None;74231f2c1390d534d802dad2459c094a;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;1231;-6256,-400;Inherit;False;Property;_NormalScale2;NormalScale2;28;0;Create;True;1;Normal;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;379;-6256,-672;Inherit;False;Property;_NormalScale;NormalScale;26;0;Create;True;1;Normal;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;1230;-5776,-224;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;375;-5232,-720;Inherit;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldReflectionVector;829;-5168,-352;Inherit;False;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldReflectionVector;828;-5168,-544;Inherit;False;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StaticSwitch;826;-4848,-544;Inherit;False;Property;_NormalIORef;NormalIORef;24;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;1075;-5136,96;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StaticSwitch;1078;-4848,-160;Inherit;False;Property;_NormalIOLit;NormalIOLit;25;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;830;-4464,-544;Inherit;False;NormalSwitchRef;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1079;-4464,-160;Inherit;False;NormalSwitchLit;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldNormalVector;1081;-5136,-160;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;1113;2512,-224;Inherit;False;433;AlbedoColor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1222;-6000,-2112;Inherit;True;Property;_OcclusionTex2;OcclusionTex2;23;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;1222;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode;381;-6000,-1728;Inherit;True;Property;_OcclusionTex;OcclusionTex;22;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;8034ddba71e91c0409a561f39b1d7d6d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp;1224;-5232,-2064;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1234;-5616,-1536;Inherit;False;Property;_SunShadow;SunShadow;20;0;Create;True;0;0;0;False;0;False;0;0.9;0;0.9;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1239;-6128,-1152;Inherit;False;Property;_SoftShadow;SoftShadow;21;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1233;-5232,-1680;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1245;-4976,-1536;Inherit;False;SunShadow;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;378;-4592,-1696;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;406;-4976,-1408;Inherit;False;Property;_OcclusionScale;OcclusionScale;19;0;Create;True;1;AO;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;868;-4720,-1856;Inherit;False;Constant;_Float0;Float 0;62;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;867;-4336,-1856;Inherit;False;Property;_OcclusionIO;OcclusionIO;18;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;382;-3952,-1856;Inherit;False;Occlusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1237;-5616,-1200;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;1226;-5616,-1984;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;376;-6256,-2064;Inherit;False;353;SS;1;0;OBJECT;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1252;-5488,-1344;Inherit;False;bbbb;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1026;-5968,1184;Inherit;False;1004;DotNor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1008;-5728,1280;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;1018;-5744,1184;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1015;-6016,1616;Inherit;False;1004;DotNor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1017;-6128,1712;Inherit;False;Constant;_TDarkOffset;顶部暗部偏移 [TDarkOffset];10;0;Create;False;1;Dark;0;0;False;0;False;0.85;0.533;0.85;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1037;-5488,1856;Inherit;False;Property;_TopDarkScale;TopDarkScale;4;0;Create;True;0;0;0;False;0;False;0;0.325;0;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1012;-6128,1984;Inherit;False;Constant;_TEmergence;顶部暗部羽化 [TEmergence];11;0;Create;False;0;0;0;False;0;False;0.2;0.14;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1013;-5744,1920;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1011;-5744,1712;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1014;-5488,1616;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1007;-6128,1472;Inherit;False;Property;_Emergence;Emergence;6;0;Create;True;0;0;0;False;0;False;0.15;0.3;0.1;0.3;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1006;-5744,1408;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1009;-5488,1184;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1032;-5104,1184;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1036;-5488,1344;Inherit;False;Property;_MDarkScale;DarkScale;3;0;Create;False;1;Dark;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1042;-4848,1184;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1253;-4592,1184;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1254;-4848,1344;Inherit;False;1252;bbbb;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;573;-4336,1184;Inherit;False;DarkMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1016;-5104,1616;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1023;-5984,2688;Inherit;False;1004;DotNor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1021;-6208,2944;Inherit;False;Constant;_BEmergence;底部暗部羽化 [BEmergence];15;0;Create;False;0;0;0;False;0;False;0.2;0.14;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1024;-6208,2816;Inherit;False;Constant;_BDarkOffset;底部暗部偏移 [BDarkOffset];14;0;Create;False;1;Dark;0;0;False;0;False;0.55;0.533;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1020;-5808,2816;Inherit;False;2;0;FLOAT;0.55;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1019;-5808,2912;Inherit;False;2;2;0;FLOAT;0.55;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1022;-5488,2736;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1044;-5056,2736;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1045;-4720,2736;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1054;-4704,2656;Inherit;False;Constant;_Float3;Float 3;50;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1055;-5520,2992;Inherit;False;DotPos;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1046;-5104,2896;Inherit;False;Property;_RefLitColor;RefLitColor;39;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;1038;-5568,2880;Inherit;False;Property;_BDarkScale;RefScale;38;0;Create;False;1;RefLit;0;0;False;0;False;0.5;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1039;-4128,2704;Inherit;False;Ref;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StaticSwitch;1052;-4448,2704;Inherit;False;Property;_RefLitIO;RefLitIO;37;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;1043;-5744,2688;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;789;-5504,3520;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;888;-5120,3568;Inherit;False;Saturation;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;889;-5120,3712;Inherit;False;Value;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;887;-5120,3456;Inherit;False;Hue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;1218;-5680,4032;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;869;-6016,5056;Inherit;False;Property;_CoatSaturation;CoatSaturation;13;0;Create;True;0;0;0;False;0;False;0.75;0.9;0.5;0.9;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;927;-6016,4928;Inherit;False;Property;_GISaturation;MatteSaturation;10;0;Create;False;0;0;0;False;0;False;0.75;0.5;0.5;0.95;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;928;-6016,4800;Inherit;False;888;Saturation;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;790;-5248,4864;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;930;-5632,4800;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;931;-5632,4928;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;891;-5632,5120;Inherit;False;887;Hue;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;806;-5248,5120;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;817;-5632,5264;Inherit;False;Constant;_Float1;Float 1;56;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;1139;-5248,5312;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.75;False;2;FLOAT;1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;1164;-5632,5440;Inherit;False;888;Saturation;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1140;-4864,5312;Inherit;False;NoiseColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;808;-4864,5120;Inherit;False;CubeColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;773;-4864,4864;Inherit;False;LitColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1130;-5632,6336;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.95;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1102;-5760,6464;Inherit;False;Property;_Contrast;Contrast;2;0;Create;True;0;0;0;False;0;False;0.5;1;0.25;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;861;-5248,5888;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;984;-5248,6080;Inherit;False;PreValue;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;1057;-5120,6208;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0.02;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;600;-4768,6208;Inherit;False;DarkColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1064;-5664,4416;Inherit;False;888;Saturation;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1212;-5984,4032;Inherit;False;889;Value;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1120;-5696,6592;Inherit;False;990;LitValueControl;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;895;-6016,5888;Inherit;False;887;Hue;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;892;-6016,6016;Inherit;False;888;Saturation;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;890;-6016,6144;Inherit;False;889;Value;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;983;-5632,6144;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.01;False;4;FLOAT;0.8;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1101;-5376,6416;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1216;-5248,4032;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;1215;-4992,4032;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;FLOAT;0.15;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;1217;-4736,4032;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;990;-4480,4032;Inherit;False;LitValueControl;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1083;1360,480;Inherit;False;754;RotatorLit;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;1071;1728,896;Inherit;True;Property;_TextureSample0;Texture Sample 0;37;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;1196;3088,832;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;1207;2608,880;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1085;3264,832;Inherit;False;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1135;2752,960;Inherit;False;Property;_MatteScale;MatteScale;8;0;Create;True;0;0;0;False;0;False;1;4.34;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1204;2240,1024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1203;2368,1024;Inherit;False;NoiseMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1154;2752,1040;Inherit;False;773;LitColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1182;2752,1120;Inherit;False;1181;Gamma;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1185;2752,1200;Inherit;False;990;LitValueControl;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1249;2752,1280;Inherit;False;1245;SunShadow;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;1178;2928,1344;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;1191;1376,1152;Inherit;False;374;RotatorRef;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1273;1376,176;Inherit;False;1272;HDRLitTex;1;0;OBJECT;;False;1;SAMPLERCUBE;0
Node;AmplifyShaderEditor.GetLocalVarNode;1274;1408,896;Inherit;False;1272;HDRLitTex;1;0;OBJECT;;False;1;SAMPLERCUBE;0
Node;AmplifyShaderEditor.TexturePropertyNode;1070;1344,-848;Inherit;True;Property;_HDRLitTex;HDRLitTex;17;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;None;8b586801afa9de84a9ba7c37174deece;False;white;LockedToCube;Cube;-1;0;2;SAMPLERCUBE;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RegisterLocalVarNode;1272;1856,-848;Inherit;False;HDRLitTex;-1;True;1;0;SAMPLERCUBE;;False;1;SAMPLERCUBE;0
Node;AmplifyShaderEditor.GetLocalVarNode;762;6688,-496;Inherit;True;433;AlbedoColor;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;835;6272,-688;Inherit;True;Property;_SecrecyTex;SecrecyTex;36;0;Create;True;0;0;0;False;0;False;-1;None;8a2a31f675274bd4598761a86f9bc17d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;1276;7376,-688;Inherit;False;Secrecy;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;366;-1168,544;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;364;-992,368;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;365;-992,512;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;831;-672,560;Inherit;False;830;NormalSwitchRef;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;424;-2400,1904;Inherit;False;Property;_NosieColor;NosieColor;31;1;[HDR];Create;False;0;0;0;False;0;False;1,1,1,1;5.992157,5.992157,5.992157,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ComponentMaskNode;425;-2096,1904;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;353;-2272,-1328;Inherit;False;SS;-1;True;1;0;SAMPLERSTATE;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.SamplerStateNode;356;-2512,-1360;Inherit;False;1;1;1;1;-1;None;1;0;SAMPLER2D;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;343;-1488,-704;Inherit;False;DiyFresnelA;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;344;-1824,-704;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;462;-2464,-640;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;345;-2464,-864;Inherit;False;True;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;346;-2128,-864;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;342;-1824,-864;Inherit;False;DiyFresnelB;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;354;-2464,-384;Inherit;False;343;DiyFresnelA;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;348;-2208,-384;Inherit;False;True;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;349;-2464,-256;Inherit;False;Constant;_FresnelPower;FresnelPower;7;0;Create;False;0;0;0;False;0;False;2;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;347;-1952,-384;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;363;-1312,192;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;367;-1312,384;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;405;-2464,192;Inherit;False;Property;_Horizontal;Horizontal;14;0;Create;True;0;0;0;False;0;False;0;279;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;372;-1952,192;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;180;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;369;-1696,192;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;368;-960,192;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.MatrixFromVectors;370;-672,192;Inherit;False;FLOAT3x3;True;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;374;-32,192;Inherit;False;RotatorRef;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;373;-288,192;Inherit;False;2;2;0;FLOAT3x3;0,0,0,1,1,1,1,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;757;-2080,768;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;180;False;1;FLOAT;0
Node;AmplifyShaderEditor.PiNode;751;-1824,768;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;745;-1440,768;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;750;-1056,768;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;748;-1248,1024;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;749;-1568,1024;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;747;-896,1024;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;746;-864,896;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.MatrixFromVectors;752;-544,768;Inherit;False;FLOAT3x3;True;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;753;-288,768;Inherit;False;2;2;0;FLOAT3x3;0,0,0,1,1,1,1,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;754;-32,768;Inherit;False;RotatorLit;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;1156;-976,1632;Inherit;False;Property;_NoiseIO;NoiseIO;30;1;[Toggle];Create;True;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;1157;-720,1584;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;391;-448,1584;Inherit;False;Noise;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerStateNode;386;-2096,1760;Inherit;False;0;0;0;1;-1;None;1;0;SAMPLER2D;;False;1;SAMPLERSTATE;0
Node;AmplifyShaderEditor.RangedFloatNode;687;-2480,1376;Inherit;False;Property;_NoiseScale;NoiseScale;32;0;Create;True;1;Noise;0;0;False;0;False;1;1;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;390;-2480,1504;Inherit;False;Property;_NosieTiling;NosieTiling;33;0;Create;True;1;Noise;0;0;False;0;False;10;30;1;30;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;385;-2096,1584;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;423;-1264,1376;Inherit;False;8;8;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT3;0,0,0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.VertexColorNode;654;-1712,1744;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;1251;-1712,2336;Inherit;False;1245;SunShadow;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1143;-1712,2112;Inherit;False;342;DiyFresnelB;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1141;-1712,2016;Inherit;False;1140;NoiseColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;623;-1712,1936;Inherit;False;1203;NoiseMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1221;-1712,2208;Inherit;False;990;LitValueControl;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1082;-608,1024;Inherit;False;1079;NormalSwitchLit;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;387;-1712,1504;Inherit;True;Property;_NosieTex;NosieTex;34;1;[NoScaleOffset];Create;True;0;0;0;False;0;False;-1;None;71c941071e2883f4dbd2fd9af6f7e7b0;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;350;-2208,-144;Inherit;False;Constant;_FresnelScale;FresnelScale;6;0;Create;False;1;Fresnel;0;0;False;0;False;0.5;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;432;-1696,-272;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;433;-1440,-272;Inherit;False;AlbedoColor;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;624;4208,1648;Inherit;False;391;Noise;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1089;4480,640;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;1124;4992,640;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;1125;4608,896;Inherit;False;Property;_ShadowColor;ShadowColor;7;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode;1123;4608,1152;Inherit;False;573;DarkMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1242;4992,896;Inherit;False;382;Occlusion;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1133;5376,640;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;1153;4992,1024;Inherit;False;Property;_EnvironmentColor;EnvironmentColor;16;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.5566037,0.5566037,0.5566037,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;1094;5760,640;Inherit;False;base;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;837;8320,-768;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;1095;7936,-768;Inherit;False;1094;base;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;836;7936,-512;Inherit;False;Property;_SecrecyIO;SecrecyIO;35;1;[Toggle];Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1277;7936,-640;Inherit;False;1276;Secrecy;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1283;8656,-768;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.HSVToRGBNode;1284;8320,-384;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;723;-5920,3712;Inherit;False;Property;_CarPaintColor;CarPaintColor;0;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0.9269671,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;1010;-6128,1280;Inherit;False;Property;_DarkOffset1;DarkOffset;5;0;Create;False;1;Dark;0;0;False;0;False;0.6;0.5;0;0.7;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1285;7680,-128;Inherit;False;Property;_CarPaintHSV_R;CarPaintHSV_R;1;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;1287;8064,-128;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.5;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;1288;1408,3456;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformPositionNode;1291;1408,3712;Inherit;False;Object;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;1292;1728,3456;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1294;2176,3456;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;1296;2432,3456;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1302;3072,3584;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1303;3328,3584;Inherit;False;FlowRGB;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1301;2688,3840;Inherit;False;Property;_FlowEmission;FlowEmission;44;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1300;3072,3840;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;1304;3328,3840;Inherit;False;FlowAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;1305;8656,-576;Inherit;False;1303;FlowRGB;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;1306;8960,-768;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1299;2688,3584;Inherit;True;Property;_FlowTex;FlowTex;40;0;Create;True;0;0;0;False;0;False;-1;None;3782122390e479d43ad8354e291a0342;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.Vector2Node;1298;2176,3712;Inherit;False;Property;_FlowSpeed;FlowSpeed;43;0;Create;True;0;0;0;False;0;False;0,0;0,-0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SwizzleNode;1293;1920,3456;Inherit;False;FLOAT2;0;2;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;1295;1792,3712;Inherit;False;Property;_FlowTilling;FlowTilling;42;0;Create;True;0;0;0;False;0;False;1,1;5,0.3;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.ColorNode;1307;2688,3968;Inherit;False;Property;_FlowColor;FlowColor;45;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.372549,0.5450979,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;1308;2704,3456;Inherit;False;Property;_FlowToggle;FlowToggle;41;1;[Toggle];Create;True;0;0;0;True;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1309;2399.014,3752.888;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1256;9434.997,-768;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;0;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1258;9434.997,-454.9969;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;False;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=ShadowCaster;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1259;9434.997,-454.9969;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;True;True;False;False;False;0;False;;False;False;False;False;False;False;False;False;False;True;1;False;;False;False;True;1;LightMode=DepthOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1260;9434.997,-454.9969;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1261;9434.997,-454.9969;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=Universal2D;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1262;9434.997,-454.9969;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;SceneSelectionPass;0;6;SceneSelectionPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=SceneSelectionPass;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1263;9434.997,-454.9969;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ScenePickingPass;0;7;ScenePickingPass;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Picking;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1264;9434.997,-454.9969;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormals;0;8;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;False;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1265;9434.997,-454.9969;Float;False;False;-1;3;UnityEditor.ShaderGraphUnlitGUI;0;13;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthNormalsOnly;0;9;DepthNormalsOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;;True;3;False;;False;True;1;LightMode=DepthNormalsOnly;False;True;9;d3d11;metal;vulkan;xboxone;xboxseries;playstation;ps4;ps5;switch;0;;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;1257;9216,-768;Float;False;True;-1;3;ASEMaterialInspector;0;13;FX12_A2_CarPaint;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;1;Forward;8;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;False;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;False;False;False;True;4;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;UniversalMaterialType=Unlit;True;5;True;12;all;0;False;True;1;1;False;;0;False;;1;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;LightMode=UniversalForwardOnly;False;False;0;;0;0;Standard;22;Surface;0;0;  Blend;0;0;Two Sided;1;0;Forward Only;0;0;Cast Shadows;0;639154042485756471;  Use Shadow Threshold;0;0;Receive Shadows;0;639154042499374261;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;0;639154042510615896;Meta Pass;0;0;Extra Pre Pass;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,;0;  Type;0;0;  Tess;16,False,;0;  Min;10,False,;0;  Max;25,False,;0;  Edge Length;16,False,;0;  Max Displacement;25,False,;0;Vertex Position,InvertActionOnDeselection;1;0;0;10;False;True;False;True;False;False;True;True;True;False;False;;False;0
WireConnection;1229;0;1228;0
WireConnection;1229;1;380;0
WireConnection;1229;2;1230;4
WireConnection;998;0;999;0
WireConnection;998;1;1029;0
WireConnection;1000;0;998;0
WireConnection;1004;0;1000;0
WireConnection;1072;0;1273;0
WireConnection;1072;1;1083;0
WireConnection;797;0;861;0
WireConnection;1174;0;1071;2
WireConnection;1174;1;403;0
WireConnection;1174;2;1183;0
WireConnection;1171;0;1190;0
WireConnection;1171;1;1172;0
WireConnection;1171;2;1184;0
WireConnection;1170;0;1174;0
WireConnection;1170;1;1171;0
WireConnection;1087;0;1170;0
WireConnection;1087;1;1105;0
WireConnection;1087;2;973;1
WireConnection;1087;3;1250;0
WireConnection;1002;0;1001;0
WireConnection;1002;1;1030;0
WireConnection;1003;0;1002;0
WireConnection;1005;0;1003;0
WireConnection;764;0;835;0
WireConnection;1100;0;1104;0
WireConnection;1100;1;1103;0
WireConnection;1100;2;1247;0
WireConnection;763;0;764;0
WireConnection;763;1;762;0
WireConnection;1149;0;1148;2
WireConnection;1195;0;1149;0
WireConnection;1247;0;1195;0
WireConnection;1247;1;1246;0
WireConnection;1112;0;1100;0
WireConnection;1112;1;1113;0
WireConnection;1199;0;1072;1
WireConnection;1197;0;1072;1
WireConnection;1197;1;1198;0
WireConnection;1197;2;1205;1
WireConnection;1197;3;1248;0
WireConnection;1181;0;1180;0
WireConnection;1190;0;1071;3
WireConnection;1159;1;1087;0
WireConnection;1159;2;1158;0
WireConnection;1228;5;1231;0
WireConnection;1228;7;377;0
WireConnection;380;5;379;0
WireConnection;380;7;377;0
WireConnection;375;0;380;0
WireConnection;829;0;827;0
WireConnection;826;1;828;0
WireConnection;826;0;829;0
WireConnection;1075;0;827;0
WireConnection;1078;1;1081;0
WireConnection;1078;0;1075;0
WireConnection;830;0;826;0
WireConnection;1079;0;1078;0
WireConnection;1222;7;376;0
WireConnection;381;7;376;0
WireConnection;1224;0;1222;1
WireConnection;1224;2;1226;4
WireConnection;1233;1;381;2
WireConnection;1233;2;1234;0
WireConnection;1245;0;1233;0
WireConnection;378;1;381;1
WireConnection;378;2;406;0
WireConnection;867;1;868;0
WireConnection;867;0;378;0
WireConnection;382;0;867;0
WireConnection;1237;1;381;3
WireConnection;1237;2;1239;0
WireConnection;1252;0;381;3
WireConnection;1008;0;1010;0
WireConnection;1008;1;1007;0
WireConnection;1018;0;1026;0
WireConnection;1013;0;1017;0
WireConnection;1013;1;1012;0
WireConnection;1011;0;1017;0
WireConnection;1011;1;1012;0
WireConnection;1014;0;1015;0
WireConnection;1014;1;1011;0
WireConnection;1014;2;1013;0
WireConnection;1006;0;1010;0
WireConnection;1006;1;1007;0
WireConnection;1009;0;1018;0
WireConnection;1009;1;1008;0
WireConnection;1009;2;1006;0
WireConnection;1032;0;1009;0
WireConnection;1032;1;1036;0
WireConnection;1042;0;1032;0
WireConnection;1042;1;1016;0
WireConnection;1253;0;1042;0
WireConnection;1253;1;1254;0
WireConnection;573;0;1253;0
WireConnection;1016;0;1014;0
WireConnection;1016;1;1037;0
WireConnection;1020;0;1024;0
WireConnection;1020;1;1021;0
WireConnection;1019;0;1024;0
WireConnection;1019;1;1021;0
WireConnection;1022;0;1043;0
WireConnection;1022;1;1020;0
WireConnection;1022;2;1019;0
WireConnection;1044;0;1022;0
WireConnection;1044;1;1038;0
WireConnection;1045;0;1044;0
WireConnection;1045;1;1046;5
WireConnection;1039;0;1052;0
WireConnection;1052;1;1054;0
WireConnection;1052;0;1045;0
WireConnection;1043;0;1023;0
WireConnection;789;0;723;5
WireConnection;888;0;789;2
WireConnection;889;0;789;3
WireConnection;887;0;789;1
WireConnection;1218;0;1212;0
WireConnection;790;0;891;0
WireConnection;790;1;930;0
WireConnection;790;2;817;0
WireConnection;930;0;928;0
WireConnection;930;1;927;0
WireConnection;931;0;928;0
WireConnection;931;1;869;0
WireConnection;806;0;891;0
WireConnection;806;1;931;0
WireConnection;806;2;817;0
WireConnection;1139;0;891;0
WireConnection;1139;1;1164;0
WireConnection;1140;0;1139;0
WireConnection;808;0;806;0
WireConnection;773;0;790;0
WireConnection;1130;0;892;0
WireConnection;861;0;895;0
WireConnection;861;1;892;0
WireConnection;861;2;983;0
WireConnection;984;0;983;0
WireConnection;1057;0;895;0
WireConnection;1057;1;1130;0
WireConnection;1057;2;1101;0
WireConnection;600;0;1057;0
WireConnection;983;0;890;0
WireConnection;1101;0;983;0
WireConnection;1101;1;983;0
WireConnection;1101;2;1102;0
WireConnection;1101;3;1120;0
WireConnection;1216;0;1218;0
WireConnection;1216;1;1064;0
WireConnection;1215;0;1216;0
WireConnection;1217;0;1215;0
WireConnection;990;0;1217;0
WireConnection;1071;0;1274;0
WireConnection;1071;1;1191;0
WireConnection;1196;0;1197;0
WireConnection;1196;1;1207;0
WireConnection;1207;0;1071;1
WireConnection;1085;0;1196;0
WireConnection;1085;1;1135;0
WireConnection;1085;2;1154;0
WireConnection;1085;3;1182;0
WireConnection;1085;4;1185;0
WireConnection;1085;5;1249;0
WireConnection;1204;0;1071;1
WireConnection;1204;1;1071;1
WireConnection;1203;0;1204;0
WireConnection;1272;0;1070;0
WireConnection;1276;0;763;0
WireConnection;366;0;367;0
WireConnection;365;0;366;0
WireConnection;365;2;363;0
WireConnection;425;0;424;0
WireConnection;353;0;356;0
WireConnection;343;0;344;0
WireConnection;344;0;346;0
WireConnection;346;0;345;0
WireConnection;346;1;462;0
WireConnection;342;0;346;0
WireConnection;348;0;354;0
WireConnection;348;1;349;0
WireConnection;347;0;348;0
WireConnection;347;1;350;0
WireConnection;363;0;369;0
WireConnection;367;0;369;0
WireConnection;372;0;405;0
WireConnection;369;0;372;0
WireConnection;368;0;363;0
WireConnection;368;2;367;0
WireConnection;370;0;368;0
WireConnection;370;1;364;0
WireConnection;370;2;365;0
WireConnection;374;0;373;0
WireConnection;373;0;370;0
WireConnection;373;1;831;0
WireConnection;757;0;405;0
WireConnection;751;0;757;0
WireConnection;745;0;751;0
WireConnection;750;0;745;0
WireConnection;750;2;749;0
WireConnection;748;0;749;0
WireConnection;749;0;751;0
WireConnection;747;0;748;0
WireConnection;747;2;745;0
WireConnection;752;0;750;0
WireConnection;752;1;746;0
WireConnection;752;2;747;0
WireConnection;753;0;752;0
WireConnection;753;1;1082;0
WireConnection;754;0;753;0
WireConnection;1157;1;423;0
WireConnection;1157;2;1156;0
WireConnection;391;0;1157;0
WireConnection;385;0;390;0
WireConnection;423;0;687;0
WireConnection;423;1;387;1
WireConnection;423;2;654;1
WireConnection;423;3;623;0
WireConnection;423;4;1141;0
WireConnection;423;5;1143;0
WireConnection;423;6;1221;0
WireConnection;423;7;1251;0
WireConnection;387;1;385;0
WireConnection;387;7;386;0
WireConnection;432;2;347;0
WireConnection;433;0;432;0
WireConnection;1089;0;1112;0
WireConnection;1089;1;1085;0
WireConnection;1089;2;1159;0
WireConnection;1089;3;624;0
WireConnection;1124;0;1089;0
WireConnection;1124;1;1125;5
WireConnection;1124;2;1123;0
WireConnection;1133;0;1124;0
WireConnection;1133;1;1242;0
WireConnection;1133;2;1153;5
WireConnection;1094;0;1133;0
WireConnection;837;0;1095;0
WireConnection;837;1;1277;0
WireConnection;837;2;836;0
WireConnection;1283;0;837;0
WireConnection;1283;1;1284;1
WireConnection;1284;2;1287;0
WireConnection;1287;0;1285;0
WireConnection;1292;0;1288;0
WireConnection;1292;1;1291;0
WireConnection;1294;0;1293;0
WireConnection;1294;1;1295;0
WireConnection;1296;0;1294;0
WireConnection;1296;2;1298;0
WireConnection;1302;0;1299;0
WireConnection;1302;1;1301;0
WireConnection;1302;2;1307;0
WireConnection;1302;3;1308;0
WireConnection;1303;0;1302;0
WireConnection;1300;0;1301;0
WireConnection;1300;1;1299;4
WireConnection;1300;2;1308;0
WireConnection;1304;0;1300;0
WireConnection;1306;0;1283;0
WireConnection;1306;1;1305;0
WireConnection;1299;1;1296;0
WireConnection;1293;0;1292;0
WireConnection;1309;0;1298;0
WireConnection;1309;1;1308;0
WireConnection;1257;2;1306;0
ASEEND*/
//CHKSM=BB9EC1AAC39575E4C1E40DC40CB086FA5F974D30