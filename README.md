## Unity Study 2
(2020. 09. 15 ~)

- Toys
https://github.com/rito15/Unity_Toys

------
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
