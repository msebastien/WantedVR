using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Constrain a clip when it enters this area. Attaches the clip in place if close enough.
/// </summary>
public class RevolverClipSnap : MonoBehaviour
{
    /// <summary>
    /// Clip transform name must contain this to be considered valid
    /// </summary>
    public string AcceptableMagazineName = "RevolverClip";

    /// <summary>
    /// The weapon this magazine is attached to (optional)
    /// </summary>RaycastWeapon
    public BNG.Grabbable AttachedWeapon;

    public float ClipSnapDistance = 0.075f;
    public float ClipUnsnapDistance = 0.15f;

    /// <summary>
    ///  How much force to apply to the inserted magazine if it is forcefully ejected
    /// </summary>
    public float EjectForce = 1f;

    public BNG.Grabbable HeldMagazine = null;
    Collider HeldCollider = null;

    public float MagazineDistance = 0f;

    bool magazineInPlace = false;

    // Lock in place for physics
    bool lockedInPlace = false;

    public AudioClip ClipAttachSound;
    public AudioClip ClipDetachSound;

    RaycastRevolver parentWeapon;
    BNG.GrabberArea grabClipArea;

    float lastEjectTime;

    void Start()
    {
        grabClipArea = GetComponentInChildren<BNG.GrabberArea>();

        if (transform.parent != null)
        {
            parentWeapon = transform.parent.GetComponent<RaycastRevolver>();
        }
    }

    void LateUpdate()
    {
        // Are we trying to grab the clip from the weapon
        checkGrabClipInput();

        // There is a magazine inside the slide. Position it properly
        if (HeldMagazine != null)
        {

            HeldMagazine.transform.parent = transform;
            Vector3 localRot = HeldMagazine.transform.localEulerAngles;

            //Debug.Log("RevolverClip Local Rot Z: " + localRot.z);

            // Lock in place immediately
            if (lockedInPlace)
            {
                HeldMagazine.transform.localPosition = Vector3.zero;
                HeldMagazine.transform.localEulerAngles = new Vector3(0.0f, 0.0f, localRot.z);
                return;
            }

            Vector3 localPos = HeldMagazine.transform.localPosition;

            // Make sure magazine is aligned with MagazineSlide
            HeldMagazine.transform.localEulerAngles = new Vector3(0.0f, 0.0f, localRot.z);

            // Only allow X translation. Don't allow to go up and through clip area
            float localX = localPos.x;

            moveMagazine(new Vector3(localX, 0, 0));

            MagazineDistance = Vector3.Distance(transform.position, HeldMagazine.transform.position);

            bool clipRecentlyGrabbed = Time.time - HeldMagazine.LastGrabTime < 1f;

            // Snap Magazine In Place
            if (MagazineDistance < ClipSnapDistance)
            {

                // Snap in place
                if (!magazineInPlace && !recentlyEjected() && !clipRecentlyGrabbed)
                {
                    attachMagazine();
                }

                // Make sure magazine stays in place if not being grabbed
                if (!HeldMagazine.BeingHeld)
                {
                    moveMagazine(Vector3.zero);
                }
            }
            // Stop aligning clip with slide if we exceed this distance
            else if (MagazineDistance >= ClipUnsnapDistance && !recentlyEjected())
            {
                detachMagazine();
            }
        }
    }

    bool recentlyEjected()
    {
        return Time.time - lastEjectTime < 0.1f;
    }

    void moveMagazine(Vector3 localPosition)
    {
        HeldMagazine.transform.localPosition = localPosition;
    }

    void checkGrabClipInput()
    {
        // No need to check for grabbing a clip out if none exists
        if (HeldMagazine == null || grabClipArea == null)
        {
            return;
        }

        // Don't grab clip if the weapon isn't being held
        if (AttachedWeapon != null && !AttachedWeapon.BeingHeld)
        {
            return;
        }

        BNG.Grabber nearestGrabber = grabClipArea.GetOpenGrabber();
        if (grabClipArea != null && nearestGrabber != null)
        {
            if (nearestGrabber.HandSide == BNG.ControllerHand.Left && BNG.InputBridge.Instance.LeftGripDown)
            {
                // grab clip
                OnGrabClipArea(nearestGrabber);
            }
            else if (nearestGrabber.HandSide == BNG.ControllerHand.Right && BNG.InputBridge.Instance.RightGripDown)
            {
                OnGrabClipArea(nearestGrabber);
            }
        }
    }

