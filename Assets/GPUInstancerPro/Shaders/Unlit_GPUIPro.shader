Shader "GPUInstancerPro/Universal Render Pipeline/Unlit"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        _Cutoff("AlphaCutout", Range(0.0, 1.0)) = 0.5

        // BlendMode
        _Surface("__surface", Float) = 0.0
        _Blend("__mode", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [ToggleUI] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _BlendOp("__blendop", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _SrcBlendAlpha("__srcA", Float) = 1.0
        [HideInInspector] _DstBlendAlpha("__dstA", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0.0
        [HideInInspector] _AddPrecomputedVelocity("_AddPrecomputedVelocity", Float) = 0.0

        // Editmode props
        _QueueOffset("Queue offset", Float) = 0.0

        // ObsoleteProperties
        [HideInInspector] _MainTex("BaseMap", 2D) = "white" {}
        [HideInInspector] _Color("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        [HideInInspector] _SampleGI("SampleGI", float) = 0.0 // needed from bakedlit
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "UniversalMaterialType" = "Unlit"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100

        // -------------------------------------
        // Render State Commands
        Blend [_SrcBlend][_DstBlend], [_SrcBlendAlpha][_DstBlendAlpha]
        ZWrite [_ZWrite]
        Cull [_Cull]

        Pass
        {
            Name "Unlit"

            // -------------------------------------
            // Render State Commands
            AlphaToMask[_AlphaToMask]

            HLSLPROGRAM
#define UNIVERSAL_DOTS_PRAGMAS_INCLUDED // Added by GPUIPro

            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _SURFACE_TYPE_TRANSPARENT
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAMODULATE_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile _ DEBUG_DISPLAY
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitForwardPass.hlsl"
            
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" // Added by GPUIPro

#include_with_pragmas "Packages/com.gurbu.gpui-pro/Runtime/Shaders/Include/GPUInstancerSetup.hlsl"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing
ENDHLSL
        }

        // Fill GBuffer data to prevent "holes", just in case someone wants to reuse GBuffer for non-lighting effects.
        // Deferred lighting is stenciled out.
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }

            HLSLPROGRAM
#define UNIVERSAL_DOTS_PRAGMAS_INCLUDED // Added by GPUIPro

            #pragma target 4.5

            // Deferred Rendering Path does not support the OpenGL-based graphics API:
            // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
            #pragma exclude_renderers gles3 glcore

            // -------------------------------------
            // Shader Stages
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _ALPHAMODULATE_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitGBufferPass.hlsl"
            
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" // Added by GPUIPro

#include_with_pragmas "Packages/com.gurbu.gpui-pro/Runtime/Shaders/Include/GPUInstancerSetup.hlsl"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing
ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R

            HLSLPROGRAM
#define UNIVERSAL_DOTS_PRAGMAS_INCLUDED // Added by GPUIPro

            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" // Added by GPUIPro

#include_with_pragmas "Packages/com.gurbu.gpui-pro/Runtime/Shaders/Include/GPUInstancerSetup.hlsl"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing
ENDHLSL
        }

        Pass
        {
            Name "DepthNormalsOnly"
            Tags
            {
                "LightMode" = "DepthNormalsOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On

            HLSLPROGRAM
#define UNIVERSAL_DOTS_PRAGMAS_INCLUDED // Added by GPUIPro

            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT // forward-only variant
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"

            //--------------------------------------
            // GPU Instancing

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitDepthNormalsPass.hlsl"
            
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" // Added by GPUIPro

#include_with_pragmas "Packages/com.gurbu.gpui-pro/Runtime/Shaders/Include/GPUInstancerSetup.hlsl"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing
ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }

            // -------------------------------------
            // Render State Commands
            Cull Off

            HLSLPROGRAM
#define UNIVERSAL_DOTS_PRAGMAS_INCLUDED // Added by GPUIPro

            #pragma target 2.0

            // -------------------------------------
            // Shader Stages
            #pragma vertex UniversalVertexMeta
            #pragma fragment UniversalFragmentMetaUnlit

            // -------------------------------------
            // Unity defined keywords
            #pragma shader_feature EDITOR_VISUALIZATION

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitMetaPass.hlsl"
            
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" // Added by GPUIPro

#include_with_pragmas "Packages/com.gurbu.gpui-pro/Runtime/Shaders/Include/GPUInstancerSetup.hlsl"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing
ENDHLSL
        }

        Pass
        {
            Name "MotionVectors"
            Tags { "LightMode" = "MotionVectors" }
            ColorMask RG

            HLSLPROGRAM
#define UNIVERSAL_DOTS_PRAGMAS_INCLUDED // Added by GPUIPro

            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma shader_feature_local_vertex _ADD_PRECOMPUTED_VELOCITY

            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #ifndef UNIVERSAL_OBJECT_MOTION_VECTORS_INCLUDED
#define UNIVERSAL_OBJECT_MOTION_VECTORS_INCLUDED

#pragma target 3.5

#pragma vertex vert
#pragma fragment frag

//--------------------------------------
// GPU Instancing

#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

//-------------------------------------
// Other pragmas
#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"

// -------------------------------------
// Includes
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include_with_pragmas "Packages/com.gurbu.gpui-pro/Runtime/Shaders/Include/GPUInstancerSetup.hlsl"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MotionVectorsCommon.hlsl"

// -------------------------------------
// Structs
struct Attributes
{
    float4 position             : POSITION;
#if _ALPHATEST_ON
    float2 uv                   : TEXCOORD0;
#endif
    float3 positionOld          : TEXCOORD4;
#if _ADD_PRECOMPUTED_VELOCITY
    float3 alembicMotionVector  : TEXCOORD5;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS                 : SV_POSITION;
    float4 positionCSNoJitter         : POSITION_CS_NO_JITTER;
    float4 previousPositionCSNoJitter : PREV_POSITION_CS_NO_JITTER;
#if _ALPHATEST_ON
    float2 uv                         : TEXCOORD0;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

// -------------------------------------
// Vertex
Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    const VertexPositionInputs vertexInput = GetVertexPositionInputs(input.position.xyz);

    #if defined(_ALPHATEST_ON)
        output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    #endif

#if defined(APLICATION_SPACE_WARP_MOTION)
    // We do not need jittered position in ASW
    output.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.position));;
    output.positionCS = output.positionCSNoJitter;
#else
    // Jittered. Match the frame.
    output.positionCS = vertexInput.positionCS;
    output.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.position));
