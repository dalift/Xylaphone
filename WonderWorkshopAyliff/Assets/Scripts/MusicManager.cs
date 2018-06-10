using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine;

[Serializable]
public class NoteData{
    [SerializeField]
    public int noteValue;
    [SerializeField]
    public float timeValue;//we need to round this to the nearest .125
    [SerializeField]
    public GameObject uiReference;//
}

[Serializable]
public class TimeLineData
{
    [SerializeField]
    public float timeValue;
    [SerializeField]
    public Transform label;
}

public class MusicManager : MonoBehaviour {

    public List<NoteData> currentNoteData = new List<NoteData>();
    List<AudioSource> audioSources = new List<AudioSource>();
    [Header("Variables")]
    public bool echo = false;
    public bool reverse = false;
    public bool playing = false;
    public bool recording = false;

    [Header("Managers")]
    public MappingManager noteMapping;

    [Header("Scene References")]
    //objects that define the note matrix
    public Transform xMinObject;
    public Transform xMaxObject;
    public Transform yMinObject;
    public Transform yMaxObject;
    Rect noteMatrix;
    public Transform noteHolder;//holds all the generated notes
    public Transform soundHolder;//holds all the played audio files
    public Transform playhead;
    
    public List<TimeLineData> timeLabels = new List<TimeLineData>();//so we can set the time increments correctly

    [Header("Prefab References")]
    public GameObject notePrefab;
    public GameObject soundPrefab;
    public MappingManager mappingPrefab;
    public InputManager inputManager;

    [Header("Playing variables")]
    public float currentTime = 0f;
    float lastTime = 0f;
    public int quatization = 0;
    // Use this for initialization
    void Start()
    {
        //So it is easier to access the bounds of thenote matrix
        noteMatrix = new Rect(xMinObject.position.x, yMinObject.position.y, xMaxObject.position.x- xMinObject.position.x, yMaxObject.position.y- yMinObject.position.y);

        //make sure all the time labels are correctly spaced
        for (int i = 0;i < timeLabels.Count; i++)
        {
            timeLabels[i].label.position = new Vector3(GetXPositionGivenTime(timeLabels[i].timeValue), timeLabels[i].label.position.y, timeLabels[i].label.position.z);
        }

        //place the playhead at zero
        playhead.position = new Vector3(noteMatrix.xMin, playhead.position.y, playhead.position.z);
        
    }

    /// <summary>
    /// Corilates space and time, given that the time is between 0 and 5 and the space is between xMin and xMax
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    float GetXPositionGivenTime(float time)
    {
        float xValue = Mathf.Lerp(noteMatrix.xMin, noteMatrix.xMax, time/5f);
        return xValue;
    }

    /// <summary>
    /// Corilates space and note values, given that note values are between 0 and 7 and the space is between yMin and yMax
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    float GetYPositionGivenValue(int value)
    {
        float yValue = Mathf.Lerp(noteMatrix.yMin, noteMatrix.yMax, value / 7f);
        return yValue;
    }

    //add a note to the note data at the current time. If not recording, start at zero.
    public void AddNoteData(int noteValue)
    {
        //Starts recording on a new slate
        if (!recording)
        {
            if (playing)
            {
                playing = false;
                inputManager.ChangePlayStopDisplay();
            }
            //Clearing the note data might remove player data that they want to keep, but could cause issues with large data sets
            ClearNoteData();
            currentTime = reverse ? 5f : 0f;
            recording = true;
        }


        NoteData newData = new NoteData();
        newData.noteValue = noteValue;
        //add the correct time that this note was added rounded to the nearest quatinized value
        float roundedTime = RoundTime(currentTime);
        //make sure we don't already have this note in the data:
        if (currentNoteData.Exists(x => x.noteValue == noteValue && x.timeValue == roundedTime))
            return;

        newData.timeValue = roundedTime;
        newData.uiReference = AddNewNoteUI(noteValue, roundedTime);
        currentNoteData.Add(newData);
        //play the note
        PlayNote(noteValue);
    }

    /// <summary>
    /// Rounds a time to the correct quartized value
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    float RoundTime(float input)
    {
        if (quatization == 0)
            return input;
        float rounded = Mathf.Round(input * quatization) / (float)quatization;
       
        return rounded;
    }
    /// <summary>
    /// Spawns the note prefab, sets its parent, names it, gives it the right icon, and positions it correctly
    /// </summary>
    /// <param name="noteValue"></param>
    /// <param name="noteTime"></param>
    /// <returns></returns>
    GameObject AddNewNoteUI(int noteValue, float noteTime)
    {
        GameObject noteUI = Instantiate
            (notePrefab, new Vector3(GetXPositionGivenTime(noteTime), GetYPositionGivenValue(noteValue), 0f), Quaternion.identity) as GameObject;
        noteUI.transform.parent = noteHolder;
        noteUI.name = noteMapping.GetNoteString(noteValue)+"_"+noteTime;
        noteUI.GetComponent<Image>().sprite = noteMapping.GetNoteTexture(noteValue);
        return noteUI;
    }

