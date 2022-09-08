using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{
    [SerializeField] private float waterDrag = 3.0f;
    [SerializeField] private float waterAngularDrag = 1.0f;

    [SerializeField] private float airDrag = 0.0f;
    [SerializeField] private float airAngularDrag = 0.05f;

    [SerializeField] private float floatingPower = 15.0f;

    [SerializeField] private float waterHeight = 4.3f;

    private Rigidbody rigidbody;

    private bool inWater;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float diffrence = transform.position.y - waterHeight;

        if (diffrence < 0.0f)
        {
            rigidbody.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(diffrence), transform.position, ForceMode.Force);
        }
    }

    private void SwitchState(bool inWater)
    {
        if(inWater)
        {
            rigidbody.drag = waterDrag;
            rigidbody.angularDrag = waterAngularDrag;
        }
        else
        {
            rigidbody.drag = airDrag;
            rigidbody.angularDrag = airAngularDrag;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            inWater = true;
            SwitchState(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Water")
        {
            inWater = false;
            SwitchState(false);
        }
    }
}
