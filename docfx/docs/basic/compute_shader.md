# 컴퓨트셰이더 (ComputeShader)

등장

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