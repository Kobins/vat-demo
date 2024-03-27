Shader "VAT/VATUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VAT_Vertices ("VAT Vertices", 2D) = "white" {}
        _VAT_Float ("VAT Float", Float) = 0
        [ShowAsVector3] _VAT_Bounds_Min ("VAT Bounds Min", Vector) = (0,0,0,0)
        [ShowAsVector3] _VAT_Bounds_Max ("VAT Bounds Max", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                uint vertexId : SV_VertexID;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _VAT_Vertices;
            float4 _VAT_Vertices_TexelSize;
            float4 _MainTex_ST;
            float _VAT_Float;
            float3 _VAT_Bounds_Min;
            float3 _VAT_Bounds_Max;

            v2f vert (appdata v)
            {
                float x = v.vertexId;
                // float y = frac(_Time.y * _VAT_Vertices_TexelSize.y);
                float y = _VAT_Float;
                float4 texelPosition = float4(x, y, 0, 0) * _VAT_Vertices_TexelSize;
                float4 colorPosition = tex2Dlod(_VAT_Vertices, texelPosition);
                float3 position = float3(
                    lerp(_VAT_Bounds_Min.x, _VAT_Bounds_Max.x, colorPosition.r),
                    lerp(_VAT_Bounds_Min.y, _VAT_Bounds_Max.y, colorPosition.g),
                    lerp(_VAT_Bounds_Min.z, _VAT_Bounds_Max.z, colorPosition.b)
                );
                
                
                v2f o;
                o.vertex = UnityObjectToClipPos(position);
                // o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
