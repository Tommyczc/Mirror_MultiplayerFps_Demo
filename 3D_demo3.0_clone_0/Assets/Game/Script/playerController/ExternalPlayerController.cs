using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExternalPlayerController : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;

    [SerializeField] private Vector3 sitdownOffset;
    public void OnSitDown()
    {
        playerCamera.transform.localPosition -= sitdownOffset;
    }

    public void OnGetUp()
    {
        playerCamera.transform.localPosition += sitdownOffset;
    }
}
