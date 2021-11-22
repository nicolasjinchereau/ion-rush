using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConveyorPath : MonoBehaviour
{
    public Transform tform;
    public Transform beg;
    public Transform end;
    public PhysicMaterial conveyorPieceMaterial;
    public bool hideColliders = true;
    public bool interactable = true;

    public int numPieces = 18;
    public float pieceHeight = 0.05f;
    public float pieceWidth = 1.82f;
    public float pieceLength = 0.21f;
    public float conveyorRPM = 10.0f;

    private List<ConveyorPiece> pieces = new List<ConveyorPiece>();

    float spoolDiameter;
    float spoolCircumference;
    float spoolLength;
    float sideLength;
    float totalLength;
    float conveyorTime;
    float rpmScale;

    ConveyorPiece CreateConveyorPiece()
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "ConveyorPiece";
        go.layer = LayerMask.NameToLayer("ConveyorBelt");
        go.transform.localScale = new Vector3(pieceWidth, pieceHeight, pieceLength);

        Collider col = go.GetComponent<Collider>();
        col.sharedMaterial = conveyorPieceMaterial;

        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        Renderer rend = go.GetComponent<Renderer>();
        rend.enabled = !hideColliders;

        return go.AddComponent<ConveyorPiece>();
    }

    void Awake()
    {
        if (hideColliders)
        {
            foreach (Renderer rend in GetComponentsInChildren<Renderer>())
                Destroy(rend);

            foreach (MeshFilter filt in GetComponentsInChildren<MeshFilter>())
                Destroy(filt);
        }

        if (interactable)
        {
            while (pieces.Count < numPieces)
            {
                ConveyorPiece piece = CreateConveyorPiece();
                piece.mTransform.localScale = new Vector3(pieceWidth, pieceHeight, pieceLength);
                piece.mTransform.parent = transform;
                pieces.Add(piece);
            }

            foreach (ConveyorPiece piece in pieces)
            {
                Transform p = piece.mTransform.parent;
                piece.mTransform.parent = null;
                piece.mTransform.localScale = new Vector3(pieceWidth, pieceHeight, pieceLength);
                piece.mTransform.parent = p;
            }

            UpdateConveyorDimensions();
        }
    }

    void UpdateConveyorDimensions()
    {
        spoolDiameter = beg.lossyScale.x;
        spoolCircumference = Mathf.PI * spoolDiameter;
        spoolLength = beg.lossyScale.z;
        sideLength = Vector3.Distance(beg.position, end.position);
        totalLength = sideLength + spoolCircumference;
        rpmScale = (2.0f * sideLength + spoolCircumference) / (sideLength + spoolCircumference);
        
        float t = 0;
        float step = 1.0f / (float)numPieces;
        
        foreach(ConveyorPiece piece in pieces) {
            MovePiece(piece, t);
            t += step;
        }
    }

    void FixedUpdate()
    {
        if (interactable)
        {
            conveyorTime += (conveyorRPM * rpmScale / 60.0f) * Time.deltaTime;

            float t = conveyorTime;
            float step = 1.0f / (float)numPieces;

            foreach (ConveyorPiece piece in pieces)
            {
                MovePiece(piece, t);
                t += step;
            }
        }
    }
    
    void MovePiece(ConveyorPiece piece, float t)
    {
        float d = t * totalLength;
        d = Mathf.Repeat(d, totalLength);
        
        Quaternion rot;
        Vector3 pos;
        
        if(d < spoolCircumference * 0.5f) // left curve
        {
            d /= spoolCircumference * 0.5f;

            rot = tform.rotation * Quaternion.Euler(180.0f + d * 180.0f, 0, 0);
            pos = beg.position;
            pos += rot * (Vector3.up * (spoolDiameter * 0.5f - pieceHeight * 0.5f));
        }
        else if(d < spoolCircumference * 0.5f + sideLength) // top
        {
            d -= spoolCircumference * 0.5f;
            d /= sideLength;
            
            rot = tform.rotation;
            pos = beg.position;
            pos += tform.up * (spoolDiameter * 0.5f - pieceHeight * 0.5f);
            pos += tform.forward * sideLength * d;
        }
        else // right curve
        {
            d -= spoolCircumference * 0.5f + sideLength;
            d /= spoolCircumference * 0.5f;
            
            rot = tform.rotation * Quaternion.Euler(d * 180.0f, 0, 0);
            pos = end.position;
            pos += rot * (Vector3.up * (spoolDiameter * 0.5f - pieceHeight * 0.5f));
        }
        
        piece.mRigidbody.MovePosition(pos);
        piece.mRigidbody.MoveRotation(rot);
    }
}
