Shader "Custom/CottonCandyShader"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 0.5, 1, 1)
        _MainTex ("Base Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _FresnelPower ("Fresnel Power", Range(0.1, 5.0)) = 2.0
        _Transparency ("Transparency", Range(0.0, 1.0)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        Lighting Off

        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _Color;
            float _FresnelPower;
            float _Transparency;
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            float4 frag (v2f i) : SV_Target
            {
                // �x�[�X�J���[�ƃm�C�Y�e�N�X�`��������
                float4 baseColor = tex2D(_MainTex, i.uv) * _Color;
                float noise = tex2D(_NoiseTex, i.uv).r;
                // Fresnel���ʂ��v�Z���āA�ӂ���Ƃ������E��\��
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 normal = float3(0, 0, 1); // �ȈՓI�Ȗ@���x�N�g��
                float fresnel = pow(1.0 - dot(viewDir, normal), _FresnelPower);
                // ���S���Z���A�O���������ɂȂ�悤�ɓ����x�𒲐�
                float alpha = lerp(_Transparency, 1.0, fresnel) * noise;
                baseColor.a *= alpha;
                return baseColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}