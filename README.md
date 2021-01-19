## Unity Study 2
(2020. 09. 15 ~)

- Toys

https://github.com/rito15/Unity_Toys

#

# Behavior Tree
- FSM (Finite State Machine)의 단점을 보완하기 위해 만들어진 기법
- FSM에서는 상태 전이 조건을 모두 각각의 상태에서 검사하지만, BT에서는 상태 동작 뿐만 아니라 전이 조건도 노드로 관리한다.
- 노드 그래프를 통해 시각화하거나 params, 빌더 패턴 등을 활용하여 스크립트 내에서도 가독성 좋게 구성할 수 있다.
- 기본적으로 Leaf, Decorator, Composite 노드를 기반으로 하며, 구현은 많이 다를 수 있다.
  - Leaf : 동작을 수행하는 노드. 대표적으로 Action 또는 Task 노드가 있다.
  - Decorator : 다른 노드에 조건을 붙여 수식하는 노드
  - Composite : 자식 노드들을 가지며, 자식들을 순회하거나 선택하는 역할을 수행하는 노드

<img src="https://user-images.githubusercontent.com/42164422/104877006-31310b00-599c-11eb-9115-bc883072ceac.png" width="500">

- 모든 노드는 실행의 결과로 true 또는 false를 리턴한다.
- Action 노드는 동작을 수행하는 역할을 하며, 무조건 true를 리턴한다.
- Condition 노드는 조건식을 검사하여, 그 결과를 리턴한다.
- NotCondition 노드는 조건식의 결과를 반대로 리턴한다.

- IfAction 노드는 Action 노드를 조건 노드가 수식하는 꼴을 하나의 노드로 통합한 형태이다. 조건 수행 결과가 false인 경우 즉시 종료하며 false를 리턴하고, true인 경우 Action을 수행하며 true를 리턴한다.

- Selector 노드는 자식들을 차례대로 순회하며 자식이 false일 경우 계속해서 다음 자식 노드를 실행하고, true일 경우 순회를 중지하여 결국 true인 하나의 자식 노드만 선택하는 형태가 된다.
  모든 자식 노드가 false일 경우 Selector 노드도 false를 리턴하며, true인 자식 노드를 만난 경우 즉시 true를 리턴하고 종료한다.
- Sequence 노드는 자식들을 차례대로 순회하며 자식이 true일 경우 계속해서 다음 자식 노드를 실행하고, false일 경우 순회를 중지한다.
  모든 자식 노드가 true인 경우 Sequence 노드도 true를 리턴하며, false인 자식이 존재하는 경우 즉시 false를 리턴하고 종료한다.
- Parallel 노드는 자식 노드들의 실행 결과에 관계 없이 모든 자식 노드를 순회한다.

#

# Ray Marching

### [1] 레이 마칭이란?
 - 폴리곤의 정점 정보를 이용해 렌더링하는 기존의 3D 렌더링 방식과는 다른 기법.
 
 - 레이 마칭의 모든 모델링들은 폴리곤이 아닌 거리 함수(SDF : Signed Distance Function)로 표면의 정보가 계산된다.
 
<center><img src="https://user-images.githubusercontent.com/42164422/104993172-ce0bab00-5a65-11eb-9eda-705de2034f17.png" width="500"></center>
 
 ###
 
 - 한 점(RO : Ray Origin, 위의 그림에서 Camera)에서 스크린의 각각의 픽셀을 향한 방향(RD : Ray Direction, 위의 그림에서 Image)들을 향해
   레이 캐스팅을 하여, 각 레이마다 여러 스텝(Step)으로 나누어 레이를 전진시키게 된다.
   
<center> <img src="https://user-images.githubusercontent.com/42164422/104993811-c1d41d80-5a66-11eb-9ad3-a861471cce8e.png" width="500"> </center>
   
###
   
 - 한 번의 스텝마다 존재하는 모든 SDF를 각각 계산하여 현재 레이 위치로부터 각 모델들과의 거리를 얻어낸다.

 - 그 중 현재 레이의 위치로부터 가장 가까운 거리값만큼 레이를 이동한다.
 
 - 레이의 다음 전진 거리가 매우 작으면(예: dS < 0.01) 해당 위치가 물체의 표면이라고 판단하고, 레이의 전진을 중단한다.
 
 - 물체의 표면을 알아내지 못했는데 전진 횟수가 MAX_STEPS를 넘어서면 해당 픽셀에는 모델의 표면이 존재하지 않는다고 판단한다.
 
 - 각 픽셀들에 대한 계산이 끝나면 표면의 노말과 라이팅 계산을 적용한다.
 
 <center><img src="https://user-images.githubusercontent.com/42164422/104995624-f0072c80-5a69-11eb-9888-15b0f89edd41.png" width="500"></center>

 - 거리 계산

 <center><img src="https://user-images.githubusercontent.com/42164422/104995731-1927bd00-5a6a-11eb-8f0b-c63f60abe394.png" width="500"> </center>

 - 노멀 계산

 <center><img src="https://user-images.githubusercontent.com/42164422/104995793-2e045080-5a6a-11eb-86db-8c7601d12846.png" width="500"> </center>
 
 - 라이트(Directional Light) 계산
 
### [2] 장점
 - 곡면을 부드럽게 렌더링할 수 있다.
 - 거리 함수, 연산 함수들을 이용하여 모델들을 다양하고 부드럽게 블렌딩하기에 좋다.
 - 각 레이를 GPU 연산을 통해 병렬적으로 연산하기에 적합하다.
 
### [3] 단점
 - 성능 소모가 크다.
 
### [4] 연관 개념
 - https://blog.hybrid3d.dev/2019-11-15-raytracing-pathtracing-denoising

 - 레이 트레이싱(Ray Traycing)
   - 눈(RO)에서 출발한 빛이 광원에 도달할 때까지, 물체의 표면에 굴절되고 반사되는 것을 추적하는 기법
   - 기본적인 레이 트레이싱은 주로 반사/스페큘러 계산에 사용
 
 - 패스 트레이싱(Path Traycing)
   - 레이 트레이싱을 이용해 디퓨즈(Diffuse) 및 스페큘러(Specular), 전역 조명(GI, Global Illumination)을 계산하는 기법
    

### References
  - http://jamie-wong.com/2016/07/15/ray-marching-signed-distance-functions
  - https://www.youtube.com/watch?v=PGtv-dBi2wE [The Art of Code]
  - https://www.youtube.com/watch?v=Cp5WWtMoeKg [Sebastian Lague]
  - https://github.com/SebLague/Ray-Marching
