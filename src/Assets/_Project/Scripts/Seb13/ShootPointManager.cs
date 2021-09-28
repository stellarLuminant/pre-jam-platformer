using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootPointManager : MonoBehaviour
{
    public LayerMask retreatLayerMask;
    public Vector3 retreatedPosition;
    public Vector3 defaultPosition;

    public new Collider2D collider;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider2D>();
        defaultPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    if (ShouldRetreat(collision))
    //    {
    //        Debug.Log("Moving to retreat");
    //        transform.localPosition = retreatedPosition;
    //    }
    //}

    //private void OnTriggerExit2D(Collider2D collision)
    //{
    //    if (!ShouldRetreat(collision))
    //    {
    //        Debug.Log("Moving to default");
    //        transform.localPosition = defaultPosition;
    //    }
    //}

    public bool ShouldRetreat(Collider2D collision)
    {
        return collision.IsTouchingLayers(retreatLayerMask);
    }
}
