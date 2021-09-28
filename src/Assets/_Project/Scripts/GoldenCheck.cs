using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenCheck : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            var chicken = collision.GetComponent<Chicken>();
            if (chicken && chicken.isGolden)
            {
                Debug.Log("Verified gold chicken");
                FindObjectOfType<VariableStorageBehaviour>().SetValue(
                    "$obtained_chicken", true);
            }
        }
    }
}
