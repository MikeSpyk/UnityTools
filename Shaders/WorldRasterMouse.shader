Shader "Unlit/WorldRasterMouse"
{
    Properties
    {
        _RasterCellSize("_RasterCellSize", Float) = 1
        _RasterColor("Raster Color", Color) = (1,1,1,1)
        _RasterLineStrength("_RasterLineStrength", Float) = 60
        _MousePositon ("_MousePositon", Vector) = (0,0,0,0)
        _ShowDistanceMouse("_ShowDistanceMouse", Float) = 2
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
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 worldPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _RasterCellSize;
            float _RasterLineStrength;
            uniform float4 _RasterColor;
            float4 _MousePositon;
            float _ShowDistanceMouse;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul (unity_ObjectToWorld, v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _RasterColor;

                float2 mousePosXY = float2(_MousePositon[0],_MousePositon[1]);
                float2 worldPosXY = float2(i.worldPos[0],i.worldPos[2]);
                float distanceMouse = distance(mousePosXY, worldPosXY);

                if(distanceMouse > _ShowDistanceMouse)
                {
                    col[3] = 0;
                }
                else
                {
                    float2 cellOffset = float2( i.worldPos[0]/_RasterCellSize - (int)(i.worldPos[0]/_RasterCellSize) ,
                                                i.worldPos[2]/_RasterCellSize - (int)(i.worldPos[2]/_RasterCellSize));
                    cellOffset = abs(cellOffset);
                    cellOffset = cellOffset * _RasterCellSize;

                    float2 cellMid = float2(_RasterCellSize,_RasterCellSize)/2;
                    float nomalizedDistanceToMid = max(abs( cellMid[0]-cellOffset[0]), abs(cellMid[1]-cellOffset[1]))/(_RasterCellSize/2);

                    col[3] = col[3] * pow(nomalizedDistanceToMid,_RasterLineStrength) * (1-distanceMouse/_ShowDistanceMouse);

                    
                }

                return col;
            }
            ENDCG
        }
    }
}
