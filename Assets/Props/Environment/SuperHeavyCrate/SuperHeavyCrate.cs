using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperHeavyCrate : MonoBehaviour
{
    public Rigidbody body;
    public BoxCollider boxCollider;

    Rigidbody kinematicBody;
    BoxCollider kinematicCollider;

    IEnumerator Start()
    {
        var crateLayer = LayerMask.NameToLayer("SuperHeavyCrate");
        var playerLayer = LayerMask.NameToLayer("Player");

        var cube = new GameObject("HeavyCreate3 Kinematic Body");
        cube.transform.localScale = new Vector3(1.125f, 1.125f, 1.125f);
        cube.layer = crateLayer;

        kinematicBody = cube.AddComponent<Rigidbody>();
        kinematicBody.isKinematic = true;

        for(int i = 0; i < 31; ++i)
            Physics.IgnoreLayerCollision(crateLayer, i, i != playerLayer);

        kinematicCollider = cube.AddComponent<BoxCollider>();

        while(!Player.that)
            yield return null;

        Physics.IgnoreCollision(boxCollider, Player.that.col);
    }

    private void FixedUpdate()
    {
        kinematicBody.MovePosition(body.position);
        kinematicBody.MoveRotation(body.rotation);
    }
}
