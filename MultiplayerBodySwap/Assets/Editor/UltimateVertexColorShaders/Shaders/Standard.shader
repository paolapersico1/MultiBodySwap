// © 2020 Michael Squiers
Shader "UltimateVertexColorShaders/Standard"
{
    Properties{
        _Albedo("Albedo", Range(0,8)) = 2.2
        _Metallic("Metallic", Range(0,1)) = 0.1
        _Emission("Emission", Range(0,1)) = 0
        _EmissionColor("Emission Color", Color) = (255,255,255,255)
        _Smoothness("Smoothness", Range(0,1)) = 0      
        _Alpha("Alpha", Range(0,1)) = 1
        [HideInInspector] _Mode("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0

    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 200

        Blend[_SrcBlend][_DstBlend]
        ZWrite[_ZWrite]

          CGPROGRAM
        // #pragma surface surf Standard vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
          #pragma surface surf Standard vertex:vert keepalpha
          #pragma target 3.0
          #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
          struct Input
          {
              float4 vertColor;
              float2 uv_MainTex;
          };

          float _Albedo;
          float _Alpha;
          half _Metallic;
          half _Smoothness;
          half _Emission;
          float4 _EmissionColor;

          void vert(inout appdata_full v, out Input o)
          {
              UNITY_INITIALIZE_OUTPUT(Input, o);
              o.vertColor.x = v.color.r;
              o.vertColor.y = v.color.g;
              o.vertColor.z = v.color.b;
              o.vertColor.w = _Alpha;
          }

          void surf(Input IN, inout SurfaceOutputStandard o)
          {
               o.Albedo = LinearToGammaSpace(pow(IN.vertColor.rgb, _Albedo));
               o.Metallic = _Metallic;
               o.Smoothness = _Smoothness;
               o.Emission = _Emission * _EmissionColor;
               o.Alpha = _Alpha;
           }
           ENDCG
    }
        CustomEditor "UVCS_StandardShaderGUI"
}