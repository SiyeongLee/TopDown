using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class itemObject : MonoBehaviour
{
    [SerializeField] ItemSo data;

    public int GetPoint()
    {
        return data.point;
    }
}

