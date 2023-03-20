// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PolyartMaskTint"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Emission("Emission", 2D) = "white" {}
		_EmissionColor("EmissionColor", Color) = (1,0.6413792,0,0)
		_EmissionPower("EmissionPower", Float) = 2
		_Color01("Color01", Color) = (0,0.1394524,0.8088235,0)
		_Color02("Color02", Color) = (0.4557808,0,0.6176471,0)
		_Color03("Color03", Color) = (1,0.6827587,0,0)
		_Brightness("Brightness", Float) = 2
		_Smoothness("Smoothness", Float) = 0
		_Metallic("Metallic", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform float4 _Color01;
		uniform float4 _Color02;
		uniform float4 _Color03;
		uniform float _Brightness;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform float4 _EmissionColor;
		uniform float _EmissionPower;
		uniform float _Metallic;
		uniform float _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode2 = tex2D( _Albedo, uv_Albedo );
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float4 tex2DNode5 = tex2D( _Mask, uv_Mask );
			float4 temp_cast_0 = (tex2DNode5.r).xxxx;
			float4 temp_cast_1 = (tex2DNode5.g).xxxx;
			float4 temp_cast_2 = (tex2DNode5.b).xxxx;
			float4 blendOpSrc20 = tex2DNode2;
			float4 blendOpDest20 = ( min( temp_cast_0 , _Color01 ) + min( temp_cast_1 , _Color02 ) + min( temp_cast_2 , _Color03 ) );
			float4 lerpResult19 = lerp( tex2DNode2 , ( ( saturate( ( blendOpSrc20 * blendOpDest20 ) )) * _Brightness ) , ( tex2DNode5.r + tex2DNode5.g + tex2DNode5.b ));
			o.Albedo = lerpResult19.rgb;
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			float4 blendOpSrc31 = tex2D( _Emission, uv_Emission );
			float4 blendOpDest31 = _EmissionColor;
			o.Emission = ( ( saturate( ( blendOpSrc31 * blendOpDest31 ) )) * _EmissionPower ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
7;29;1906;1004;1765.94;567.9145;1.49412;True;True
Node;AmplifyShaderEditor.SamplerNode;5;-1531.305,-183.2265;Float;True;Property;_Mask;Mask;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;6;-1441.885,69.66664;Float;False;Property;_Color01;Color01;5;0;Create;True;0;0;False;0;0,0.1394524,0.8088235,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;14;-1443.024,472.9286;Float;False;Property;_Color03;Color03;7;0;Create;True;0;0;False;0;1,0.6827587,0,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;13;-1441.885,266.741;Float;False;Property;_Color02;Color02;6;0;Create;True;0;0;False;0;0.4557808,0,0.6176471,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMinOpNode;16;-1129.368,273.1366;Float;True;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMinOpNode;17;-1127.928,493.3281;Float;True;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMinOpNode;15;-1142.564,41.18781;Float;True;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;18;-861.7225,158.1337;Float;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-1216.314,-532.9558;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;25;-573.9053,488.036;Float;False;Property;_EmissionColor;EmissionColor;3;0;Create;True;0;0;False;0;1,0.6413792,0,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;20;-620.3496,-54.13513;Float;False;Multiply;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;24;-605.8335,236.9768;Float;True;Property;_Emission;Emission;2;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;23;-494.2988,113.9074;Float;False;Property;_Brightness;Brightness;8;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-318.5047,-102.2385;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-1009.789,-246.2799;Float;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BlendOpsNode;31;-268.8571,94.52509;Float;True;Multiply;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-210.9263,409.4833;Float;False;Property;_EmissionPower;EmissionPower;4;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;19;-56.85875,-419.8503;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;30;53.63184,230.6446;Float;False;Property;_Smoothness;Smoothness;9;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;29.17365,96.84375;Float;False;Property;_Metallic;Metallic;10;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;2.22398,-38.75273;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;275.9479,-186.7888;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;PBRMaskTint;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;16;0;5;2
WireConnection;16;1;13;0
WireConnection;17;0;5;3
WireConnection;17;1;14;0
WireConnection;15;0;5;1
WireConnection;15;1;6;0
WireConnection;18;0;15;0
WireConnection;18;1;16;0
WireConnection;18;2;17;0
WireConnection;20;0;2;0
WireConnection;20;1;18;0
WireConnection;22;0;20;0
WireConnection;22;1;23;0
WireConnection;21;0;5;1
WireConnection;21;1;5;2
WireConnection;21;2;5;3
WireConnection;31;0;24;0
WireConnection;31;1;25;0
WireConnection;19;0;2;0
WireConnection;19;1;22;0
WireConnection;19;2;21;0
WireConnection;28;0;31;0
WireConnection;28;1;27;0
WireConnection;0;0;19;0
WireConnection;0;2;28;0
WireConnection;0;3;29;0
WireConnection;0;4;30;0
ASEEND*/
//CHKSM=E00A94087D493EDDE87A8373479C5AAFB72ACB53