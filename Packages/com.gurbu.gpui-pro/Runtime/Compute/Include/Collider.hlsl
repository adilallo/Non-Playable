﻿// GPU Instancer Pro
// Copyright (c) GurBu Technologies

#ifndef __collider_hlsl_
#define __collider_hlsl_

uniform float3 boundsCenter;
uniform float3 boundsExtents;

uniform float4x4 modifierTransform;
uniform float modifierRadius;
uniform float modifierHeight;
uniform float3 modifierAxis;

#include "Packages/com.gurbu.gpui-pro/Runtime/Compute/Include/Matrix.hlsl"

inline bool IsInsideBounds(float3 position)
{
    float3 Min = boundsCenter - boundsExtents;
    float3 Max = boundsCenter + boundsExtents;

    return position.x >= Min.x && position.x <= Max.x
    && position.y >= Min.y && position.y <= Max.y
    && position.z >= Min.z && position.z <= Max.z;
}

inline bool IsInsideBox(float3 position)
{
    float3 Min = boundsCenter - boundsExtents;
    float3 Max = boundsCenter + boundsExtents;

    float4 BoundingBox[8];
    BoundingBox[0] = mul(modifierTransform, float4(Min.x, Max.y, Min.z, 1.0)); // P5
    BoundingBox[1] = mul(modifierTransform, float4(Min.x, Max.y, Max.z, 1.0)); // P6
    BoundingBox[2] = mul(modifierTransform, float4(Max.x, Max.y, Max.z, 1.0)); // P7
    BoundingBox[3] = mul(modifierTransform, float4(Max.x, Max.y, Min.z, 1.0)); // P8
    BoundingBox[4] = mul(modifierTransform, float4(Max.x, Min.y, Min.z, 1.0)); // P4
    BoundingBox[5] = mul(modifierTransform, float4(Max.x, Min.y, Max.z, 1.0)); // P3
    BoundingBox[6] = mul(modifierTransform, float4(Min.x, Min.y, Max.z, 1.0)); // P2
    BoundingBox[7] = mul(modifierTransform, float4(Min.x, Min.y, Min.z, 1.0)); // P1

    // https://math.stackexchange.com/questions/1472049/check-if-a-point-is-inside-a-rectangular-shaped-area-3d

    //i=p2−p1
    float3 i = BoundingBox[6].xyz - BoundingBox[7].xyz;
    //j=p4−p1
    float3 j = BoundingBox[4].xyz - BoundingBox[7].xyz;
    //k=p5−p1
    float3 k = BoundingBox[0].xyz - BoundingBox[7].xyz;
    //v=pv−p1
    float3 v = position - BoundingBox[7].xyz;

    float vi = dot(v, i);
    float vj = dot(v, j);
    float vk = dot(v, k);
    //0<v⋅i<i⋅i
    return 0 < vi && vi < dot(i, i)
    //0<v⋅j<j⋅j
    && 0 < vj && vj < dot(j, j)
    //0<v⋅k<k⋅k
    && 0 < vk && vk < dot(k, k);

}

inline bool IsInsideSphere(float3 position)
{
    return distance(boundsCenter, position) <= modifierRadius;
}

inline bool IsInsideCapsule(float3 position)
{
    float pointChange = 0;
    if (modifierHeight / 2 > modifierRadius)
    {
        pointChange = (modifierHeight / 2) - modifierRadius;
        float4x4 scaled = SetScaleOfMatrix(modifierTransform, 1);

        // https://math.stackexchange.com/questions/1905533/find-perpendicular-distance-from-point-to-line-in-3d
        float3 A = position;
        float3 B = mul(scaled, float4(boundsCenter.x + modifierAxis.x * pointChange, boundsCenter.y + modifierAxis.y * pointChange, boundsCenter.z + modifierAxis.z * pointChange, 1.0)).xyz;
        float3 C = mul(scaled, float4(boundsCenter.x - modifierAxis.x * pointChange, boundsCenter.y - modifierAxis.y * pointChange, boundsCenter.z - modifierAxis.z * pointChange, 1.0)).xyz;

        float3 d = (C - B) / distance(C, B);
        float3 v = A - B;
        float t = dot(v, d);
        float3 P = B + t * d;
    
        float distPB = distance(P, B);
        float distPC = distance(P, C);
        float distBC = distance(B, C);

        if (abs(distBC - (distPB + distPC)) < 0.1)
            return distance(P, A) <= modifierRadius;
        else if (distPB < distPC)
            return distance(B, A) <= modifierRadius;
        else
            return distance(C, A) <= modifierRadius;
    }
    else
        return distance(boundsCenter + modifierTransform._14_24_34, position) <= modifierRadius;
}
#endif