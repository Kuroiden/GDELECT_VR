using System.Collections.Generic;
using System.Net;
using Unity.XR.CoreUtils;
using UnityEngine;

public class ObjectShatter : MonoBehaviour
{
    Rigidbody MainRB;
    Vector3 ObjectVelocity;

    public bool isReleased = false;
    [SerializeField][Range(0f, 3f)] float BreakagePoint; [Space]

    [SerializeField] List<GameObject> ObjectPieces;

    [Header("Piece Properties")]
    [SerializeField] [Range(0.1f,25f)] float Mass;
    [SerializeField] [Range(0f,8f)] float LinearDampening;
    [SerializeField] [Range(0f,8f)] float AngularDampening;

    void Awake()
    {
        MainRB = GetComponent<Rigidbody>();
        MainRB.mass = Mass;

        for (int i = 0; i < transform.childCount; i++)
        {
            ObjectPieces.Add(transform.GetChild(i).gameObject);
        }

        Debug.Log($"Pieces found in {this.gameObject.name}: {ObjectPieces.Count}");
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

        Vector3 CollisionVelocity = ObjectVelocity;

        if (ObjectVelocity == Vector3.zero)
        {
            GameObject ColliderObj = collision.collider.gameObject;

            if (ColliderObj.GetComponent<Rigidbody>() != null)
            {
                Rigidbody ColliderRB = ColliderObj.GetComponent<Rigidbody>();
                float ColliderMass = ColliderRB.mass * 100f;

                // Checks for any collider that hits the breakble directly while at rest
                bool ColliderMovement = true;

                // Collider mass is also considered
                Vector3 ColliderLinearVelocity = ColliderRB.linearVelocity * ColliderMass;
                Vector3 ColliderAngularVelocity = ColliderRB.angularVelocity * ColliderMass;

                CollisionVelocity = ColliderLinearVelocity != Vector3.zero ? ColliderLinearVelocity : ColliderAngularVelocity != Vector3.zero ? ColliderAngularVelocity : Vector3.zero;

                if (ColliderLinearVelocity.x <= BreakagePoint && ColliderLinearVelocity.y <= BreakagePoint && ColliderLinearVelocity.z <= BreakagePoint)
                    ColliderMovement = false;
                if (ColliderAngularVelocity.x <= BreakagePoint && ColliderAngularVelocity.y <= BreakagePoint && ColliderAngularVelocity.z <= BreakagePoint)
                    ColliderMovement = false;

                if (ColliderMovement) goto BreakObject;
                else return; // Returns if the breakable is not hit hard enough
            }
            else return; // Returns if the breakable is at rest and nothing hits it
        }

        Debug.Log($"{this.gameObject.name} collided with {collision.collider.name} (Velocity: {ObjectVelocity})");

        // Check if breakble is dropped high or thrown hard enough
        if (ObjectVelocity.x <= BreakagePoint && ObjectVelocity.y <= BreakagePoint && ObjectVelocity.z <= BreakagePoint) return;

        // Break apart breakbable
        BreakObject:
        foreach (GameObject piece in ObjectPieces)
        {
            piece.GetComponent<Collider>().enabled = true;

            Rigidbody rb = piece.AddComponent<Rigidbody>();
            rb.mass = Mass;
            rb.linearDamping = LinearDampening;
            rb.angularDamping = AngularDampening;
        }

        Debug.Log($"<color=#00ff00>{this.gameObject.name} has broken.");

        // Ensures that the object behaves as if it is truly broken
        Destroy(GetComponent<Collider>());
        Destroy(GetComponent<Rigidbody>());
    }
}
