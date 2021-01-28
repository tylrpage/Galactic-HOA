
using NetStack.Quantization;
using NetStack.Serialization;
using UnityEngine;

public class ClientPeerData
{
    public int Id;
    public PositionInterp PositionInterp;
    public AnimationController AnimationController;
    public Transform PlayerTransform;
    public bool IsPlaying;
    public Inputs Inputs;
    public LeafBlower LeafBlower;
}