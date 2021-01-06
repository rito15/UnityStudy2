using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rito.BehaviorTree
{
    /// <summary> BT 공통 데이터 관리 </summary>
    public interface IData { }
    /// <summary> BT 공통 상태 변수 관리 </summary>
    public interface IState { }
    /// <summary> BT 공통 동작 관리 </summary>
    public interface ICore { }

    /// <summary> 
    /// 최상위 노드 인터페이스
    /// <para/> 상속 노드 클래스에서 Bank, State, Core 필드로 캐싱
    /// </summary>
    public interface INode 
    {
        bool Run();
    }

    /// <summary>
    /// 상속 클래스에서 ISelectorNode, ICompositeNode 상속
    /// <para/> - 하위 노드들 순회 중 true를 받으면 순회 종료
    /// <para/> - 하위 노드들 중 하나만 실행하게 됨
    /// </summary>
    public interface ISelectorNode : INode { }

    /// <summary> 
    /// 상속 클래스에서 ISequenceNode, ICompositeNode 상속
    /// <para/> - 하위 노드들 순회 중 false를 받으면 순회 종료
    /// <para/> - false를 받기 전까지 하위 노드를 모두 순회하게 됨
    /// </summary>
    public interface ISequenceNode : INode { }

    /// <summary> 조건 검사 코드 </summary>
    public interface IConditionNode : INode { }

    /// <summary> Bank, State, Core에 명시적으로 접근하기 위한 노드 </summary>
    public interface IUpdateNode : INode { }

    /// <summary> 행동 수행 노드 </summary>
    public interface IActionNode : INode { }

    /// <summary> 리스트로 하위 노드들을 갖고 순회하기 위한 노드 </summary>
    public interface ICompositeNode : INode 
    {
        ///// <summary> 리스트에 새로운 노드 등록 </summary>
        //ICompositeNode Add(INode childNode);
    }
}