#endif

    float4 prevPos = (unity_MotionVectorsParams.x == 1) ? float4(input.positionOld, 1) : input.position;

#if _ADD_PRECOMPUTED_VELOCITY
    prevPos = prevPos - float4(input.alembicMotionVector, 0);
#endif

    output.previousPositionCSNoJitter = mul(_PrevViewProjMatrix, mul(UNITY_PREV_MATRIX_M, prevPos));

    return output;
}

// -------------------------------------
// Fragment
float4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if defined(_ALPHATEST_ON)
        Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    #endif

    #if defined(LOD_FADE_CROSSFADE)
        LODFadeCrossFade(input.positionCS);
    #endif

    #if defined(APLICATION_SPACE_WARP_MOTION)
        return float4(CalcAswNdcMotionVectorFromCsPositions(input.positionCSNoJitter, input.previousPositionCSNoJitter), 1);
    #else
        return float4(CalcNdcMotionVectorFromCsPositions(input.positionCSNoJitter, input.previousPositionCSNoJitter), 0, 0);
    #endif
}


#endif // UNIVERSAL_OBJECT_MOTION_VECTORS_INCLUDED

            ENDHLSL
        }

        Pass
        {
            Name "XRMotionVectors"
            Tags { "LightMode" = "XRMotionVectors" }
            ColorMask RGBA

            // Stencil write for obj motion pixels
            Stencil
            {
                WriteMask 1
                Ref 1
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
#define UNIVERSAL_DOTS_PRAGMAS_INCLUDED // Added by GPUIPro

            #pragma shader_feature_local _ALPHATEST_ON
            #pragma multi_compile _ LOD_FADE_CROSSFADE
            #pragma shader_feature_local_vertex _ADD_PRECOMPUTED_VELOCITY
            #define APLICATION_SPACE_WARP_MOTION 1
            #include "Packages/com.unity.render-pipelines.universal/Shaders/UnlitInput.hlsl"
            #ifndef UNIVERSAL_OBJECT_MOTION_VECTORS_INCLUDED
#define UNIVERSAL_OBJECT_MOTION_VECTORS_INCLUDED

#pragma target 3.5

#pragma vertex vert
#pragma fragment frag

//--------------------------------------
// GPU Instancing

#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

//-------------------------------------
// Other pragmas
#include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"

// -------------------------------------
// Includes
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include_with_pragmas "Packages/com.gurbu.gpui-pro/Runtime/Shaders/Include/GPUInstancerSetup.hlsl"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityInput.hlsl"

#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MotionVectorsCommon.hlsl"

// -------------------------------------
// Structs
struct Attributes
{
    float4 position             : POSITION;
#if _ALPHATEST_ON
    float2 uv                   : TEXCOORD0;
#endif
    float3 positionOld          : TEXCOORD4;
#if _ADD_PRECOMPUTED_VELOCITY
    float3 alembicMotionVector  : TEXCOORD5;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS                 : SV_POSITION;
    float4 positionCSNoJitter         : POSITION_CS_NO_JITTER;
    float4 previousPositionCSNoJitter : PREV_POSITION_CS_NO_JITTER;
#if _ALPHATEST_ON
    float2 uv                         : TEXCOORD0;
#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

// -------------------------------------
// Vertex
Varyings vert(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    const VertexPositionInputs vertexInput = GetVertexPositionInputs(input.position.xyz);

    #if defined(_ALPHATEST_ON)
        output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
    #endif

#if defined(APLICATION_SPACE_WARP_MOTION)
    // We do not need jittered position in ASW
    output.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.position));;
    output.positionCS = output.positionCSNoJitter;
#else
    // Jittered. Match the frame.
    output.positionCS = vertexInput.positionCS;
    output.positionCSNoJitter = mul(_NonJitteredViewProjMatrix, mul(UNITY_MATRIX_M, input.position));
#endif

    float4 prevPos = (unity_MotionVectorsParams.x == 1) ? float4(input.positionOld, 1) : input.position;

#if _ADD_PRECOMPUTED_VELOCITY
    prevPos = prevPos - float4(input.alembicMotionVector, 0);
#endif

    output.previousPositionCSNoJitter = mul(_PrevViewProjMatrix, mul(UNITY_PREV_MATRIX_M, prevPos));

    return output;
}

// -------------------------------------
// Fragment
float4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if defined(_ALPHATEST_ON)
        Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    #endif

    #if defined(LOD_FADE_CROSSFADE)
        LODFadeCrossFade(input.positionCS);
    #endif

    #if defined(APLICATION_SPACE_WARP_MOTION)
        return float4(CalcAswNdcMotionVectorFromCsPositions(input.positionCSNoJitter, input.previousPositionCSNoJitter), 1);
    #else
        return float4(CalcNdcMotionVectorFromCsPositions(input.positionCSNoJitter, input.previousPositionCSNoJitter), 0, 0);
    #endif
}


#endif // UNIVERSAL_OBJECT_MOTION_VECTORS_INCLUDED

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.UnlitShader"
}