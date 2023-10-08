Shader"Minecraft/TransparentShader" {

	Properties {
		_MainTex ("Block Texture Atlas", 2D) = "white" {}
		_GlobalLightLevel ("Global Light Level", Range(0, 1)) = 0
	}

	SubShader {
		
		Tags {"Queue"="Transparent" "RenderType"="Transparent"}
LOD 100
Lighting Off
ZWrite Off
Blend SrcAlpha
OneMinusSrcAlpha

		Pass {
		
			CGPROGRAM
				#pragma vertex vertFunction
				#pragma fragment fragFunction
				#pragma target 2.0

#include "UnityCG.cginc"

struct appdata
{
				
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
    float4 color : COLOR;

};

struct v2f
{
				
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float2 uv2 : TEXCOORD1;
    float4 color : COLOR;

};

sampler2D _MainTex;
float _GlobalLightLevel;

v2f vertFunction(appdata v)
{
				
    v2f o;

    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    o.uv2 = v.uv2;
    o.color = v.color;
					
    return o;

}

fixed4 fragFunction(v2f i) : SV_Target
{
    float light = max(i.uv2.x, i.uv2.y);
    float ao = i.color.a;
    float shade = light * ao * _GlobalLightLevel;
    shade = 1 - shade;
	
    fixed4 col = tex2D(_MainTex, i.uv);
    clip(col.a);
    if (col.a > 0)
    {
        col = float4(col.r * i.color.r, col.g * i.color.g, col.b * i.color.b, col.a);
        col = lerp(col, float4(0, 0, 0, 1), shade);
    }

    return col;

}

                ENDCG

		}


	}

}