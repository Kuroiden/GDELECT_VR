using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class ObjectShatter : MonoBehaviour
{
    Rigidbody MainRB;
    Vector3 ObjectVelocity;

    public bool isReleased = false;
    [SerializeField] float BreakagePoint; [Space]

    [SerializeField] List<GameObject> ObjectPieces;

    [Header("Piece Properties")]
    [SerializeField] [Range(0.1f,25f)] float Mass;
    [SerializeField] [Range(0f,8f)] float LinearDampening;
    [SerializeField] [Range(0f,8f)] float AngularDampening;

    void Awake()
    {
        MainRB = GetComponent<Rigidbody>();

        for (int i = 0; i < transform.childCount; i++)
        {
            ObjectPieces.Add(transform.GetChild(i).gameObject);
        }

        Debug.Log($"Pieces found in {this}: {ObjectPieces.Count}");
    }

    void Update()
    {
        if (MainRB != null)
        {
            ObjectVelocity = MainRB.linearVelocity.Abs();
            MainRB.useGravity = isReleased;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ObjectPieces[0].GetComponent<Rigidbody>() != null) return;

        Debug.Log($"{this} collided with {collision} (Velocity: {ObjectVelocity})");

        if (ObjectVelocity == Vector3.zero) return;
        if (ObjectVelocity.x <= BreakagePoint && ObjectVelocity.y <= BreakagePoint && ObjectVelocity.z <= BreakagePoint) return;

        foreach (GameObject piece in ObjectPieces)
        {
            piece.GetComponent<Collider>().enabled = true;

            Rigidbody rb = piece.AddComponent<Rigidbody>();
            rb.mass = Mass;
            rb.linearDamping = LinearDampening;
            rb.angularDamping = AngularDampening;
        }

        Destroy(GetComponent<Collider>());
        Destroy(GetComponent<Rigidbody>());
    }
}
