using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 설명 : 
public class Test_Impulse : MonoBehaviour
{
    [Range(1f, 100f)]
    public float _force = 10f;

    [Range(0.1f, 2f)]
    public float _uncontrollableTime = 0.5f;

    public ForceMode _forceMode = ForceMode.Impulse;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger");

        Vector3 dir = (other.transform.position - transform.position).normalized;

        var ccc = other.GetComponent<Rito.CharacterControl.CharacterMainController>();
        if(ccc) ccc.KnockBack(dir * _force, _uncontrollableTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Vector3 dir = (hit.transform.position - transform.position).normalized;
        hit.rigidbody.AddForce(dir * _force, _forceMode);

        Debug.Log("CC Hit");
    }
}
