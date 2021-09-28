using Hellmade.Sound;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipNPC : MonoBehaviour
{
    PanicMode panicMode;
    public GameObject npc;

    // Start is called before the first frame update
    void Start()
    {
        panicMode = FindObjectOfType<PanicMode>();
        Debug.Assert(panicMode);

        npc.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (panicMode.Panicking)
        {
            npc.SetActive(true);
        }
    }
}
