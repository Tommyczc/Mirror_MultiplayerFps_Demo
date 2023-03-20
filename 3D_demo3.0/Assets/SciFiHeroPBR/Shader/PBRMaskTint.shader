// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PBRMaskTint"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_SAM("SAM", 2D) = "white" {}
		_Emission("Emission", 2D) = "white" {}
		_EmissionColor("EmissionColor", Color) = (1,0.6413792,0,0)
		_EmissionPower("EmissionPower", Float) = 2
		_Color01("Color01", Color) = (0,0.1394524,0.8088235,0)
		_Color03("Color03", Color) = (1,0.6827587,0,0)
		_Color02("Color02", Color) = (0.4557808,0,0.6176471,0)
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

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform sampler2D _Mask;
		uniform float4 _Mask_ST;
		uniform float4 _Color01;
		uniform float4 _Color02;
		uniform float4 _Color03;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform float4 _EmissionColor;
		uniform float _EmissionPower;
		uniform sampler2D _SAM;
		uniform float4 _SAM_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode2 = tex2D( _Albedo, uv_Albedo );
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			float4 tex2DNode5 = tex2D( _Mask, uv_Mask );
			float4 temp_cast_0 = (tex2DNode5.r).xxxx;
			float4 temp_cast_1 = (tex2DNode5.g).xxxx;
			float4 temp_cast_2 = (tex2DNode5.b).xxxx;
			float4 blendOpSrc20 = tex2DNode2;
			float4 blendOpDest20 = ( min( temp_cast_0 , _Color01 ) + min( temp_cast_1 , _Color02 ) + min( temp_cast_2 , _Color03 ) );
			float4 lerpResult19 = lerp( tex2DNode2 , ( ( saturate( ( blendOpSrc20 * blendOpDest20 ) )) * 2.0 ) , ( tex2DNode5.r + tex2DNode5.g + tex2DNode5.b ));
			o.Albedo = lerpResult19.rgb;
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			o.Emission = ( ( tex2D( _Emission, uv_Emission ) * _EmissionColor ) * _EmissionPower ).rgb;
			float2 uv_SAM = i.uv_texcoord * _SAM_ST.xy + _SAM_ST.zw;
			float4 tex2DNode4 = tex2D( _SAM, uv_SAM );
			o.Metallic = tex2DNode4.b;
			o.Smoothness = tex2DNode4.r;
			o.Occlusion = tex2DNode4.g;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
7;29;1906;1004;1864.031;600.3706;1.451209;True;True
Node;AmplifyShaderEditor.SamplerNode;5;-1531.305,-183.2265;Float;True;Property;_Mask;Mask;1;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;6;-1441.885,69.66664;Float;False;Property;_Color01;Color01;7;0;Create;True;0;0;False;0;0,0.1394524,0.8088235,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;14;-1443.024,472.9286;Float;False;Property;_Color03;Color03;8;0;Create;True;0;0;False;0;1,0.6827587,0,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;13;-1441.885,266.741;Float;False;Property;_Color02;Color02;9;0;Create;True;0;0;False;0;0.4557808,0,0.6176471,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMinOpNode;16;-1129.368,273.1366;Float;True;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMinOpNode;17;-1127.928,493.3281;Float;True;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMinOpNode;15;-1142.564,41.18781;Float;True;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;18;-858.2451,224.2051;Float;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;2;-1216.314,-532.9558;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;24;-605.8335,236.9768;Float;True;Property;_Emission;Emission;4;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;25;-573.9053,488.036;Float;False;Property;_EmissionColor;EmissionColor;5;0;Create;True;0;0;False;0;1,0.6413792,0,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;20;-620.3496,-54.13513;Float;False;Multiply;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-428.2277,150.4205;Float;False;Constant;_Float0;Float 0;8;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-224.1641,200.6965;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-60.17803,254.3914;Float;False;Property;_EmissionPower;EmissionPower;6;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;21;-1009.789,-246.2799;Float;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-318.5047,-102.2385;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;2.22398,-38.75273;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;19;-56.85875,-419.8503;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;1;-153.7722,626.7712;Float;True;Property;_Normal;Normal;2;0;Create;True;0;0;False;0;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;4;-176.4572,393.7277;Float;True;Property;_SAM;SAM;3;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
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
WireConnection;26;0;24;0
WireConnection;26;1;25;0
WireConnection;21;0;5;1
WireConnection;21;1;5;2
WireConnection;21;2;5;3
WireConnection;22;0;20;0
WireConnection;22;1;23;0
WireConnection;28;0;26;0
WireConnection;28;1;27;0
WireConnection;19;0;2;0
WireConnection;19;1;22;0
WireConnection;19;2;21;0
WireConnection;0;0;19;0
WireConnection;0;1;1;0
WireConnection;0;2;28;0
WireConnection;0;3;4;3
WireConnection;0;4;4;1
WireConnection;0;5;4;2
ASEEND*/
//CHKSM=05943348B3C5596A48721EBC7BE2CAA45F21D06E