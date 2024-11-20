Shader "Unlit/ParticleShader"
{
    Properties
    {
        //Main
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags{"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        Pass
        {
            Blend ONE ONE
            Cull BACK
            Lighting Off 
            ZWrite off
          
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                half4 normal : NORMAL;
                float4 texcoords : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                half4 color : COLOR;
            };

            //Main
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv.xy = TRANSFORM_TEX(v.texcoords.xy,_MainTex);
                return o;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
