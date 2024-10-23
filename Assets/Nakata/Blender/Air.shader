Shader "Maeda/Air"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(0.5, 5.0)) = 2.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }

        Pass
        {
            ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct input
            {
                float4 vertex : POSITION; // ���_���W
            };

            struct v2f
            {
                float4 pos : POSITION; // ���_���W
                float3 worldPos : TEXCOORD0; // ���[���h���W
            };

            float4 _BaseColor;
            float4 _RimColor;
            float _RimPower;

            // ���� -> v2f
            v2f vert(input v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // ���_���W���N���b�v��Ԃɕϊ�
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // ���[���h���W�ɕϊ�
                return o;
            }

            // v2f -> �o��
            fixed4 frag(v2f i) : SV_Target
            {
                // �J�����ƃs�N�Z���̊Ԃ̃x�N�g��
                float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);

                // �������C�g�̌v�Z
                float rimFactor = pow(1.0 - dot(viewDir, normalize(i.worldPos)), _RimPower);
                float4 rimColor = _RimColor * rimFactor;

                // ��{�F�Ƀ������C�g��������
                return _BaseColor + rimColor;
            }
            ENDCG
        }
    }
}
