
Shader "Shader Forge/GPUSkinningCartoon" {
    Properties {
        _MainTex ("MainTex (主贴图)", 2D) = "white" {}
		_MainColor("MainColor (主颜色)", Color) = (1,1,1,1)
		_ShadowTex("ShadowTex (阴影贴图)", 2D) = "black" {}
		_ShadowStrength("ShadowStrength (阴影强度)", Range(0, 1)) = 0.1
        _shadowAngleStep ("ShadowAngleStep (阴影范围)", Range(-1, 1)) = 0.4
		_outlinewidth("outlinewidth (描边粗细)", Range(0, 0.01)) = 0.006
		_outlineColor("outlineColor (描边颜色)", Color) = (0,0,0,1)
		//_SceneColor("场景颜色",Color) = (1,1,1,1)
		_SceneLightTex("场景亮度图",2D) = "black"{}
		_SceenLightBeginX("_SceenLightBeginX", Range(-200, 200)) = 0
		_SceenLightBeginZ("_SceenLightBeginZ", Range(-200, 200)) = 0
		_SceenLightBeginXLength("_SceenLightBeginXLength", Range(0, 200)) = 0
		_SceenLightBeginZLength("_SceenLightBeginZLength", Range(0, 200)) = 0
    }
    SubShader {
        Tags {
            "RenderType"="Transparent"
        }

        Pass {
            Name "Outline"
            Tags {
            }
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform float _outlinewidth;
            uniform float4 _outlineColor;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
				float y : float;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.pos = UnityObjectToClipPos( float4(v.vertex.xyz + v.normal*_outlinewidth,1) );
				o.y = mul (unity_ObjectToWorld, v.vertex).y;
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
				
                return _outlineColor;
            }
            ENDCG
        }
		
        Pass {
            Name "FORWARD"
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
			uniform sampler2D _ShadowTex; uniform float4 _ShadowTex_ST;
            uniform float4 _MainColor;
            uniform float4 _ShadowColor;
			uniform float _ShadowStrength;
            uniform float _shadowAngleStep;
			//uniform half4 _SceneColor;


			uniform float _SceenLightBeginX;
			uniform float _SceenLightBeginZ;
			uniform float _SceenLightBeginXLength;
			uniform float _SceenLightBeginZLength;

			uniform sampler2D _SceneLightTex;
			uniform float4 _SceneLightTex_ST;

			fixed4 _LightColor0;
			
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
				float y : float;
                float2 uv0 : TEXCOORD0;
				float2 posWorldUV : TEXCOORD1;
                //float4 dynamicLIghtColor : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
            };
            VertexOutput vert (VertexInput v) 
			{
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                float4 worldPOs = mul(unity_ObjectToWorld, v.vertex);
				float2 posWorldUV;



				posWorldUV.x = clamp((worldPOs.x - _SceenLightBeginX)/ _SceenLightBeginXLength,0,1);
				posWorldUV.y = clamp((worldPOs.z - _SceenLightBeginZ) / _SceenLightBeginZLength,0,1);
				o.posWorldUV = posWorldUV;
				/*o.dynamicLIghtColor = tex2Dlod(_SceneLightTex, float4(posWorldUV.x, posWorldUV.y,0,0));*/

                o.pos = UnityObjectToClipPos( v.vertex );
				o.y = mul (unity_ObjectToWorld, v.vertex).y;
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
				//return float4(i.dynamicLIghtColor.rgb,1);
				//return float4(1, 1, 1, 1);
                i.normalDir = normalize(i.normalDir);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);

////// Lighting:
////// Emissive:
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
				float4 _ShadowTex_var = tex2D(_ShadowTex, TRANSFORM_TEX(i.uv0, _ShadowTex));
				float4 dynamicLIghtColor = tex2D(_SceneLightTex, TRANSFORM_TEX(i.posWorldUV, _SceneLightTex));


				float4 emissive = _MainTex_var;
				emissive.rgb *=	_MainColor.rgb;

				// 灯光角度阴影
				float angle = dot(lightDirection, i.normalDir);
				float stepValue = step(angle, _shadowAngleStep);
				emissive.rgb -= stepValue * (1.0 - _ShadowTex_var.b) * _ShadowStrength *_ShadowTex_var.g;

				// 画的褶皱阴影
				emissive.rgb -= (1.0 - stepValue) * _ShadowTex_var.r * _ShadowStrength*_ShadowTex_var.g;
				//emissive.rgb *= _ShadowTex_var.g;
				//emissive.rgb *= UNITY_LIGHTMODEL_AMBIENT.rgb;

				// 内描边
				/*if (_ShadowTex_var.g > 0)
				{
					emissive.rgb -= (1 - _outlineColorIn.rgb) * _ShadowTex_var.g;
				}*/
				//return float4(_ShadowTex_var.g, _ShadowTex_var.g, _ShadowTex_var.g, 1);
				float3 finalColor = emissive;
				float deltaValue = dynamicLIghtColor.a;
				//finalColor.rgb += deltaValue * i.dynamicLIghtColor.rgb;


				//vec4 sceneLighting = texture2D(u_SceneLightingTexture, v_SceneLightingUV);
				float brightness =  deltaValue - 0.5;
				finalColor.rgb += dynamicLIghtColor.rgb * brightness;
				//finalColor.rgb -= step(deltaValue, 0.5) * (0.25 - deltaValue * 0.5);
				//finalColor.rgb = deltaValue;

				//finalColor.rgb += deltaValue * _LightColor0.rgb;
				
                fixed4 finalRGBA = fixed4(finalColor, _ShadowTex_var.b);

                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }

    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
