using UnityEngine;
using System;

public class KeyCodeTester : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    Debug.Log("KeyCode «ö¤U¡G" + kcode);
                }
            }
        }
    }
}
