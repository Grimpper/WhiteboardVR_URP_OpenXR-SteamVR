using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserTracker : MonoBehaviour
{
    public Transform user;

    private void LateUpdate()
    {
        transform.position = user.position - user.localPosition.y * Vector3.up;
    }
}
