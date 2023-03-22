using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class portal : MonoBehaviour
{
    public GameObject connector;
    public portalMethod method;
    public enum portalMethod
    {
        transform,
        force
    }
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (other.gameObject.GetComponent<PlayerMovement>() != null)
            {
                //短暂距离可以用force
                if (method == portalMethod.force)
                {
                    float overshootYAxis = 0;
                    Vector3 grapplePoint = connector.transform.position;
                    Vector3 lowestPoint =
                        new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

                    float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
                    float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

                    if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

                    other.gameObject.GetComponent<PlayerMovement>().JumpToPosition(grapplePoint, highestPointOnArc);
                }
                else if (method==portalMethod.transform)
                {
                    other.gameObject.transform.position = connector.transform.position;
                }
            }
        }
    }
    
    
}
