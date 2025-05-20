using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gane/Item", fileName = "NewItem")]
public class ItemSo : ScriptableObject
{
    [Header("Score value")]
    public int point = 10;
}