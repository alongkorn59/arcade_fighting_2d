using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField] private GameObject cmCamera;
    [SerializeField] private Transform[] targets;
    public int FocusIndex = 0;
    private bool isFocusing = false;

    #region Follow Up
    [SerializeField] private Vector3 offset;
    private float smoothTime;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 startPosition = Vector3.zero;
    #endregion

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isFocusing)
        {
            Vector3 targetsPosition = targets[FocusIndex].position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetsPosition, ref currentVelocity, smoothTime);
        }
    }

    public void StartFocusing()
    {
        cmCamera.SetActive(false);
        transform.rotation = new Quaternion();
        isFocusing = true;
    }

    public void SwitchTarget()
    {
        FocusIndex = FocusIndex == 0 ? 1 : 0;
    }

    public void StopFocusing()
    {
        isFocusing = false;
        cmCamera.SetActive(true);
    }
}
