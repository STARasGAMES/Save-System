using System;
using UnityEngine;

public class test_script : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Start1");
        //gameObject.SetActive(false);
        Destroy(gameObject);
        Debug.Log("Start2");
    }

    private void Update()
    {
        Debug.Log("Update");
    }

    private void OnDisable()
    {
        Debug.Log($"OnDisable {enabled} {isActiveAndEnabled}");
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
    }
}
