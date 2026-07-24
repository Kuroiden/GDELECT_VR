using System.Collections.Generic;
using Unity.XR.CoreUtils;
using Unity.XR.Management;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class ObjectShatter : MonoBehaviour
{
    Rigidbody MainRB;
    Vector3 ObjectVelocity;
    public float CurrBreakagePoint = 0f;

    public bool isReleased = false;
    [Tooltip("Weight or density of the whole object")]
    [SerializeField] [Range(0.1f,25f)] float Mass;
    [Tooltip("Ceiling of how much force (velocity) the whole object can take before breaking")]
    [SerializeField] [Range(0.1f, 2.5f)] float BreakagePoint; [Space]
    [Tooltip("How durable the whole object is (0.1 = Breaks on contact; 10 = Durable)")]
    [SerializeField] [Range(0.1f, 10f)] float Durability; [Space]

    [SerializeField] List<GameObject> ObjectPieces;

    [Header("Piece Properties")]
    [Tooltip("Degree of dampening every piece to linear motion (0 = No dampening; 50 max)")]
    [SerializeField] [Range(0f,50f)] float LinearDampening;
    [Tooltip("Degree of dampening every piece to rotation (0 = Free rotation; 50 max)")]
    [SerializeField] [Range(0f,50f)] float AngularDampening;

    void Awake()
    {
        CurrBreakagePoint = BreakagePoint;
        MainRB = GetComponent<Rigidbody>();

        // Automatically adjusts rigidbody parameters
        MainRB.mass = Mass;
        MainRB.interpolation = RigidbodyInterpolation.Interpolate;
        MainRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        if (ObjectPieces.Count == 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                ObjectPieces.Add(transform.GetChild(i).gameObject);
            }
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

        Vector3 CollisionImpulse = collision.impulse;

        GameObject ColliderObj = collision.collider.gameObject;
        ContactPoint CollisionContact = collision.contacts[0];

        List<float> points = new List<float>();
        Dictionary<float, GameObject> pieces = new Dictionary<float, GameObject>();

        foreach (GameObject piece in ObjectPieces)
        {
            float currPoint = Vector3.Distance(CollisionContact.point, piece.transform.position);

            if (!points.Contains(currPoint)) points.Add(currPoint);
            if (!pieces.ContainsKey(currPoint)) pieces.Add(currPoint, piece);
        }

        float NearestPoint = Mathf.Min(points.ToArray());

        GameObject PointOfCollision = pieces[NearestPoint];

        #region Debug
        if (ObjectVelocity == Vector3.zero)
            Debug.Log($"<color=#ffff00><color=#00ffff>{collision.collider.name}</color> collided with <color=#00ff88>{this.gameObject.name}</color> (Velocity: {CollisionImpulse})</color>");
        else
            Debug.Log($"<color=#ff8800><color=#00ff88>{this.gameObject.name}</color> collided with <color=#00ffff>{collision.collider.name}</color> (Velocity: {CollisionImpulse})</color>");
        
        if (CollisionContact.point != null) Debug.Log($"Contact point detected. Contact found at <color=#00ff88>{PointOfCollision.name}</color>");
        #endregion

        // Check if breakble is dropped high or thrown hard enough
        if (CollisionImpulse.x <= CurrBreakagePoint && CollisionImpulse.y <= CurrBreakagePoint && CollisionImpulse.z <= CurrBreakagePoint) LowerBreakagePoint(CollisionImpulse);
        else BreakObject(PointOfCollision, ColliderObj.GetComponent<Rigidbody>(), CollisionImpulse);
    }

    // Lower object breakage point with every collision
    void LowerBreakagePoint(Vector3 Velocity)
    {
        float forceApplied = 0f;

        if (Velocity != Vector3.zero) forceApplied = Mathf.Max(Velocity.x, Velocity.y, Velocity.z);

        CurrBreakagePoint -= (forceApplied / Durability);
        if (CurrBreakagePoint < 0f) CurrBreakagePoint = 0f;
    }

    // Break apart breakable
    void BreakObject(GameObject PointOfCollision, Rigidbody Collider, Vector3 CollisionImpulse)
    {
        float forceApplied = 0f;

        if (CollisionImpulse != Vector3.zero) forceApplied = Mathf.Max(CollisionImpulse.x, CollisionImpulse.y, CollisionImpulse.z);

        foreach (GameObject piece in ObjectPieces)
        {
            Rigidbody rb = piece.AddComponent<Rigidbody>();

            rb.mass = Mass / ObjectPieces.Count; // Divides overall mass with the total number of pieces
            rb.linearDamping = LinearDampening;
            rb.angularDamping = AngularDampening;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            piece.AddComponent<XRGrabInteractable>();
            piece.AddComponent<XRGeneralGrabTransformer>();
        }

        foreach (GameObject piece in ObjectPieces)
        {
            Rigidbody rb = piece.GetComponent<Rigidbody>();
            rb.AddExplosionForce(forceApplied / Durability, PointOfCollision.transform.position, 1f);
        }


        Debug.Log($"<color=#00ff00><color=#00ff88>{this.gameObject.name}</color> has broken.</color>");

        // Ensures that the object behaves as if it is truly broken
        Destroy(GetComponent<XRGrabInteractable>());
        Destroy(GetComponent<XRGeneralGrabTransformer>());
        Destroy(GetComponent<Collider>());
        Destroy(GetComponent<Rigidbody>());
    }
}
