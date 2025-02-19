﻿using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;
using UnityEngine.UI;

public class ChatDemo : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The client to communicate with the server via.")]
    private UnityClient client;

    [SerializeField]
    [Tooltip("The InputField the user can type in.")]
    private InputField input;

    [SerializeField]
    [Tooltip("The transform to place new messages in.")]
    private Transform chatWindow;

    [SerializeField]
    [Tooltip("The scrollrect for the chat window (if present).")]
    private ScrollRect scrollRect;

    [SerializeField]
    [Tooltip("The message prefab where messages will be added.")]
    private GameObject messagePrefab;

    private void Awake()
    {
        //Check we have a client to send/receive from
        if (client == null)
        {
            Debug.LogError("No client assigned to Chat component!");
            return;
        }

        //Subscribe to the event for when we receive messages
        client.MessageReceived += Client_MessageReceived;
        client.Disconnected += Client_Disconnected;
    }

    private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        //Get an instance of the message received
        using (Message message = e.GetMessage() as Message)
        {
            //Get the DarkRiftReader from the message and read the text in it into the UI
            using (DarkRiftReader reader = message.GetReader())
                AddMessage(reader.ReadString());
        }
    }

    private void Client_Disconnected(object sender, DisconnectedEventArgs e)
    {
        //If we've disconnected add a message to say whether it was us or the server that triggered the 
        //disconnection
        if (e.LocalDisconnect)
            AddMessage("You have disconnected from the server.");
        else
            AddMessage("You were disconnected from the server.");
    }

    private void AddMessage(string message)
    {
        //Now we need to create a new UI object to put the message in so instantiate our prefab and add it 
        //as a child to the chat window
        GameObject messageObj = Instantiate(messagePrefab) as GameObject;
        messageObj.transform.SetParent(chatWindow);

        //We need the Text component so search for it
        Text text = messageObj.GetComponentInChildren<Text>();

        //If the Text component is present then assign the text out message
        if (text != null)
            text.text = message;
        else
            Debug.LogError("Message object does not contain a Text component!");

        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    //This will be called when the user presses enter in the input field
    public void MessageEntered()
    {
        //Check we have a client to send from
        if (client == null)
        {
            Debug.LogError("No client assigned to Chat component!");
            return;
        }

        //First we need to build a DarkRiftWriter to put the data we want to send in, it'll default to Unicode 
        //encoding so we don't need to worry about that
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            //We can then write the input text into it
            writer.Write(input.text);

            //Next we construct a message, in this case we can just use a default tag because there is nothing fancy
            //that needs to happen before we read the data.
            using (Message message = Message.Create(0, writer))
            {
                //Finally we send the message to everyone connected!
                client.SendMessage(message, SendMode.Reliable);
            }
        }
    }
}
