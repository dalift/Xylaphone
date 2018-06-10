using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class InputManager : MonoBehaviour {

    public MusicManager musicManager;

    [Header("Play Pause Button")]
    public GameObject ui_playButton;
    public GameObject ui_stopButton;

    /// <summary>
    /// Called when the player presses the play/stop button
    /// </summary>
    public void PlayStopButton()
    {
        ChangePlayStopDisplay();
        musicManager.PlayStopButton();
    }

    //Called from the Music Manager, when the player stops the player while playing using the keyboard
    public void ChangePlayStopDisplay()
    {
        ui_stopButton.SetActive(!ui_stopButton.activeSelf);
        ui_playButton.SetActive(!ui_playButton.activeSelf);
    }

    //Set through the radial buttons. Appropriate values are: 0, 1, 4, 8
    public void SetQuatization(int value)
    {
        musicManager.quatization = value;
        
    }

    /// <summary>
    /// Toggled from the Echo option.
    /// </summary>
    /// <param name="input"></param>
    public void ToggleEcho(bool input)
    {
        musicManager.echo = input;
    }

    /// <summary>
    /// Toggled from the Reverse option.
    /// </summary>
    /// <param name="input"></param>
    public void ToggleReverse(bool input)
    {
        musicManager.reverse = input;
    }

}
