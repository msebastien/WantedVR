using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// When shooting, rotate revolver's cylinder
public class RotateCylinder : MonoBehaviour
{
    private BNG.Grabbable weaponCylinder;

    public void RotateRevolverCylinder(float angle)
    {
        weaponCylinder = GetComponentInChildren<RevolverClipSnap>(false).HeldMagazine;
            
        if (weaponCylinder != null)
        {
            weaponCylinder.transform.Rotate(new Vector3(0.0f, 0.0f, 1.0f), angle, Space.Self);
        }    
    }
}
