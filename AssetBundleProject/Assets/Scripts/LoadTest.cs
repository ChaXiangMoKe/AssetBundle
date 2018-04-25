using UnityEngine;
using System;
using System.Collections;

public class LoadTest : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        RGResource.LoadGameObjectAsync("Prefabs/CubeWall", Test, 1);

    }

    // Update is called once per frame
    void Update()
    {

    }

    void Test(GameObject prefab, LoadEventData evData)
    {
        GameObject go = Instantiate(prefab) as GameObject;
        go.transform.position = new Vector3(1, 1, 1);
    }
}
