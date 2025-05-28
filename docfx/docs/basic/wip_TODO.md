
---


## HelloWorld

``` hlsl
#pragma kernel CSMain
RWTexture2D<float4> Result;

[numthreads(8,8,1)]
void CSMain (uint3 id : SV_DispatchThreadID)
{
    Result[id.xy] = float4(1, 0, 0, 1);
}
```

``` cs
RenderTexture renderTexture;
Material material;
ComputeShader shader;

// Shader Model 5.0 level pixel or compute shaders can write into arbitrary locations of some textures, called "unordered access views" in UsingDX11GL3Features
renderTexture.enableRandomWrite = true;
// Universal's Unlit using `_BaseMap` as Main Texture.
material.SetTexture("_MainTex", renderTexture);

// .compute: #pragma kernel CSMain
int kernelHandle = shader.FindKernel("CSMain");
// .compute: RWTexture2D<float4> Result;
shader.SetTexture(kernelHandle, "Result", renderTexture); 
```

``` cs
// .cs
shader.Dispatch(KernelHandle, 3, 2, 1);

// .compute
[numthreads(4, 4, 1)]
void CSMain (uint3 id : SV_DispatchThreadID)

// [total]
// (3 * 2 * 1) * (4 * 4 * 1) = 96 threads.
```

``` cs
new RenderTexture(width: 256, height: 256, depth: 0);
// 256 x 256
// (0, 256)    (256, 256)
//      +-------+
//      |   .   |
//      |   .   |
//      +-------+
// (0,   0)    (256,   0)

// GroupID
// Dispatch(int kernelIndex, int threadGroupsX, int threadGroupsY, int threadGroupsZ);
shader.Dispatch(kernelHandle, 256 /  8, 256 /  8, 1);
shader.Dispatch(kernelHandle, 256 / 16, 256 / 16, 1);
// 32 * 32 * 1 | 256 / 8  = 32
// 16 * 16 * 1 | 256 / 16 = 16

// ThreadID
[numthreads(8, 8, 1)]
void CSMain (uint3 id : SV_DispatchThreadID)
// 8 * 8 * 1

// DispatchedThreadID
// 32 * 8 = 256  | ThreadGroup * NumThreads
// 16 * 8 = 128  | ThreadGroup * NumThreads


// (0, 256)    (256, 256)
//      +-------+
//      |   .   |
//      |   .   |
//      +-------+
// (0,   0)    (256,   0)
// 
// (0, 128)    (128, 128)
//      +---+
//      |   |
//      +---+
// (0,   0)    (128,   0)
// 
// (32 * 32 * 1) * (8 * 8 * 1) = 65536
// (16 * 16 * 1) * (8 * 8 * 1) = 16384
```

## Buffer

- 구조체 넘길때 사용

|                             |                |                          |
| --------------------------- | -------------- | ------------------------ |
| StructuredBuffer\<T>        | 읽기 가능      |                          |
| RWStructuredBuffer\<T>      | 읽기/쓰기 가능 |                          |
| AppendStructuredBuffer\<T>  | push           | ComputeBufferType.Append |
| ComsumeStructuredBuffer\<T> | pop            |                          |


|                   |                                                                               | [GraphicsBuffer.Target](https://docs.unity3d.com/ScriptReference/GraphicsBuffer.Target.html) | [ComputeBufferType](https://docs.unity3d.com/6000.1/Documentation/ScriptReference/ComputeBufferType.html) |
| ----------------- | ----------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| Raw               | can be used as a raw byte-address buffer.                                     | O                                                                                            | O                                                                                                         |
| Append            | can be used as an append-consume buffer.                                      | O                                                                                            | O                                                                                                         |
| Counter           | with an internal counter.                                                     | O                                                                                            | O                                                                                                         |
| Constant          | can be used as a constant buffer (uniform buffer).                            | O                                                                                            | O                                                                                                         |
| Structured        | can be used as a structured buffer.                                           | O                                                                                            | O                                                                                                         |
| IndirectArguments | can be used as an indirect argument buffer for indirect draws and dispatches. | O                                                                                            | O                                                                                                         |
| -                 | -                                                                             | -                                                                                            | -                                                                                                         |
| CopySource        | can be used as a source for CopyBuffer.                                       | O                                                                                            | X                                                                                                         |
| CopyDestination   | can be used as a destination for CopyBuffer.                                  | O                                                                                            | X                                                                                                         |
| Index             | can be used as an index buffer.                                               | O                                                                                            | X                                                                                                         |
| Vertex            | can be used as a vertex buffer.                                               | O                                                                                            | X                                                                                                         |

## SRP

``` cs
cmd.Blit(_CameraSource, _Source);

// BaD
// _feature.shader.Dispatch(_handleKernel, _groupSize.x, _groupSize.y, 1);

// Good
cmd.DispatchCompute(_feature.shader, _handleKernel, _groupSize.x, _groupSize.y, 1);

cmd.Blit(_Output, _CameraSource);
```

``` cs
// Bad - gamma
// textureToMake = new RenderTexture(
//     _texSize.x / divide,
//     _texSize.y / divide,
//     0)
// {
//     enableRandomWrite = true
// };
// textureToMake.Create();

// Good - linear
textureToMake = new RenderTexture(
    _texSize.x / divide,
    _texSize.y / divide,
    0,
    format: RenderTextureFormat.ARGB32,
    readWrite: RenderTextureReadWrite.Linear)
{
    enableRandomWrite = true
};
textureToMake.Create();
```

## Shared

``` hlsl
// shared
#pragma kernel CS_1
#pragma kernel CS_2

shared Texture2D<float4> _Source;
shared RWTexture2D<float4> _Output;
```

``` hlsl
StructuredBuffer<Particle> _ParticleBuffer;

struct VStoFS
{
    float4 positionNDC  : SV_POSITION;
    float psize         : PSIZE;
}

// vertexID
//   point : 0
//   triangle : [0, 1, 2]
VStoFS vert(uint vertexID : SV_VertexID, uint instanceID : SV_InstanceID)
{
  Particle p = _ParticleBuffer[instanceID];
}
```