    /// <summary>
    /// Triggers the coroutine that manages the audio source and determines if there is echo
    /// </summary>
    /// <param name="noteValue"></param>
    void PlayNote(int noteValue)
    {
        StartCoroutine(AudioSourcePlayer(noteValue, 0));
        if (echo)
        {
            StartCoroutine(AudioSourcePlayer(noteValue, 1));
            StartCoroutine(AudioSourcePlayer(noteValue, 2));
            //StartCoroutine(AudioSourcePlayer(noteValue, 3));
        }        
    }

    /// <summary>
    /// Spawns an audio source, sets it to the right note, checks to see if the audio source is playing, and deletes it when finished
    /// </summary>
    /// <param name="thisAudio"></param>
    /// <returns></returns>
    IEnumerator AudioSourcePlayer(int noteValue, int echoValue)//echoValue - 0-3
    {
        
        //spawn the audio source prefab, and set the clip
        GameObject source = Instantiate(soundPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        source.name = noteMapping.GetNoteString(noteValue) + "_" + echoValue;
        source.transform.parent = soundHolder;

        AudioSource thisAudio = source.GetComponent<AudioSource>();
        //use the note mapper to get the correct clip
        thisAudio.clip = noteMapping.GetNoteSound(noteValue);
        //add the audio source to the audioSources list
        audioSources.Add(thisAudio);

        //make adjustments based on the echoe value:
        for (int i = echoValue; i > 0; i--)
            thisAudio.volume = thisAudio.volume * .75f;//attenuated by 75%

        yield return new WaitForSeconds(.25f * echoValue);//250ms of delay

        //in case we stop playing and thisAudio is destroyed
        if (thisAudio == null)
            yield break;
        thisAudio.Play();
        //Wait for the audio source to stop playing
        while (thisAudio.isPlaying)
        {
            yield return new WaitForEndOfFrame();
            if (thisAudio == null)
                yield break;
        }
        audioSources.Remove(thisAudio);
        Destroy(thisAudio.gameObject);
    }

    /// <summary>
    /// Removes all audio sources from the scene, clears the audio source data, and pauses the header
    /// Called internaly
    /// </summary>
    void StopPlaying()
    {
        playing = false;
        StopAllCoroutines();//stops all the echo stacked audio files from playing
        //stop all audio from playing
        for (int i = 0; i < audioSources.Count; i++)
        {
            Destroy(audioSources[i].gameObject);
        }
        audioSources.Clear();
    }

    /// <summary>
    /// Removes all the note data, clears the UI, stops playing audio files, and resets the header
    /// Called from the clear button
    /// </summary>
    public void ClearNoteData()
    {
        StopPlaying();

        //remove all the note UI, and clear the data
        for(int i = 0; i < currentNoteData.Count; i++)
        {
            Destroy(currentNoteData[i].uiReference);
        }
        currentNoteData.Clear();

        //place the playhead at zero
        playhead.position = new Vector3(noteMatrix.xMin, playhead.position.y, playhead.position.z);
    }

    /// <summary>
    /// Starts playing the music. Called internally
    /// </summary>
    void StartPlaying()
    {
        if (recording)
            recording = false;
        currentTime = reverse ? 5.01f : 0f;
        playing = true;
    }

    /// <summary>
    /// Called when the Play/Stop button is pressed. Triggered from Input Manager
    /// </summary>
    public void PlayStopButton()
    {
        if(playing || recording)
        {
            StopPlaying();
            //update the icon
        }
        else{
            StartPlaying();
            //update the icon
        }
    }

	// Update is called once per frame
	void Update () {
        //update the current time
        if(playing || recording)
        {
            lastTime = currentTime;
            if (reverse)//decrease the current play time
            {
                currentTime -= Time.deltaTime;
                PlayNotesInRange();
                if (currentTime < 0)
                {
                    currentTime = 0;
                    playing = false;
                    recording = false;
                }
            }
            else//increase the current play time
            {
                currentTime += Time.deltaTime;
                PlayNotesInRange();
                if (currentTime > 5f)
                {
                    currentTime = 5f;
                    playing = false;//let the audio files finish playing
                    recording = false;
                }
            }
            //Position the playhead
            playhead.position = new Vector3(GetXPositionGivenTime(currentTime), playhead.position.y, playhead.position.z);
        }
	}

    public void Test01()
    {
        Debug.Log("Updated the Script");
    }

    /// <summary>
    /// Plays all notes that are between the last time and the current time regaurdless of which is higher
    /// </summary>
    void PlayNotesInRange()
    {
        //get all notes between the current time, and the next time step, and play them
        for (int i = 0; i < currentNoteData.Count; i++)
        {
            if ((currentNoteData[i].timeValue >= lastTime && currentNoteData[i].timeValue < currentTime)
                || (currentNoteData[i].timeValue < lastTime && currentNoteData[i].timeValue >= currentTime))
            {
                PlayNote(currentNoteData[i].noteValue);
            }
        }
    }
}
