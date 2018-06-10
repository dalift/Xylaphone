using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class NoteMapping
{
    [SerializeField]
    public int noteValue;
    [SerializeField]
    public Sprite noteDisplay;
    [SerializeField]
    public AudioClip noteSound;
    [SerializeField]
    public string displayString;
}

public class MappingManager : MonoBehaviour {

    //holds data that we use in other scripts, for easy reference on future projects
    public List<NoteMapping> noteMapping = new List<NoteMapping>();

    public String GetNoteString(int noteValue)
    {
        String correctString = noteMapping.FirstOrDefault(x => x.noteValue == noteValue).displayString;
        return correctString;
    }

    public Sprite GetNoteTexture(int noteValue)
    {
        Sprite correctTexture = noteMapping.FirstOrDefault(x => x.noteValue == noteValue).noteDisplay;
        return correctTexture;
    }

    public AudioClip GetNoteSound(int noteValue)
    {
        AudioClip correctClip = noteMapping.FirstOrDefault(x => x.noteValue == noteValue).noteSound;
        return correctClip;
    }
}
