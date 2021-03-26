# ComputeShader

- 일반 렌더링 파이프라인과 별도로 그래픽 카드에서 실행되는 프로그램.
- 그래픽스와 관련이 없는 환경에서도 GPU의 성능을 활용할 수 있는 API 필요
  - GPGPU API등장 (General-purpose computing on graphics processing units)

| GPGPU API     |               |
|---------------|---------------|
| OpenCL        | Khronos Group |
| CUDA C        | Nvidia        |
| DirectCompute | Microsoft     |

| Platform | Graphic Library                                         |
|----------|---------------------------------------------------------|
| Windows  | OpenGL 4.3 / Vulkan / DirectX 11,12, / Shader Model 5.0 |
| Linux    | OpenGL 4.3 / Vulkan                                     |
| macOS    | Metal                                                   |
| Android  | OpenGL ES 3.1 / Vulkan                                  |
| iOS      | Metal                                                   |

| 렌더 파이프라인            | 독립           |
|----------------------------|----------------|
| `VS` > HS > DS > GS > `PS` | Compute Shader |

## Group / Thread / Kernel

|        |                                 |                |
|--------|---------------------------------|----------------|
| Group  | 쓰레드의 그룹. 동시 쓰레드 실행 | .Dispatch      |
| Thread | 커널 실행하는 단위              | [numthreads]   |
| Kernel | GPU가 처리하는 함수             | #pragma kernel |

|                     |                                                    |
|---------------------|----------------------------------------------------|
| SV_GroupID          | .Dispatch(3, 2, 1)                                 |
| SV_GroupThreadID    | [numthreads(2, 2, 1)]                              |
| SV_DispatchThreadID | SV_GroupID * numthreads + SV_GroupThreadID         |
| SV_GroupIndex       | [0, (numthreadsX * numthreadsY * numThreadsZ) – 1] |

![./res/dispatch_ids.jpg](./res/dispatch_ids.jpg)

``` cs
/// ==================== .cs
_shader.Dispatch(kernelIndex, 3, 2, 1);

/// ==================== .compute
[numthreads(2, 2, 1)]

uint3 groupID    : SV_GroupID
uint3 threadID   : SV_GroupThreadID
uint3 dispatchID : SV_DispatchThreadID
uint  groupIndex : SV_GroupIndex

// SV_DispatchThreadID = SV_GroupID * numthreads + GroupThreadID
dispatchID == groupID * uint3(2, 2, 1) + threadID;

// SV_GroupIndex =   SV_GroupThreadID.z * numthreads.x * numthreads.y
//                 + SV_GroupThreadID.y * numthreads.x
//                 + SV_GroupThreadID.x
groupIndex == threadID.z * 2 * 2 + threadID.y * 2 + threadID.x;

uint index = dispatchID.x + dispatchID.y * 6;

_Buffer[index] = index;
//  0  1  2  3  4  5
//  6  7  8  9 10 11
// 12 13 14 15 16 17
// 18 19 20 21 22 23

_Buffer[index] = groupID.x;   | _Buffer[index] = groupID.y;
//  0  0  1  1  2  2          |  0  0  0  0  0  0
//  0  0  1  1  2  2          |  0  0  0  0  0  0
//  0  0  1  1  2  2          |  1  1  1  1  1  1
//  0  0  1  1  2  2          |  1  1  1  1  1  1
_Buffer[index] = threadID.x;  | _Buffer[index] = threadID.y;
//  0  1  0  1  0  1          |  0  0  0  0  0  0
//  0  1  0  1  0  1          |  1  1  1  1  1  1
//  0  1  0  1  0  1          |  0  0  0  0  0  0
//  0  1  0  1  0  1          |  1  1  1  1  1  1
_Buffer[index] = dispatchID.x;| _Buffer[index] = dispatchID.y
//  0  1  2  3  4  5          |  0  0  0  0  0  0
//  0  1  2  3  4  5          |  1  1  1  1  1  1
//  0  1  2  3  4  5          |  2  2  2  2  2  2
//  0  1  2  3  4  5          |  3  3  3  3  3  3

_Buffer[index] = groupIndex;
//  0  1  0  1  0  1
//  2  3  2  3  2  3
//  0  1  0  1  0  1
//  2  3  2  3  2  3
```

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
|-----------------------------|----------------|--------------------------|
| StructuredBuffer\<T>        | 읽기 가능      |                          |
| RWStructuredBuffer\<T>      | 읽기/쓰기 가능 |                          |
| AppendStructuredBuffer\<T>  | push           | ComputeBufferType.Append |
| ComsumeStructuredBuffer\<T> | pop            |                          |

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

## Ref

- <https://docs.unity3d.com/ScriptReference/ComputeShader.html>
- <https://www.udemy.com/course/compute-shaders/>
  - <https://github.com/NikLever/UnityComputeShaders>
- [youtube: Compute Shaders: Optimize your engine using compute / Lou Kramer, AMD](https://www.youtube.com/watch?v=0DLOJPSxJEg)
- <https://catlikecoding.com/unity/tutorials/basics/compute-shaders/>
- <https://www.ronja-tutorials.com/post/050-compute-shader/>
- [ScrawkBlog](http://scrawkblog.com/) - 링크깨짐
  - <https://cheneyshen.com/category/all/gpu/compute-shader/>
  - <http://diskhkme.blogspot.com/2015/12/unity-directcompute-1.html>
