Shader "Unlit/MovingTextureCorner"
{
 Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("_SpeedX", Float) = 1
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
                float2 uv : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _Speed;
            fixed _Rotation;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed2 scrolledUV = i.worldPos.xz;
                fixed2 scrolledUV = i.uv;
                //scrolledUV.x = scrolledUV.x * _Time;
                //scrolledUV.y = scrolledUV.y * _Time;

                fixed sinX = sin( 3.141 /2);
                fixed cosX = cos( 3.141 /2);
                fixed sinY = sin( 3.141 /2);
                fixed2x2 rotationMatrix = fixed2x2( cosX, -sinX, sinY, cosX);

                fixed2 rotatedUv = mul( i.uv, rotationMatrix );



                float3 direction = float3(1,1,0);
                float3 start = float3(0,0,0);
                float3 vecToMid = cross(direction, float3(i.uv.x,i.uv.y,0) - start);
                

                fixed lerpWeight;

                if(vecToMid.z < 0)
                {
                    fixed maxDistanceToMid = scrolledUV.x;
                    fixed distanceToMid = scrolledUV.x - scrolledUV.y; 
                    //col = fixed4((1- distanceToMid/maxDistanceToMid)/2,0,0,1);

                    lerpWeight = (1- distanceToMid/maxDistanceToMid)/2; // why is this not working?
                    lerpWeight = 0;
                }
                else
                {
                    fixed maxDistanceToMid = scrolledUV.y;
                    fixed distanceToMid = scrolledUV.y - scrolledUV.x; 
                    //col = fixed4(distanceToMid/maxDistanceToMid/2 + 0.5,0,0,1);

                    lerpWeight = distanceToMid/maxDistanceToMid/2 + 0.5; // why is this not working?
                    lerpWeight = 1;
                }

                 sinX = sin( 3.141 /2 * lerpWeight);
                 cosX = cos( 3.141 /2 * lerpWeight);
                 sinY = sin( 3.141 /2 * lerpWeight);
                 rotationMatrix = fixed2x2( cosX, -sinX, sinY, cosX);

                //scrolledUV = mul( i.uv, rotationMatrix );
                scrolledUV = lerp(scrolledUV - fixed2(0,(fixed)_Time), rotatedUv - fixed2(0,(fixed)_Time), lerpWeight);

                //scrolledUV =  rotatedUv;

                fixed4 col = tex2D(_MainTex, scrolledUV);

                //col = fixed4(lerpWeight,0,0,1);


                return col;
            }
            ENDCG
        }
    }
}
