using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class PanicMode : MonoBehaviour
{
    public Light2D[] globalLights;
    public Color panicColor = Color.red;

    BoundaryManager boundaryManager;
    MusicManager musicManager;

    [Header("Debug")]
    public bool forcePanic = false;
    bool panicking;
    public bool Panicking
    {
        get { return panicking; }
        set
        {
            if (!panicking) {

                StartCoroutine(musicManager.PlayFastDrumsEnum());
                foreach (var item in globalLights)
                {
                    item.color = panicColor;
                }

                foreach (var item in boundaryManager.Boundaries)
                {
                    item.CheckPanicEnemySpawns();
                }
            }
            panicking = value;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        boundaryManager = FindObjectOfType<BoundaryManager>();
        musicManager = FindObjectOfType<MusicManager>();
        Debug.Assert(boundaryManager);
        Debug.Assert(musicManager);
    }

    // Update is called once per frame
    void Update()
    {
        if (forcePanic)
        {
            Debug.Log("Forcing panic");
            forcePanic = false;
            Panicking = true;
        }
    }
}
