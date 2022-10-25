using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameo.UI;

public class Demo001Controller : MonoBehaviour
{
    public BasePageManager Manager;

    private void Start()
    {
        Manager.Initialize();
    }
}
