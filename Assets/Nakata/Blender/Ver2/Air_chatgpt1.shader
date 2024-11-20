Shader "Custom/CottonCandyShader_2"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 0.5, 1, 1)
        _ExpandAmount ("Expand Amount", Float) = 0.5
        _Density ("Density", Float) = 5.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent"  "IgnoreProjector"="True""RenderType" = "Transparent" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            fixed4 _Color;
            float _ExpandAmount;
            float _Density;

            v2f vert (appdata v)
            {
                v2f o;
                
                // ���_��@�������ɉ����o��
                float3 worldNormal = normalize(mul((float3x3)unity_WorldToObject, v.normal));
                float3 offsetPos = v.vertex.xyz + worldNormal * _ExpandAmount;
                
                // �ʒu���Ɩ@������ۑ�
                o.worldPos = mul(unity_ObjectToWorld, float4(offsetPos, 1.0)).xyz;
                o.worldNormal = worldNormal;
                o.pos = UnityObjectToClipPos(float4(offsetPos, 1.0));

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // ���S����̋����Ɋ�Â������x�̌v�Z
                float distFromCenter = length(i.worldPos);
                float alpha = saturate(1.0 - distFromCenter / _Density);

                // �J���[�ƃA���t�@�̓K�p
                fixed4 col = _Color;
                col.a *= alpha;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
