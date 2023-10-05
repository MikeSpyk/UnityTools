Shader "Unlit/MovingTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _SpeedX ("_SpeedX", Float) = 1
        _SpeedY ("_SpeedY", Float) = 1
        _Rotation ("_Rotation", Range(0, 6.3)) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float3 worldPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed _SpeedX;
            fixed _SpeedY;
            fixed _Rotation;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed2 scrolledUV = i.worldPos.xz;
 
                scrolledUV[0] += _SpeedX * _Time;
                scrolledUV[1] += _SpeedY * _Time;

                fixed sinX = sin( _Rotation);
                fixed cosX = cos( _Rotation);
                fixed sinY = sin( _Rotation);
                fixed2x2 rotationMatrix = fixed2x2( cosX, -sinX, sinY, cosX);

                scrolledUV = mul( scrolledUV, rotationMatrix );

                fixed4 col = tex2D(_MainTex, scrolledUV) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
