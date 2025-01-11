using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : AIAgent
{
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            Move(new Vector2Int(0, 1));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Move(new Vector2Int(0, -1));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Move(new Vector2Int(-1, 0));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Move(new Vector2Int(1, 0));
        }
    }
}
