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
                float4 vertex : POSITION; // 頂点座標
            };

            struct v2f
            {
                float4 pos : POSITION; // 頂点座標
                float3 worldPos : TEXCOORD0; // ワールド座標
            };

            float4 _BaseColor;
            float4 _RimColor;
            float _RimPower;

            // 入力 -> v2f
            v2f vert(input v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // 頂点座標をクリップ空間に変換
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // ワールド座標に変換
                return o;
            }

            // v2f -> 出力
            fixed4 frag(v2f i) : SV_Target
            {
                // カメラとピクセルの間のベクトル
                float3 viewDir = normalize(i.worldPos - _WorldSpaceCameraPos);

                // リムライトの計算
                float rimFactor = pow(1.0 - dot(viewDir, normalize(i.worldPos)), _RimPower);
                float4 rimColor = _RimColor * rimFactor;

                // 基本色にリムライトを加える
                return _BaseColor + rimColor;
            }
            ENDCG
        }
    }
}
