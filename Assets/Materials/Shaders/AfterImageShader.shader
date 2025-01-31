// This shader fills the mesh shape with a color predefined in the code.
Shader "AfterImageShader"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {        
        [MainColor] _MainColor ("Main Color", Color) = (1, 0, 0, 1)
        _OffColor ("Off Color", Color) = (1, 0, 0, 1)
        [PerRendererData] _Opacity ("Opacity", Range(0, 1)) = 1.0
        _LineThickness("LineThickness", Range(0.01,5.0)) = 1.0
        _LineStep("LineStep", Range(0.01,1.0)) = 0.2
        _LineModulus("LineModulus", Range(0.01,1.0)) = 0.2
        _LineSpeed("LineSpeed", Range(0.01,25.0)) = 1.0
        
}

    // The SubShader block containing the Shader code. 
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
       
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalRenderPipeline"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "AfterImagePass"

            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            // This line defines the name of the vertex shader. 
            #pragma vertex vert
            // This line defines the name of the fragment shader. 
            #pragma fragment frag

            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            
            
            CBUFFER_START(UnityPerMaterial)
            half _Opacity;
            CBUFFER_END

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;                 
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float3 worldPos     : TEXCOORD0;
            };            
            

            half4 _MainColor;
            half4 _OffColor;
            half _LineThickness;
            half _LineStep;
            half _LineModulus;
            half _LineSpeed;

            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS).xyz;
                // Returning the output.
                return OUT;
            }

            // The fragment shader definition.            
            half4 frag(Varyings IN) : SV_Target
            {
                float worldThicknessLineSpeed = ((IN.worldPos.y * _LineThickness) + (_Time * _LineSpeed));
                half calculateOpacity = step(_LineStep, fmod(worldThicknessLineSpeed, _LineModulus));
                // Defining the color variable and returning it.
                half4 customColor = lerp(_MainColor, _OffColor, calculateOpacity);
                customColor.a = min(_Opacity,customColor.a);
                return customColor;
            }
            ENDHLSL
        }
    }
}


// Shader "AfterImageShader"
// {
//     Properties
//     {
//         _MainColor ("Main Color", Color) = (1, 0, 0, 1)
//         _Opacity ("Opacity", Range(0, 1)) = 1.0
//     }

//     SubShader
//     {
//         Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }

//         Pass
//         {
//             HLSLPROGRAM
//             #pragma vertex vert
//             #pragma fragment frag

//             #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

//             struct Attributes
//             {
//                 float4 positionOS   : POSITION;
//             };

//             struct Varyings
//             {
//                 float4 positionHCS  : SV_POSITION;
//                 float3 worldPos     : TEXCOORD0;
//             };

//             half4 _MainColor;
//             half _Opacity;

//             Varyings vert (Attributes IN)
//             {
//                 Varyings OUT;
//                 OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
//                 OUT.worldPos = TransformObjectToWorld(IN.positionOS).xyz;
//                 return OUT;
//             }

//             half4 frag (Varyings IN) : SV_Target
//             {
//                 float lineSpeed = 1.0;
//                 float lineThickness = 0.1;
//                 float alpha = _Opacity * (sin((IN.worldPos.y + _Time * lineSpeed) * 20.0) * 0.5 + 0.5) * step(lineThickness, frac(IN.worldPos.y + _Time * lineSpeed));
                
//                 half4 color = _MainColor;
//                 color.a *= alpha;
//                 return color;
//             }
//             ENDHLSL
//         }
//     }
//     FallBack "Transparent/Cutout/Diffuse"
// }
