using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class NotesSO : ScriptableObject
{
    [Header("Basic Info")]
    public string notesName;
    public Sprite icon;

    [TextArea]
    public string description;
}
