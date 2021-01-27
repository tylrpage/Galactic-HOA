using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusTextController : MonoBehaviour
{
    public Text StatusText;

    private Coroutine _roundCountdown;

    public void SetWaitingForPlayers(int count)
    {
        StatusText.text = $"Waiting for your neighbors: {count}/{Constants.PLAYER_NEEDED}";
    }

    public void SetLiftOffCountdown(short time)
    {
        CancelRoundStart();
        _roundCountdown = StartCoroutine(LiftOffCountdown(time));
    }

    public void SetFlyingCountdown(short time)
    {
        StartCoroutine(RoundFlyingCountdown(time));
    }

    public void SetRoundCountdown(short time)
    {
        StartCoroutine(RoundCountdown(time));
    }

    public void SetLandingText()
    {
        StatusText.text = $"Landing...";
    }

    public void CancelRoundStart()
    {
        if (_roundCountdown != null)
            StopCoroutine(_roundCountdown);
    }
    
    private IEnumerator LiftOffCountdown(short time)
    {
        for (int i = time; i >= 1; i--)
        {
            StatusText.text = $"Commencing lift off in: {i}";
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator RoundFlyingCountdown(short time)
    {
        for (int i = time; i >= 1; i--)
        {
            StatusText.text = $"Round starts in: {i}";
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator RoundCountdown(short time)
    {
        for (int i = time; i >= 1; i--)
        {
            StatusText.text = $"Time left in round: {i}";
            yield return new WaitForSeconds(1);
        }
        StatusText.text = $"Round over!";
    }
}
