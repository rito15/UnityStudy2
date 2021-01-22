## Unity Study 2
(2021. 01. 05 ~)

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
  
### 구현 예시

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
 
 ###
 
 - 거리 계산
 <center><img src="https://user-images.githubusercontent.com/42164422/104995624-f0072c80-5a69-11eb-9888-15b0f89edd41.png" width="500"></center>

 - 노멀 계산
 <center><img src="https://user-images.githubusercontent.com/42164422/104995731-1927bd00-5a6a-11eb-8f0b-c63f60abe394.png" width="500"> </center>

 - 라이트(Directional Light) 계산
 <center><img src="https://user-images.githubusercontent.com/42164422/104995793-2e045080-5a6a-11eb-86db-8c7601d12846.png" width="500"> </center>
 
 - 간단한 구현 예시 (https://github.com/SebLague/Ray-Marching 활용)
 <center><img src="https://user-images.githubusercontent.com/42164422/105003713-5f831900-5a76-11eb-8090-bd2e8d6f9b87.png" width="500"></center>
 
###
 
### [2] 장점
 - 곡면을 부드럽게 렌더링할 수 있다.
 - 거리 함수, 연산 함수들을 이용하여 모델들을 다양하고 부드럽게 블렌딩하기에 좋다.
 - 각 레이를 GPU 연산을 통해 병렬적으로 연산하기에 적합하다.
 
### [3] 단점
 - 성능 소모가 크다.
 <img src="https://user-images.githubusercontent.com/42164422/105004241-023b9780-5a77-11eb-9d91-015809da2d88.png" width="500">
 
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
  

#

# Fog of War (전장의 안개)
## [1] 개념
 - 맵 전체에 걸쳐 RGBA(0, 0, 0, a)의 텍스쳐를 씌워 시야를 표현한다.
 - 지정한 유닛이 현재 위치한 영역들은 a = 0,
 - 지정한 유닛이 한 번이라도 위치했던 영역들은 a = 0.5~0.8,
 - 지정한 유닛이 한 번도 방문하지 않은 영역은 a = 1로 표현한다.

###

## [2] 구현 방법
### [2-1] 카메라와 지상 사이에 안개 플레인 사용
 - 시야의 역할을 해줄 검정색 플레인을 카메라와 지상 사이에 위치시킨다.
 - 맵 전체를 좌표 형태의 2차원배열로 관리하여, 유닛들이 현재 위치한 영역, 방문했던 영역, 한 번도 방문하지 않은 영역의 정보를 실시간으로 저장한다.
 - 카메라와 해당 유닛들 사이에서 시야를 적용할 안개 플레인의 로컬좌표를 구하고 정점 색상을 변경시킨다.
  - 좌표를 구하는 방법 : 레이캐스트 또는 비례식 사용 (여기서는 비례식)

<img src="https://user-images.githubusercontent.com/42164422/105534824-6a1d0700-5d31-11eb-8125-3610abd888b3.png" width="500">

<img src="https://user-images.githubusercontent.com/42164422/105534838-6e492480-5d31-11eb-8914-546fc287e45f.png" width="500">

###

### [2-2] 타일맵 기반 구현

<img src="https://user-images.githubusercontent.com/42164422/105534813-65f0e980-5d31-11eb-9151-9d8859e12acc.png" width="500">

###

### 타일맵
- 정사각형 타일 하나의 가로,세로 너비와 전체 안개 플레인의 가로, 세로 너비를 결정한다. (예 : 타일 너비 0.5, 안개 20x20)

- 게임 시작 시 각각의 타일마다 지형의 높이(position.y)를 계산해 2차원 배열로 저장한다. (-Y 방향 레이캐스트 이용, 배열의 크기는 전체 타일 개수(플레인 가로 너비 * 세로 너비))

###

### 유닛
- 시야를 밝힐 대상 유닛들은 리스트를 통해 실시간으로 관리된다.

###

### Visit 배열
- 배열의 크기는 타일의 개수와 같다.
- Visit.current 배열은 현재 유닛들의 시야가 유지되는 타일들에 대해 true 값을 가지며, 시야 계산이 끝날 때마다 전체 false로 초기화된다.
- Visit.ever 배열은 한 번이라도 시야가 확보됐던 타일들에 대해 true값을 가지며, 한 번 true가 된 타일은 항상 그 값을 true로 유지한다.
- 유닛들의 위치를 중심으로 반복문을 통해, 주기적으로 Visit 배열을 갱신한다.

###

### 시야 계산
- 주기적으로(0.2~0.5초) 각 유닛들이 위치한 타일 기준으로 주변의 시야를 계산한다.

  - [1] 시야만큼의 원형범위 내 모든 타일들을 가져온다.
   <img src="https://user-images.githubusercontent.com/42164422/105534301-a56b0600-5d30-11eb-9c2c-1695ecf657cc.png" width="250">
   
  - [2] 현재 유닛 타일보다 높은 곳에 위치한 타일들은 배제한다.
   <img src="https://user-images.githubusercontent.com/42164422/105534318-aa2fba00-5d30-11eb-8f32-303b340ecd35.png" width="250">
   
  - [3] 유닛 타일과 각각의 타일 사이에 유닛 타일보다 더 높은 타일이 존재하는 경우, 해당 타일을 배제한다.
   <img src="https://user-images.githubusercontent.com/42164422/105534332-adc34100-5d30-11eb-84e1-e3acde754022.png" width="250">

  - [4] 결과로 얻은 타일들에 대해, Visit.current 및 Visit.ever 배열의 값을 true로 초기화한다,
   <img src="https://user-images.githubusercontent.com/42164422/105534342-b156c800-5d30-11eb-81d9-76480f4c9dcb.png" width="250">

###

### Fog 텍스쳐
- 텍스쳐의 알파값 저장을 위한 Color 배열이 필요하며, 크기는 타일의 개수와 같다.
- Fog 텍스쳐의 넓이는 타일의 가로 개수 * 세로 개수와 같다.
- 시야 계산이 끝날 때마다 Visit.current, Visit.ever 값에 따라 Color 배열의 A 값을 초기화한다. (current -> 0, ever -> 0.5~0.8, 그 외 1)
- Color 배열을 텍스쳐에 적용한다.

###

### Fog 쉐이더
- Fog 쉐이더는 ZTest Off로 설정하여 항상 다른 오브젝트들 위에 보이게 한다.

###

### 블러, 보간 효과
- 자연스러운 블러 효과를 위해, 가우시안 블러를 적용하는 쉐이더를 사용한다.
- 매 프레임마다 이전 프레임의 시야 텍스쳐를 현재 프레임의 텍스쳐에 부드럽게 보간하여 적용한다.
- 부자연스러운 픽셀이 나타나는 것을 방지하기 위해 렌더 텍스쳐를 여러 장 거쳐 보간을 적용한다.

###

<img src="https://user-images.githubusercontent.com/42164422/105534346-b451b880-5d30-11eb-9f4a-3bb0b35c069b.gif" width="500">

<img src="https://user-images.githubusercontent.com/42164422/105534357-b6b41280-5d30-11eb-9ffa-c6a6e3d9ab3c.gif" width="500">

### Reference
  - https://github.com/QinZhuo/FogOfWar_ForUnity


