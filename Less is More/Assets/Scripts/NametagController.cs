using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NametagController : MonoBehaviour
{
    public TextMesh DisplayNameText;

    private void Awake()
    {
        DisplayNameText.text = "";
    }

    public void SetName(string name)
    {
        DisplayNameText.text = name;
    }
}
