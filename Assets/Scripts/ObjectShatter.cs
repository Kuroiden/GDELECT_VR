using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class ObjectShatter : MonoBehaviour
{
    Rigidbody MainRB;
    Vector3 ObjectVelocity;

    public bool isReleased = false;
    [Tooltip("Weight or density of the whole object")]
    [SerializeField] [Range(0.1f,25f)] float Mass;
    [Tooltip("Ceiling of how much force (velocity) the whole object can take before breaking")]
    [SerializeField][Range(0.1f, 25f)] float BreakagePoint; [Space]
    [Tooltip("How fragile the whole object is (0.1 = Breaks on contact; 10 = Durable)")]
    [SerializeField][Range(0.1f, 10f)] float Fragility; [Space]

    [SerializeField] List<GameObject> ObjectPieces;

    [Header("Piece Properties")]
    [Tooltip("Degree of dampening every piece to linear motion (0 = No dampening; 50 max)")]
    [SerializeField] [Range(0f,50f)] float LinearDampening;
    [Tooltip("Degree of dampening every piece to rotation (0 = Free rotation; 50 max)")]
    [SerializeField] [Range(0f,50f)] float AngularDampening;

    void Awake()
    {
        MainRB = GetComponent<Rigidbody>();

        // Automatically adjusts rigidbody parameters
        MainRB.mass = Mass;
        MainRB.interpolation = RigidbodyInterpolation.Interpolate;
        MainRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        for (int i = 0; i < transform.childCount; i++)
        {
            ObjectPieces.Add(transform.GetChild(i).gameObject);
        }

        Debug.Log($"Pieces found in <color=#00ff88>{this.gameObject.name}</color>: {ObjectPieces.Count}");
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

                Debug.Log($"<color=#ffff00><color=#00ffff>{collision.collider.name}</color> collided with <color=#00ff88>{this.gameObject.name}</color> (Velocity: {CollisionVelocity})</color>");

                if (ColliderLinearVelocity.x <= BreakagePoint && ColliderLinearVelocity.y <= BreakagePoint && ColliderLinearVelocity.z <= BreakagePoint)
                    ColliderMovement = false;
                if (ColliderAngularVelocity.x <= BreakagePoint && ColliderAngularVelocity.y <= BreakagePoint && ColliderAngularVelocity.z <= BreakagePoint)
                    ColliderMovement = false;

                LowerBreakagePoint(CollisionVelocity);

                if (ColliderMovement) BreakObject();
                else return; // Returns if the breakable is not hit hard enough
            }
            else return; // Returns if the breakable is at rest and nothing hits it
        }

        Debug.Log($"<color=#ff8800><color=#00ff88>{this.gameObject.name}</color> collided with <color=#00ffff>{collision.collider.name}</color> (Velocity: {ObjectVelocity})</color>");

        // Check if breakble is dropped high or thrown hard enough
        if (ObjectVelocity.x <= BreakagePoint && ObjectVelocity.y <= BreakagePoint && ObjectVelocity.z <= BreakagePoint) LowerBreakagePoint(ObjectVelocity);
        else BreakObject();
    }

    // Lower object breakage point with every collision
    void LowerBreakagePoint(Vector3 Velocity)
    {
        float forceApplied = 0f;

        if (Velocity != Vector3.zero) forceApplied = Mathf.Max(Velocity.x, Velocity.y, Velocity.z);

        BreakagePoint -= (forceApplied / Fragility);
        if (BreakagePoint < 0f) BreakagePoint = 0f;
    }

    // Break apart breakable
    void BreakObject()
    {
        foreach (GameObject piece in ObjectPieces)
        {
            piece.GetComponent<Collider>().enabled = true;

            Rigidbody rb = piece.AddComponent<Rigidbody>();

            rb.mass = Mass / ObjectPieces.Count; // Divides overall mass with the total number of pieces
            rb.linearDamping = LinearDampening;
            rb.angularDamping = AngularDampening;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        Debug.Log($"<color=#00ff00><color=#00ff88>{this.gameObject.name}</color> has broken.</color>");

        // Ensures that the object behaves as if it is truly broken
        Destroy(GetComponent<Collider>());
        Destroy(GetComponent<Rigidbody>());
    }
}
