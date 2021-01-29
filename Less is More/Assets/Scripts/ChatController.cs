using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    public Text ChatBoxText;
    public InputField InputField;
    public GameObject ChatParent;
    public int MaxMessagesInBox;
    public bool PollForEnter = false;

    private List<string> messages;
    private string oldText = "";
    private bool typing;

    public event Action<string> MessageEntered;

    private void Start()
    {
        messages = new List<string>();
        UpdateChatBox();
        HideChat();
    }

    public void ShowChat()
    {
        ChatParent.SetActive(true);
    }

    public void HideChat()
    {
        ChatParent.SetActive(false);
    }

    public void Update()
    {
        if (PollForEnter && Input.GetKeyDown(KeyCode.Return))
        {
            if (typing)
            {
                if (InputField.text != "")
                {
                    Debug.Log("Sending");
                    MessageEntered?.Invoke(InputField.text);
                    InputField.SetTextWithoutNotify("");
                    oldText = "";
                }
                
                InputField.interactable = false;
                typing = false;
            }
            else
            {
                InputField.interactable = true;
                InputField.Select();
                InputField.ActivateInputField();
                typing = true;
            }
        }
    }

    public void OnInputChanged(string newText)
    {
        Debug.Log(newText);
        Regex rg = new Regex(@"^[a-zA-Z0-9 ]{0,17}$");
        if (!rg.IsMatch(newText))
        {
            InputField.SetTextWithoutNotify(oldText);
        }
        else
        {
            oldText = newText;
        }
    }

    public void PushNewMessage(string sender, string message)
    {
        messages.Add($"{sender}: {message}");
        UpdateChatBox();
    }

    public void UpdateChatBox()
    {
        string newString = "";
        if (messages.Count > MaxMessagesInBox)
        {
            for (int i = messages.Count - MaxMessagesInBox; i < messages.Count; i++)
            {
                string message = messages[i];
                newString += message + "\n";
            }
        }
        else
        {
            for (int i = 0; i < messages.Count; i++)
            {
                string message = messages[i];
                newString += message + "\n";
            }
        }

        ChatBoxText.text = newString;
    }
}
