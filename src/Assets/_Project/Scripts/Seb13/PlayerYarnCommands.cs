using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerYarnCommands : MonoBehaviour
{
    DialogueRunner dialogueRunner;

    void Awake()
    {
        dialogueRunner = FindObjectOfType<DialogueRunner>();

        // Create a new command called 'camera_look', which looks at a target.
        dialogueRunner.AddCommandHandler(
            "camera_look",     // the name of the command
            CameraLookAtTarget // the method to run
        );
    }

    /// <summary>
    /// The method that gets called when '<<camera_look>>' is run.
    /// </summary>
    /// <param name="parameters"></param>
    void CameraLookAtTarget(string[] parameters)
    {
        // Take the first parameter, and use it to find the object
        string targetName = parameters[0];
        GameObject target = GameObject.Find(targetName);

        // Log an error if we can't find it
        if (target == null)
        {
            Debug.LogError($"Cannot make camera look at {targetName}:" +
                "cannot find target");
            return;
        }

        // Make the main camera look at this target
        Camera.main.transform.LookAt(target.transform);
    }
}
