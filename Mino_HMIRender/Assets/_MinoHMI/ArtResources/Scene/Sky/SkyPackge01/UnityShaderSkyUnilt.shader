// Made with Amplify Shader Editor v1.9.6.3
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "CES/UnityShaderSkyUnilt"
{
	Properties
	{
		[Header((Sky))]_SkyGradientTop("高空颜色_昼", Color) = (1,1,1,1)
		_SkyGradientBottom("低空颜色_昼", Color) = (1,1,1,1)
		_SkyGradientExponent("高低空混合值", Float) = 1
		[Header((HorizonLine))]_HorizonLineColor("边际线颜色_昼", Color) = (1,1,1,1)
		_HorizonLineContribution("边际线大小", Range( 0 , 1)) = 1
		_HorizonLineExponent("边际线混合强度", Float) = 6.8
		[Header((Sun))]_SunColor("太阳颜色", Color) = (1,1,1,0)
		_SunIntensity("太阳亮度", Float) = 1
		_SunRadius("太阳大小", Range( 0 , 1)) = 0.002
		_SunBloom("太阳光晕", Float) = 0.04
		_SunRotation("太阳角度", Vector) = (0,0,0,0)
		[Header((Moon))]_MoonTex("月亮贴图", 2D) = "white" {}
		_MoonColor("月亮颜色", Color) = (0,0,0,0)
		_MoonIntensity("月亮亮度", Float) = 0
		_MoonSize("月亮大小", Range( 0.1 , 1)) = 0.4060405
		_MoonBloom("月亮光晕", Float) = 0.04
		_MoonDirection("月亮角度", Vector) = (0,0,0,0)
		[Header((StarCloud))][Space(10)]_BackGroundTex("星云贴图", 2D) = "black" {}
		_CloudsBackgroundColor("云层颜色", Color) = (0,0,0,0)
		_CloudsBackgroundBrightness("云层透明度", Float) = 1
		_StarColor("星星颜色", Color) = (0,0,0,0)
		_Float16("闪烁速度", Range( 0 , 1)) = 0
		_StarsIntensityHigh("星星亮度", Float) = 0
		_StarTilling("星星平铺值", Vector) = (1,1,0,0)
		[Header((CubeCloud))]_CubeMap("云贴图", CUBE) = "white" {}
		[Toggle(_ROTATIONSWITCH_ON)] _RotationSwitch("角度开关", Float) = 0
		_Rotation("旋转角度", Range( 0 , 360)) = 0
		_Float9("亮度", Float) = 3
		_RotationSpeed("旋转速度", Float) = 0
		_Color0("颜色", Color) = (0,0,0,0)

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Background" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#include "UnityStandardBRDF.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_VERT_POSITION
			#pragma shader_feature_local _ROTATIONSWITCH_ON


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform float4 _SunColor;
			uniform float _SunIntensity;
			uniform float _SunBloom;
			uniform float3 _SunRotation;
			uniform float _SunRadius;
			uniform float4 _MoonColor;
			uniform float _MoonIntensity;
			uniform sampler2D _MoonTex;
			uniform half3 _MoonDirection;
			uniform half _MoonSize;
			uniform float _MoonBloom;
			uniform float4 _HorizonLineColor;
			uniform float _HorizonLineExponent;
			uniform float _HorizonLineContribution;
			uniform float4 _SkyGradientTop;
			uniform float4 _SkyGradientBottom;
			uniform float _SkyGradientExponent;
			uniform float4 _StarColor;
			uniform sampler2D _BackGroundTex;
			uniform float2 _StarTilling;
			uniform float _Float16;
			uniform float _StarsIntensityHigh;
			uniform float4 _CloudsBackgroundColor;
			uniform float _CloudsBackgroundBrightness;
			uniform samplerCUBE _CubeMap;
			uniform float _Rotation;
			uniform float _RotationSpeed;
			uniform float _Float9;
			uniform float4 _Color0;
			float2 ConvertLocalPosToUV299( float3 LocalPos )
			{
				return float2(-atan2(LocalPos.z, LocalPos.x), -acos(LocalPos.y)) / float2(2.0 * UNITY_PI, 0.5 * UNITY_PI);
			}
			

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 normalizeResult599 = normalize( ( _MoonDirection + float3(0.1,0.1,0.1) ) );
				half3 MoonDirection534 = normalizeResult599;
				float3 temp_output_480_0 = cross( MoonDirection534 , half3(0,1,0) );
				float3 normalizeResult483 = normalize( temp_output_480_0 );
				float dotResult487 = dot( normalizeResult483 , v.vertex.xyz );
				float3 normalizeResult482 = normalize( cross( MoonDirection534 , temp_output_480_0 ) );
				float dotResult486 = dot( normalizeResult482 , v.vertex.xyz );
				float2 appendResult489 = (float2(dotResult487 , dotResult486));
				float lerpResult488 = lerp( 20.0 , 2.0 , _MoonSize);
				float2 vertexToFrag492 = (( appendResult489 * lerpResult488 )*0.5 + 0.5);
				o.ase_texcoord1.xy = vertexToFrag492;
				float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
				float lerpResult351 = lerp( 1.0 , ( unity_OrthoParams.y / unity_OrthoParams.x ) , unity_OrthoParams.w);
				float CAMERAMOD352 = lerpResult351;
				float3 appendResult343 = (float3(ase_worldPos.x , ( ase_worldPos.y * CAMERAMOD352 ) , ase_worldPos.z));
				float3 appendResult334 = (float3(cos( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) ) , 0.0 , ( sin( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) ) * -1.0 )));
				float3 appendResult335 = (float3(0.0 , CAMERAMOD352 , 0.0));
				float3 appendResult338 = (float3(sin( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) ) , 0.0 , cos( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) )));
				float3 normalizeResult340 = normalize( ase_worldPos );
				#ifdef _ROTATIONSWITCH_ON
				float3 staticSwitch344 = mul( float3x3(appendResult334, appendResult335, appendResult338), normalizeResult340 );
				#else
				float3 staticSwitch344 = appendResult343;
				#endif
				float3 vertexToFrag345 = staticSwitch344;
				o.ase_texcoord3.xyz = vertexToFrag345;
				
				o.ase_texcoord2 = v.vertex;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				o.ase_texcoord3.w = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float3 SunTintColor460 = (_SunColor).rgb;
				float3 normalizeResult186 = normalize( ( _SunRotation + float3(0.1,0.1,0.1) ) );
				float3 ase_worldViewDir = UnityWorldSpaceViewDir(WorldPosition);
				ase_worldViewDir = Unity_SafeNormalize( ase_worldViewDir );
				float dotResult191 = dot( normalizeResult186 , ase_worldViewDir );
				float SunDotV193 = dotResult191;
				float3 temp_output_473_0 = ( ( ( SunTintColor460 * _SunIntensity ) * ( _SunBloom * 0.01 ) ) / sqrt( max( ( (-SunDotV193*0.5 + 0.5) - ( _SunRadius * 0.01 ) ) , 0.0002 ) ) );
				float3 SunColor475 = ( temp_output_473_0 * temp_output_473_0 );
				float3 MoonTintColor562 = (_MoonColor).rgb;
				float2 vertexToFrag492 = i.ase_texcoord1.xy;
				float3 normalizeResult599 = normalize( ( _MoonDirection + float3(0.1,0.1,0.1) ) );
				half3 MoonDirection534 = normalizeResult599;
				ase_worldViewDir = normalize(ase_worldViewDir);
				float dotResult529 = dot( MoonDirection534 , ase_worldViewDir );
				float4 temp_output_498_0 = ( tex2D( _MoonTex, vertexToFrag492 ) * 1.0 * saturate( dotResult529 ) );
				float3 MoonTexture517 = (temp_output_498_0).rgb;
				float dotResult536 = dot( normalizeResult599 , ase_worldViewDir );
				float MoonDotV537 = dotResult536;
				float MoonAlpha559 = (temp_output_498_0).a;
				float3 temp_output_581_0 = ( ( MoonTintColor562 * _MoonBloom * 0.01 ) / sqrt( max( ( (-MoonDotV537*0.5 + 0.5) - ( MoonAlpha559 * 0.01 ) ) , 0.0002 ) ) );
				float3 MoonBloom583 = ( temp_output_581_0 * temp_output_581_0 * ( 1.0 - ( MoonAlpha559 * 0.5 ) ) );
				float3 MoonColor569 = ( ( MoonTintColor562 * _MoonIntensity * 10.0 * MoonTexture517 ) + MoonBloom583 );
				float4 temp_cast_2 = (0.0).xxxx;
				float3 normalizeResult13 = normalize( WorldPosition );
				float dotResult12 = dot( normalizeResult13 , float3(0,1,0) );
				float MaskHorizon17 = dotResult12;
				float temp_output_108_0 = abs( MaskHorizon17 );
				float4 lerpResult70 = lerp( temp_cast_2 , ( _HorizonLineColor * saturate( abs( pow( ( 1.0 - temp_output_108_0 ) , _HorizonLineExponent ) ) ) ) , _HorizonLineContribution);
				float4 horizonLineColor74 = lerpResult70;
				float4 lerpResult77 = lerp( _SkyGradientTop , _SkyGradientBottom , abs( pow( ( 1.0 - saturate( MaskHorizon17 ) ) , _SkyGradientExponent ) ));
				float4 FinalColor92 = ( horizonLineColor74 + lerpResult77 );
				float3 LocalPos299 = i.ase_texcoord2.xyz;
				float2 localConvertLocalPosToUV299 = ConvertLocalPosToUV299( LocalPos299 );
				float4 tex2DNode262 = tex2D( _BackGroundTex, ( localConvertLocalPosToUV299 * _StarTilling ) );
				float StarTex271 = tex2DNode262.g;
				float temp_output_278_0 = ( StarTex271 * StarTex271 );
				float StarNoise264 = tex2DNode262.b;
				float mulTime268 = _Time.y * _Float16;
				float clampResult307 = clamp( dotResult12 , 0.0 , 1.0 );
				float StarMask309 = clampResult307;
				float3 StarsColor448 = ( ( (_StarColor).rgb * ( temp_output_278_0 * temp_output_278_0 ) * ( sin( ( ( ( StarNoise264 * 3.0 ) + mulTime268 ) * ( 2.0 * UNITY_PI ) ) ) + 1.0 ) * _StarsIntensityHigh * 10.0 * StarMask309 ) + float3( 0,0,0 ) );
				float3 temp_output_291_0 = (_CloudsBackgroundColor).rgb;
				float3 CloudsColor294 = temp_output_291_0;
				float MaskCommon374 = pow( ( StarMask309 * ( 1.0 - temp_output_108_0 ) ) , 1.0 );
				float CloudTex276 = ( tex2D( _BackGroundTex, localConvertLocalPosToUV299 ).r * MaskCommon374 );
				float clampResult293 = clamp( ( ( ( CloudTex276 * CloudTex276 ) * _CloudsBackgroundBrightness ) * MaskHorizon17 ) , 0.0 , 1.0 );
				float CloudsMask300 = clampResult293;
				float4 lerpResult302 = lerp( ( FinalColor92 + float4( StarsColor448 , 0.0 ) ) , float4( CloudsColor294 , 0.0 ) , CloudsMask300);
				float3 vertexToFrag345 = i.ase_texcoord3.xyz;
				float4 CubeMap347 = texCUBE( _CubeMap, vertexToFrag345 );
				float4 clampResult382 = clamp( ( CubeMap347 * _Float9 * float4( _Color0.rgb , 0.0 ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,1 ) );
				
				
				finalColor = ( float4( SunColor475 , 0.0 ) + float4( MoonColor569 , 0.0 ) + lerpResult302 + clampResult382 );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19603
Node;AmplifyShaderEditor.CommentaryNode;182;-7856,-4288;Inherit;False;1680.361;1428.935;Sun&Moon Direction;16;187;193;191;190;186;220;221;219;534;533;537;536;535;597;599;598;Sun&Moon Direction;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;533;-7744,-3712;Half;False;Property;_MoonDirection;月亮角度;16;0;Create;False;1;Moon Color;0;0;False;0;False;0,0,0;-40,-17.5,-9.2;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;597;-7744,-3440;Inherit;False;Constant;_Vector4;Vector 2;13;0;Create;True;0;0;0;False;0;False;0.1,0.1,0.1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;598;-7392,-3712;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;599;-7168,-3712;Inherit;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;477;-7872,-1488;Inherit;False;4103.51;1941.873;MoonColor;56;560;569;587;588;566;589;563;564;562;561;586;585;594;596;576;574;575;584;577;579;581;580;582;583;578;571;595;593;573;572;479;483;487;484;488;485;517;515;559;558;498;494;495;530;492;529;527;528;491;490;489;486;482;481;480;531;MoonColor;0,0.47821,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;534;-6736,-3712;Half;False;MoonDirection;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;531;-7840,-1344;Inherit;False;534;MoonDirection;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;479;-7824,-1184;Half;False;Constant;_Vector3;Vector 3;9;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CrossProductOpNode;480;-7536,-1248;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CrossProductOpNode;481;-7328,-1344;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;482;-7072,-1344;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;483;-7328,-1184;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;484;-7360,-1072;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;476;-1552,-3344;Inherit;False;1604;891;StarMask;11;10;13;14;12;17;307;309;126;18;21;27;StarMask;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;486;-6880,-1344;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;487;-6880,-1184;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;485;-7088,-1008;Half;False;Property;_MoonSize;月亮大小;14;0;Create;False;0;0;0;False;0;False;0.4060405;0.35;0.1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;10;-1504,-3296;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;489;-6704,-1344;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;488;-6704,-1056;Inherit;False;3;0;FLOAT;20;False;1;FLOAT;2;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;13;-1232,-3296;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;14;-1312,-3120;Inherit;False;Constant;_Vector0;Vector 0;1;0;Create;True;0;0;0;False;0;False;0,1,0;0,1,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;490;-6480,-1344;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DotProductOpNode;12;-1024,-3296;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;491;-6288,-1344;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;528;-6288,-1040;Inherit;False;534;MoonDirection;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;527;-6256,-944;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;353;-3168,-864;Inherit;False;4436;1060.508;CubeMap;34;319;320;321;322;323;324;325;326;327;328;329;330;331;332;333;334;335;336;337;338;339;340;341;342;343;344;345;346;347;348;349;350;351;352;CubeMap;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;317;-3184,-2192;Inherit;False;3236;992;horizonLineColor;21;64;66;108;67;69;68;179;73;62;63;71;72;70;74;369;371;372;374;370;383;384;horizonLineColor;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;17;-336,-3296;Inherit;True;MaskHorizon;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;259;-3136,-4736;Inherit;False;2011.602;969.2559;Static BackGround;12;276;264;271;360;365;272;262;261;260;299;298;604;Static BackGround;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;529;-6000,-1040;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;492;-6048,-1344;Inherit;False;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;319;-3088,-496;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;320;-3104,-368;Inherit;False;Property;_RotationSpeed;旋转速度;28;0;Create;False;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;307;-640,-3040;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;64;-3136,-1568;Inherit;True;17;MaskHorizon;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;298;-3104,-4672;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;530;-5712,-1040;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;495;-5712,-1136;Inherit;False;Constant;_Float17;Float 1;23;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;494;-5792,-1344;Inherit;True;Property;_MoonTex;月亮贴图;11;1;[Header];Create;False;1;(Moon);0;0;False;0;False;-1;ba5fcf51cd3e29440b6814a5b29593f1;ba5fcf51cd3e29440b6814a5b29593f1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode;321;-2928,-672;Inherit;False;Property;_Rotation;旋转角度;26;0;Create;False;0;0;0;False;0;False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;322;-2816,-496;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;535;-7152,-3392;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;309;-192,-3040;Inherit;True;StarMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;108;-2832,-1568;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;370;-2688,-2016;Inherit;False;Constant;_MaskScale;MaskScale ;23;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CustomExpressionNode;299;-2816,-4672;Inherit;False;return float2(-atan2(LocalPos.z, LocalPos.x), -acos(LocalPos.y)) / float2(2.0 * UNITY_PI, 0.5 * UNITY_PI)@;2;Create;1;True;LocalPos;FLOAT3;0,0,0;In;;Inherit;False;ConvertLocalPosToUV;True;False;0;;False;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;260;-2816,-4560;Inherit;False;Property;_StarTilling;星星平铺值;23;0;Create;False;0;0;0;False;0;False;1,1;15,3;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;498;-5440,-1344;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;323;-2560,-672;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OrthoParams;349;-2064,16;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DotProductOpNode;536;-6896,-3488;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;219;-7760,-4240;Inherit;False;Property;_SunRotation;太阳角度;10;0;Create;False;0;0;0;False;0;False;0,0,0;-5,-9,-20;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;221;-7760,-4016;Inherit;False;Constant;_Vector2;Vector 2;13;0;Create;True;0;0;0;False;0;False;0.1,0.1,0.1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;369;-2320,-2032;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;371;-2336,-2128;Inherit;False;309;StarMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;261;-2544,-4512;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode;558;-5152,-1136;Inherit;False;FLOAT;3;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode;324;-2320,-672;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;350;-1744,-16;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;348;-1760,-112;Inherit;False;Constant;_Float8;Float 2;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;537;-6624,-3472;Inherit;False;MoonDotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;220;-7408,-4240;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;372;-2048,-2128;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;384;-2032,-1888;Inherit;False;Constant;_Float12;Float 12;24;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;66;-2816,-1760;Inherit;False;Constant;_Float4;Float 4;5;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;262;-2352,-4576;Inherit;True;Property;_BackGroundTex;星云贴图;17;1;[Header];Create;False;1;(StarCloud);0;0;False;1;Space(10);False;-1;None;fa542060c1215e34c878cd7d49c0992e;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;559;-4960,-1136;Inherit;True;MoonAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;325;-2112,-672;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;351;-1504,16;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;571;-7840,-112;Inherit;False;537;MoonDotV;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;190;-7152,-3952;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;186;-7200,-4240;Inherit;True;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;263;32,-5792;Inherit;False;2660.369;1682.479;StarsColor;23;289;285;305;306;284;282;287;281;277;278;280;273;275;269;270;268;267;266;265;448;451;453;450;StarsColor;1,0.3160377,0.7846898,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;264;-1920,-4416;Inherit;False;StarNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;383;-1808,-2128;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;67;-2448,-1760;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-2496,-1456;Inherit;True;Property;_HorizonLineExponent;边际线混合强度;5;0;Create;False;0;0;0;False;0;False;6.8;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;326;-1856,-624;Inherit;False;Constant;_Float1;Float 0;4;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;327;-1856,-736;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;352;-1232,16;Inherit;False;CAMERAMOD;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;595;-7536,224;Inherit;False;Constant;_Float19;Float 19;30;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;593;-7584,32;Inherit;False;559;MoonAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;586;-7568,-112;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;560;-5312,-400;Inherit;False;Property;_MoonColor;月亮颜色;12;0;Create;False;0;0;0;False;0;False;0,0,0,0;0.6941177,0.7981969,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.DotProductOpNode;191;-6832,-4144;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;265;64,-5280;Inherit;False;Constant;_Float2;Float 2;19;0;Create;True;0;0;0;False;0;False;3;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;266;64,-5408;Inherit;False;264;StarNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;453;80,-5024;Inherit;False;Property;_Float16;闪烁速度;21;0;Create;False;0;0;0;False;0;False;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;316;496,-3024;Inherit;False;2676;1328;FinalColor;13;79;80;78;81;83;82;180;75;76;77;85;87;92;FinalColor;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;374;-1536,-2128;Inherit;False;MaskCommon;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;68;-2016,-1760;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;328;-1856,-448;Inherit;False;Constant;_Float3;Float 1;4;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;329;-1856,-816;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;330;-1856,-352;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode;331;-1856,-256;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;332;-1664,-736;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;333;-1888,-544;Inherit;False;352;CAMERAMOD;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;454;-7856,-2592;Inherit;False;2125.946;748.4448;SunColor;21;475;474;473;472;471;470;469;468;467;466;465;464;463;462;461;460;459;458;457;456;455;SunColor;1,0.5181768,0,1;0;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;585;-7392,-112;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;594;-7328,32;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;561;-5040,-400;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;193;-6560,-4144;Inherit;False;SunDotV;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;267;304,-5408;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;268;272,-5152;Inherit;False;1;0;FLOAT;0.25;False;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;179;-1728,-1760;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;79;544,-2080;Inherit;False;17;MaskHorizon;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;272;-2560,-4336;Inherit;True;Property;_BackGroundTex1;BackGround Tex;17;1;[Header];Create;True;1;Static Background;0;0;False;1;Space(10);False;-1;None;None;True;0;False;black;Auto;False;Instance;262;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode;365;-2496,-4064;Inherit;True;374;MaskCommon;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;334;-1408,-816;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;335;-1408,-560;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;336;-1040,-528;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;337;-1104,-320;Inherit;False;352;CAMERAMOD;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;338;-1408,-304;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;457;-7840,-2192;Inherit;False;193;SunDotV;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;596;-7104,-112;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;562;-4848,-400;Inherit;False;MoonTintColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PiNode;270;528,-5152;Inherit;False;1;0;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;269;592,-5408;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;73;-1504,-1760;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;62;-1600,-1968;Inherit;False;Property;_HorizonLineColor;边际线颜色_昼;3;1;[Header];Create;False;1;(HorizonLine);0;0;False;0;False;1,1,1,1;0.3867922,0.2842393,0.2317102,0.7019608;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SaturateNode;80;784,-2080;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;576,-2208;Inherit;False;Constant;_Float7;Float 7;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;360;-2048,-4288;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;271;-1584,-4640;Inherit;False;StarTex;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.MatrixFromVectors;339;-1136,-816;Inherit;False;FLOAT3x3;True;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.NormalizeNode;340;-640,-624;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;341;-768,-352;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;455;-7824,-2528;Inherit;False;Property;_SunColor;太阳颜色;6;1;[Header];Create;False;1;(Sun);0;0;False;0;False;1,1,1,0;1,1,1,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.NegateNode;459;-7632,-2192;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;461;-7632,-1936;Inherit;False;Constant;_Float10;Float 10;23;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;458;-7712,-2016;Inherit;False;Property;_SunRadius;太阳大小;8;0;Create;False;0;0;0;False;0;False;0.002;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;584;-6928,-112;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.0002;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;575;-7184,112;Inherit;False;559;MoonAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;574;-7152,272;Inherit;False;Constant;_Float15;Float 9;24;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;572;-6960,-416;Inherit;False;562;MoonTintColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;573;-6896,-320;Inherit;False;Property;_MoonBloom;月亮光晕;15;0;Create;False;0;0;0;False;0;False;0.04;0.85;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;576;-6896,-224;Inherit;False;Constant;_Float18;Float 0;23;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;273;816,-5408;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;274;-3152,-3360;Inherit;False;1313.99;866.3525;Clouds Color;13;300;293;286;381;380;290;279;294;291;288;283;556;557;Clouds Color;0.6179246,0.9168098,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-1232,-1968;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;71;-1232,-2144;Inherit;False;Constant;_Float5;Float 5;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;-1280,-1584;Inherit;True;Property;_HorizonLineContribution;边际线大小;4;0;Create;False;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;81;1040,-2208;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;276;-1776,-4288;Inherit;False;CloudTex;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;275;656,-5536;Inherit;False;271;StarTex;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;1024,-1984;Inherit;True;Property;_SkyGradientExponent;高低空混合值;2;0;Create;False;0;0;0;False;0;False;1;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;456;-7504,-2528;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;342;-368,-816;Inherit;False;2;2;0;FLOAT3x3;0,0,0,1,1,1,1,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;343;-576,-496;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;463;-7456,-2192;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;462;-7424,-2016;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SqrtOpNode;578;-6640,-112;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;579;-6928,112;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;577;-6640,-416;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;280;1040,-5280;Inherit;False;Constant;_Float6;Float 4;19;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;278;1072,-5536;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;281;1168,-5744;Inherit;False;Property;_StarColor;星星颜色;20;0;Create;False;0;0;0;False;0;False;0,0,0,0;1,1,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SinOpNode;277;1104,-5408;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;279;-3136,-2880;Inherit;True;276;CloudTex;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;70;-640,-2144;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;82;1280,-2208;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;515;-5152,-1344;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;464;-6992,-2288;Inherit;False;Constant;_Float11;Float 11;23;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;460;-7280,-2528;Inherit;False;SunTintColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;466;-7264,-2400;Inherit;False;Property;_SunIntensity;太阳亮度;7;0;Create;False;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;467;-6992,-2368;Inherit;False;Property;_SunBloom;太阳光晕;9;0;Create;False;0;0;0;False;0;False;0.04;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;344;-80,-784;Inherit;False;Property;_RotationSwitch;角度开关;25;0;Create;False;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;465;-7216,-2192;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;580;-6512,112;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;581;-6400,-416;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode;287;1456,-5744;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;282;1328,-5536;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;284;1360,-5408;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;306;1296,-5056;Inherit;False;Constant;_Float0;Float 0;18;0;Create;True;0;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;285;1296,-5264;Inherit;False;Property;_StarsIntensityHigh;星星亮度;22;0;Create;False;0;0;0;False;0;False;0;0.0015;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;283;-2912,-2880;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;286;-2880,-2624;Inherit;False;Property;_CloudsBackgroundBrightness;云层透明度;19;0;Create;False;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;74;-192,-2144;Inherit;True;horizonLineColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.AbsOpNode;180;1552,-2208;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;76;1456,-2464;Inherit;False;Property;_SkyGradientBottom;低空颜色_昼;1;0;Create;False;0;0;0;False;0;False;1,1,1,1;0.3064699,0.3509014,0.6698113,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.ColorNode;75;1456,-2672;Inherit;False;Property;_SkyGradientTop;高空颜色_昼;0;1;[Header];Create;False;1;(Sky);0;0;False;0;False;1,1,1,1;0.2358487,0.2358487,0.2358487,1;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.GetLocalVarNode;305;1296,-5168;Inherit;False;309;StarMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;517;-4960,-1344;Inherit;False;MoonTexture;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;470;-6928,-2528;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;469;-6768,-2368;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;345;320,-784;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;468;-7040,-2192;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.0002;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;582;-6064,-416;Inherit;True;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;289;1712,-5568;Inherit;False;6;6;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;290;-2688,-2880;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;381;-2672,-2624;Inherit;False;17;MaskHorizon;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;77;1856,-2672;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;2048,-2976;Inherit;False;74;horizonLineColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;471;-6576,-2528;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;346;672,-624;Inherit;True;Property;_CubeMap;云贴图;24;1;[Header];Create;False;1;(CubeCloud);0;0;False;1;;False;-1;None;614752a4a12a53c479c1d46fe5171f9b;True;0;False;white;LockedToCube;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SqrtOpNode;472;-6736,-2192;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;583;-5776,-416;Inherit;True;MoonBloom;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;563;-4784,-320;Inherit;False;Property;_MoonIntensity;月亮亮度;13;0;Create;False;0;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;564;-4784,-240;Inherit;False;Constant;_Float14;Float 6;21;0;Create;True;0;0;0;False;0;False;10;8.91;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;589;-4848,-160;Inherit;False;517;MoonTexture;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;450;1968,-5568;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;380;-2448,-2880;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;288;-2928,-3248;Inherit;False;Property;_CloudsBackgroundColor;云层颜色;18;0;Create;False;0;0;0;False;0;False;0,0,0,0;0.2358487,0.2358487,0.2358487,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode;87;2544,-2976;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;602;-3152,352;Inherit;False;1723.193;1069.323;UnityShaderSkyUnilt;15;385;368;366;367;382;304;303;301;222;93;554;302;532;216;217;UnityShaderSkyUnilt;0.0788092,0.2344644,0.7264151,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;347;1056,-624;Inherit;False;CubeMap;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;473;-6416,-2528;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;566;-4480,-400;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;588;-4816,-64;Inherit;False;583;MoonBloom;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;448;2144,-5568;Inherit;False;StarsColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;293;-2224,-2880;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode;291;-2592,-3248;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;92;2928,-2976;Inherit;True;FinalColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;474;-6160,-2528;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;587;-4208,-400;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;366;-2864,912;Inherit;False;347;CubeMap;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;368;-2832,992;Inherit;False;Property;_Float9;亮度;27;0;Create;False;0;0;0;False;0;False;3;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;385;-2896,1072;Inherit;False;Property;_Color0;颜色;29;0;Create;False;0;0;0;False;0;False;0,0,0,0;0.09433924,0.09433924,0.09433924,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RegisterLocalVarNode;300;-2064,-2880;Inherit;True;CloudsMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;294;-2048,-3248;Inherit;False;CloudsColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;-3072,640;Inherit;False;92;FinalColor;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;301;-3072,736;Inherit;False;448;StarsColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;475;-5984,-2528;Inherit;False;SunColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;569;-4032,-400;Inherit;False;MoonColor;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;367;-2592,912;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;554;-2736,640;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;304;-2768,832;Inherit;False;300;CloudsMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;303;-2768,752;Inherit;False;294;CloudsColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;216;-2400,448;Inherit;False;475;SunColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;532;-2400,544;Inherit;False;569;MoonColor;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;382;-2368,912;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,1;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;302;-2448,640;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;451;96,-5504;Inherit;False;Constant;_Float13;Float 13;30;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;126;-1408,-2640;Inherit;False;True;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;18;-1376,-2816;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;21;-1088,-2816;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;27;-464,-2816;Inherit;True;MaskSunDir;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;556;-2304,-3136;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;557;-2624,-3040;Inherit;False;-1;;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;187;-6832,-4240;Inherit;False;SunDirection;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;217;-2000,448;Inherit;True;4;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;604;-1696,-4480;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;222;-1696,448;Float;False;True;-1;2;ASEMaterialInspector;100;5;CES/UnityShaderSkyUnilt;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Background=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
WireConnection;598;0;533;0
WireConnection;598;1;597;0
WireConnection;599;0;598;0
WireConnection;534;0;599;0
WireConnection;480;0;531;0
WireConnection;480;1;479;0
WireConnection;481;0;531;0
WireConnection;481;1;480;0
WireConnection;482;0;481;0
WireConnection;483;0;480;0
WireConnection;486;0;482;0
WireConnection;486;1;484;0
WireConnection;487;0;483;0
WireConnection;487;1;484;0
WireConnection;489;0;487;0
WireConnection;489;1;486;0
WireConnection;488;2;485;0
WireConnection;13;0;10;0
WireConnection;490;0;489;0
WireConnection;490;1;488;0
WireConnection;12;0;13;0
WireConnection;12;1;14;0
WireConnection;491;0;490;0
WireConnection;17;0;12;0
WireConnection;529;0;528;0
WireConnection;529;1;527;0
WireConnection;492;0;491;0
WireConnection;307;0;12;0
WireConnection;530;0;529;0
WireConnection;494;1;492;0
WireConnection;322;0;319;0
WireConnection;322;1;320;0
WireConnection;309;0;307;0
WireConnection;108;0;64;0
WireConnection;299;0;298;0
WireConnection;498;0;494;0
WireConnection;498;1;495;0
WireConnection;498;2;530;0
WireConnection;323;0;321;0
WireConnection;323;1;322;0
WireConnection;536;0;599;0
WireConnection;536;1;535;0
WireConnection;369;0;370;0
WireConnection;369;1;108;0
WireConnection;261;0;299;0
WireConnection;261;1;260;0
WireConnection;558;0;498;0
WireConnection;324;0;323;0
WireConnection;350;0;349;2
WireConnection;350;1;349;1
WireConnection;537;0;536;0
WireConnection;220;0;219;0
WireConnection;220;1;221;0
WireConnection;372;0;371;0
WireConnection;372;1;369;0
WireConnection;262;1;261;0
WireConnection;559;0;558;0
WireConnection;325;0;324;0
WireConnection;351;0;348;0
WireConnection;351;1;350;0
WireConnection;351;2;349;4
WireConnection;186;0;220;0
WireConnection;264;0;262;3
WireConnection;383;0;372;0
WireConnection;383;1;384;0
WireConnection;67;0;66;0
WireConnection;67;1;108;0
WireConnection;327;0;325;0
WireConnection;352;0;351;0
WireConnection;586;0;571;0
WireConnection;191;0;186;0
WireConnection;191;1;190;0
WireConnection;374;0;383;0
WireConnection;68;0;67;0
WireConnection;68;1;69;0
WireConnection;329;0;325;0
WireConnection;330;0;325;0
WireConnection;331;0;325;0
WireConnection;332;0;327;0
WireConnection;332;1;326;0
WireConnection;585;0;586;0
WireConnection;594;0;593;0
WireConnection;594;1;595;0
WireConnection;561;0;560;0
WireConnection;193;0;191;0
WireConnection;267;0;266;0
WireConnection;267;1;265;0
WireConnection;268;0;453;0
WireConnection;179;0;68;0
WireConnection;272;1;299;0
WireConnection;334;0;329;0
WireConnection;334;1;328;0
WireConnection;334;2;332;0
WireConnection;335;0;328;0
WireConnection;335;1;333;0
WireConnection;335;2;328;0
WireConnection;338;0;330;0
WireConnection;338;1;328;0
WireConnection;338;2;331;0
WireConnection;596;0;585;0
WireConnection;596;1;594;0
WireConnection;562;0;561;0
WireConnection;269;0;267;0
WireConnection;269;1;268;0
WireConnection;73;0;179;0
WireConnection;80;0;79;0
WireConnection;360;0;272;1
WireConnection;360;1;365;0
WireConnection;271;0;262;2
WireConnection;339;0;334;0
WireConnection;339;1;335;0
WireConnection;339;2;338;0
WireConnection;340;0;336;0
WireConnection;341;0;336;2
WireConnection;341;1;337;0
WireConnection;459;0;457;0
WireConnection;584;0;596;0
WireConnection;273;0;269;0
WireConnection;273;1;270;0
WireConnection;63;0;62;0
WireConnection;63;1;73;0
WireConnection;81;0;78;0
WireConnection;81;1;80;0
WireConnection;276;0;360;0
WireConnection;456;0;455;0
WireConnection;342;0;339;0
WireConnection;342;1;340;0
WireConnection;343;0;336;1
WireConnection;343;1;341;0
WireConnection;343;2;336;3
WireConnection;463;0;459;0
WireConnection;462;0;458;0
WireConnection;462;1;461;0
WireConnection;578;0;584;0
WireConnection;579;0;575;0
WireConnection;579;1;574;0
WireConnection;577;0;572;0
WireConnection;577;1;573;0
WireConnection;577;2;576;0
WireConnection;278;0;275;0
WireConnection;278;1;275;0
WireConnection;277;0;273;0
WireConnection;70;0;71;0
WireConnection;70;1;63;0
WireConnection;70;2;72;0
WireConnection;82;0;81;0
WireConnection;82;1;83;0
WireConnection;515;0;498;0
WireConnection;460;0;456;0
WireConnection;344;1;343;0
WireConnection;344;0;342;0
WireConnection;465;0;463;0
WireConnection;465;1;462;0
WireConnection;580;0;579;0
WireConnection;581;0;577;0
WireConnection;581;1;578;0
WireConnection;287;0;281;0
WireConnection;282;0;278;0
WireConnection;282;1;278;0
WireConnection;284;0;277;0
WireConnection;284;1;280;0
WireConnection;283;0;279;0
WireConnection;283;1;279;0
WireConnection;74;0;70;0
WireConnection;180;0;82;0
WireConnection;517;0;515;0
WireConnection;470;0;460;0
WireConnection;470;1;466;0
WireConnection;469;0;467;0
WireConnection;469;1;464;0
WireConnection;345;0;344;0
WireConnection;468;0;465;0
WireConnection;582;0;581;0
WireConnection;582;1;581;0
WireConnection;582;2;580;0
WireConnection;289;0;287;0
WireConnection;289;1;282;0
WireConnection;289;2;284;0
WireConnection;289;3;285;0
WireConnection;289;4;306;0
WireConnection;289;5;305;0
WireConnection;290;0;283;0
WireConnection;290;1;286;0
WireConnection;77;0;75;0
WireConnection;77;1;76;0
WireConnection;77;2;180;0
WireConnection;471;0;470;0
WireConnection;471;1;469;0
WireConnection;346;1;345;0
WireConnection;472;0;468;0
WireConnection;583;0;582;0
WireConnection;450;0;289;0
WireConnection;380;0;290;0
WireConnection;380;1;381;0
WireConnection;87;0;85;0
WireConnection;87;1;77;0
WireConnection;347;0;346;0
WireConnection;473;0;471;0
WireConnection;473;1;472;0
WireConnection;566;0;562;0
WireConnection;566;1;563;0
WireConnection;566;2;564;0
WireConnection;566;3;589;0
WireConnection;448;0;450;0
WireConnection;293;0;380;0
WireConnection;291;0;288;0
WireConnection;92;0;87;0
WireConnection;474;0;473;0
WireConnection;474;1;473;0
WireConnection;587;0;566;0
WireConnection;587;1;588;0
WireConnection;300;0;293;0
WireConnection;294;0;291;0
WireConnection;475;0;474;0
WireConnection;569;0;587;0
WireConnection;367;0;366;0
WireConnection;367;1;368;0
WireConnection;367;2;385;5
WireConnection;554;0;93;0
WireConnection;554;1;301;0
WireConnection;382;0;367;0
WireConnection;302;0;554;0
WireConnection;302;1;303;0
WireConnection;302;2;304;0
WireConnection;21;0;18;0
WireConnection;21;1;126;0
WireConnection;27;0;21;0
WireConnection;556;0;291;0
WireConnection;556;1;557;0
WireConnection;187;0;186;0
WireConnection;217;0;216;0
WireConnection;217;1;532;0
WireConnection;217;2;302;0
WireConnection;217;3;382;0
WireConnection;222;0;217;0
ASEEND*/
//CHKSM=D6FC6BC3CE8A730EED922FC41A45F0F5F5427A82