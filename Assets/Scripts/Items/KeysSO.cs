using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class KeysSO : ScriptableObject
{
    [Header("Basic Info")]
    public string keyName;
    public Sprite icon;

    [TextArea]
    public string description;
}
