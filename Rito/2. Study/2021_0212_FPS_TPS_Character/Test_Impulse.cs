using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 설명 : 
public class Test_Impulse : MonoBehaviour
{
    [Range(1f, 100f)]
    public float _force = 10f;

    public ForceMode _forceMode = ForceMode.Impulse;

    private void OnTriggerEnter(Collider other)
    {

        var rb = other.GetComponent<Rigidbody>();
        var pbm = other.GetComponent<Rito.CharacterControl.PhysicsBasedMovement>();

        if(!rb || !pbm) return;

        Vector3 dir = (other.transform.position - transform.position).normalized;
        //dir.y = 0.05f - (other.transform.position.y - transform.position.y) * 0.5f;

        rb.AddForce(dir * _force, _forceMode);
        pbm.SetOutOfControl(1f);

        Debug.Log("Knock Back !");
    }
}
