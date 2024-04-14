Shader "VAT/VATUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VAT_Vertices ("VAT Vertices", 2D) = "white" {}
        _VAT_Frame_Index ("VAT Frame Index", Float) = 0
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
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                uint vertexId : SV_VertexID;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // GPU Instancing
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
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _VAT_Frame_Index)
            UNITY_INSTANCING_BUFFER_END(Props)
            float3 _VAT_Bounds_Min;
            float3 _VAT_Bounds_Max;

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                
                float x = v.vertexId + 0.5f;
                // float y = _Time.y + 0.5f;
                float y = UNITY_ACCESS_INSTANCED_PROP(Props, _VAT_Frame_Index) + 0.5f;
                float4 texelPosition = float4(x, y, 0, 0) * _VAT_Vertices_TexelSize;
                float4 rawColorPosition = tex2Dlod(_VAT_Vertices, texelPosition);
                uint rawColor =
                     ((uint)(rawColorPosition.r * 255) << 24)
                    +((uint)(rawColorPosition.g * 255) << 16)
                    +((uint)(rawColorPosition.b * 255) <<  8)
                    +((uint)(rawColorPosition.a * 255)      );
                float3 colorPosition = float3(
                    ((rawColor & 0xFFE00000) >> 21) / 2048.0f, // 0b11111111111000000000000000000000
                    ((rawColor & 0x001FF800) >> 11) / 1024.0f, // 0b00000000000111111111100000000000
                    ((rawColor & 0x000007FF)      ) / 2048.0f  // 0b00000000000000000000011111111111
                );
                
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
