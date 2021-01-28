using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ConnectUIController : MonoBehaviour
{
    [SerializeField] private InputField NameInput;
    [SerializeField] private Button ConnectButton;
    [SerializeField] private GameObject ConnectScreenGroup;
    [SerializeField] private Sprite ConnectSprite;
    [SerializeField] private Sprite ConnectingSprite;

    public event Action<string> ConnectPressed;
    
    public string DisplayName { get; private set; }

    public void OnNameInputChanged(string newName)
    {
        Regex rg = new Regex(@"^[a-zA-Z0-9]{2,20}$");
        if (rg.IsMatch(newName))
        {
            DisplayName = newName;
            ConnectButton.interactable = true;
        }
        else
        {
            ConnectButton.interactable = false;
        }
    }

    private void Update()
    {
        if (ConnectScreenGroup.activeInHierarchy && Input.GetKeyDown(KeyCode.Return) && ConnectButton.interactable)
        {
            ConnectButton.onClick.Invoke();
        }
    }

    public void OnConnectPressed()
    {
        ConnectPressed?.Invoke(NameInput.text);
        ConnectButton.GetComponent<Image>().sprite = ConnectingSprite;
        DisableConnectButton();
    }

    public void SwitchToConnectButtonSprite()
    {
        ConnectButton.GetComponent<Image>().sprite = ConnectSprite;
    }

    public void DisableConnectButton()
    {
        ConnectButton.interactable = false;
    }

    public void EnableConnectButton()
    {
        ConnectButton.interactable = true;
    }

    public void ShowConnectScreen()
    {
        ConnectScreenGroup.SetActive(true);
    }

    public void HideConnectScreen()
    {
        ConnectScreenGroup.SetActive(false);
    }
}