    void attachMagazine()
    {
        // Drop Item
        var grabber = HeldMagazine.GetPrimaryGrabber();
        HeldMagazine.DropItem(grabber, false, false);

        // Play Sound
        BNG.VRUtils.Instance.PlaySpatialClipAt(ClipAttachSound, transform.position, 1f);

        // Move to desired location before locking in place
        moveMagazine(Vector3.zero);

        // Add fixed joint to make sure physics work properly
        if (transform.parent != null)
        {
            Rigidbody parentRB = transform.parent.GetComponent<Rigidbody>();
            if (parentRB)
            {
                FixedJoint fj = HeldMagazine.gameObject.AddComponent<FixedJoint>();
                fj.autoConfigureConnectedAnchor = true;
                fj.axis = new Vector3(0, 1, 0);
                fj.connectedBody = parentRB;
            }

            // If attached to a Raycast weapon, let it know we attached something
            if (parentWeapon)
            {
                parentWeapon.OnAttachedAmmo();
            }
        }

        // Don't let anything try to grab the magazine while it's within the weapon
        // We will use a grabbable proxy to grab the clip back out instead
        HeldMagazine.enabled = false;

        lockedInPlace = true;
        magazineInPlace = true;
    }

    /// <summary>
    /// Detach Magazine from it's parent. Removes joint, re-enables collider, and calls events
    /// </summary>
    /// <returns>Returns the magazine that was ejected or null if no magazine was attached</returns>
    BNG.Grabbable detachMagazine()
    {

        if (HeldMagazine == null)
        {
            return null;
        }

        BNG.VRUtils.Instance.PlaySpatialClipAt(ClipDetachSound, transform.position, 1f, 0.9f);

        HeldMagazine.transform.parent = null;

        // Remove fixed joint
        if (transform.parent != null)
        {
            Rigidbody parentRB = transform.parent.GetComponent<Rigidbody>();
            if (parentRB)
            {
                FixedJoint fj = HeldMagazine.gameObject.GetComponent<FixedJoint>();
                if (fj)
                {
                    fj.connectedBody = null;
                    Destroy(fj);
                }
            }
        }

        // Reset Collider
        if (HeldCollider != null)
        {
            HeldCollider.enabled = true;
            HeldCollider = null;
        }

        // Let wep know we detached something
        if (parentWeapon)
        {
            parentWeapon.OnDetachedAmmo();
        }

        // Can be grabbed again
        HeldMagazine.enabled = true;
        magazineInPlace = false;
        lockedInPlace = false;
        lastEjectTime = Time.time;

        var returnGrab = HeldMagazine;
        HeldMagazine = null;

        return returnGrab;
    }

    public void EjectMagazine()
    {
        BNG.Grabbable ejectedMag = detachMagazine();
        lastEjectTime = Time.time;

        StartCoroutine(EjectMagRoutine(ejectedMag));
    }

    IEnumerator EjectMagRoutine(BNG.Grabbable ejectedMag)
    {

        if (ejectedMag != null && ejectedMag.GetComponent<Rigidbody>() != null)
        {

            Rigidbody ejectRigid = ejectedMag.GetComponent<Rigidbody>();

            // Wait before ejecting

            // Move clip right before we eject it
            ejectedMag.transform.parent = transform;

            bool IsEjectRight = false;
            if (ejectedMag.transform.localPosition.x > ClipSnapDistance)
            {
                ejectedMag.transform.localPosition = new Vector3(0.1f, 0, 0);
                IsEjectRight = true;
            } 
            else if(ejectedMag.transform.localPosition.x < -ClipSnapDistance)
            {
                ejectedMag.transform.localPosition = new Vector3(-0.1f, 0, 0);
            }

            
            ejectedMag.transform.parent = null;

            // Eject with physics force to the right or to the left
            if (IsEjectRight)
                ejectRigid.AddForce(ejectedMag.transform.right * EjectForce, ForceMode.VelocityChange);
            else
                ejectRigid.AddForce(-ejectedMag.transform.right * EjectForce, ForceMode.VelocityChange);

            yield return new WaitForFixedUpdate();
            ejectedMag.transform.parent = null;

        }

        yield return null;
    }

    // Pull out magazine from clip area
    public void OnGrabClipArea(BNG.Grabber grabbedBy)
    {
        if (HeldMagazine != null)
        {
            // Store reference so we can eject the clip first
            BNG.Grabbable temp = HeldMagazine;

            // Make sure the magazine can be gripped
            HeldMagazine.enabled = true;

            // Eject clip into hand
            detachMagazine();

            // Now transfer grab to the grabber
            temp.enabled = true;

            grabbedBy.GrabGrabbable(temp);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        BNG.Grabbable grab = other.GetComponent<BNG.Grabbable>();
        if (HeldMagazine == null && grab != null && grab.transform.name.Contains(AcceptableMagazineName))
        {
            HeldMagazine = grab;
            HeldMagazine.transform.parent = transform;

            HeldCollider = other;

            // Disable the collider while we're sliding it in to the weapon
            if (HeldCollider != null)
            {
                HeldCollider.enabled = false;
            }
        }
    }
}
