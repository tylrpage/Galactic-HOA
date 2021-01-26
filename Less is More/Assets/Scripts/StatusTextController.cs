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
        StatusText.text = $"Players in circle: {count}/2";
    }

    public void SetRoundAboutToStart(short time)
    {
        CancelRoundStart();
        _roundCountdown = StartCoroutine(RoundCountdown(time));
    }

    public void CancelRoundStart()
    {
        if (_roundCountdown != null)
            StopCoroutine(_roundCountdown);
    }
    
    private IEnumerator RoundCountdown(short time)
    {
        for (int i = time; i >= 1; i--)
        {
            StatusText.text = $"Round starting in: {i}";
            yield return new WaitForSeconds(1);
        }
    }
}
