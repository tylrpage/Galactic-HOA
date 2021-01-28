using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorController : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] private SpriteRenderer headSprite;
    [SerializeField] private SpriteRenderer bodySprite;
    [SerializeField] private SpriteRenderer lFootSprite;
    [SerializeField] private SpriteRenderer rFootSprite;
#pragma warning restore 0649
    
    private GameController _gameController;

    private void Awake()
    {
        _gameController = Camera.main.GetComponent<GameController>();
    }

    public void SetPlayerColors(ushort head, ushort body, ushort feet)
    {
        headSprite.color = _gameController.ConvertColorCodeToColor(head);
        bodySprite.color = _gameController.ConvertColorCodeToColor(body);
        lFootSprite.color = _gameController.ConvertColorCodeToColor(feet);
        rFootSprite.color = _gameController.ConvertColorCodeToColor(feet);
    }
}
