using UnityEngine;
using System.Collections;

public class WreckingBall : Useable
{
    public BoxCollider area;
    public Rigidbody body;
    public float moveSpeed = 1.5f;

    Vector3 direction;
    float minX;
    float maxX;
    float minZ;
    float maxZ;

    public override void OnAwake()
    {
        base.OnAwake();

        var pos = transform.position;
        var boundSize = area.bounds.size * 0.5f;
        minX = pos.x - boundSize.x;
        maxX = pos.x + boundSize.x;
        minZ = pos.z - boundSize.z;
        maxZ = pos.z + boundSize.z;
        Destroy(area.GetComponent<MeshRenderer>());
    }

    public override void OnUpdateDirection(Vector3 direction)
    {
        this.direction = direction;
    }

    public override int OnAction()
    {
        // activate magnet
        return 1;
    }

    void FixedUpdate()
    {
        var newPos = body.position + direction * Time.deltaTime * moveSpeed;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);
        body.MovePosition(newPos);
    }
}
