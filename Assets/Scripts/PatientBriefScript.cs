using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class PatientBriefScript : MonoBehaviour
{
    

    public void LoadScene()
    {
        // Load the Patient Brief scene
        SceneManager.LoadScene("TherapistHomePage");
    }
}
