using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTracker : MonoBehaviour
{
    public Transform room;

    private void LateUpdate()
    {
        transform.position = room.position;
    }
}
