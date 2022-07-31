﻿
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public class ServerPeerData
{
    public int Id;
    public Inputs Inputs;
    public Transform PlayerTransform;
    public LeafBlower PlayerBlower;
    public bool FlipSprite;
    public string CurrentAnimationName;
    public bool IsPlaying;
    public string displayName;
    public ushort Score;
    public ushort HeadColor;
    public ushort BodyColor;
    public ushort FeetColor;
    public ushort RoundsPlayed;
}