using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class FerrisWheelSpinner : MonoBehaviour
{

    [SerializeField] private float turnSpeed;
    [SerializeField] private List<GondolaScript> gondolaScripts;
    [SerializeField] private GameObject emptyPrefab;
    private List<GameObject> positionMarkers = new List<GameObject>();

    void Start()
    {
        foreach(GondolaScript gondola in gondolaScripts)
        {
            GameObject tempObj = Instantiate(emptyPrefab,this.transform);
            tempObj.transform.position = gondola.transform.position;
            gondola.anchorPoint = tempObj;
            positionMarkers.Add(tempObj);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float zRotation = this.transform.eulerAngles.z + (turnSpeed * Time.deltaTime);
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, zRotation);
    }
}
