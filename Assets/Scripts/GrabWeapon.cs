using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
public class GrabWeapon : XRGrabInteractable
{   
    void Start()
    {
        if (!attachTransform)
        {
            GameObject attachPoint = new GameObject("GrabOffset");
            attachPoint.transform.SetParent(transform, false);
            attachTransform = attachPoint.transform;
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        attachTransform.position = args.interactorObject.transform.position;
        attachTransform.rotation = args.interactorObject.transform.rotation;
        base.OnSelectEntered(args);
    }

}
