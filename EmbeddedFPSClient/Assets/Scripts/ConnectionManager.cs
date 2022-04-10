using System;
using System.Net;
using DarkRift;
using DarkRift.Client.Unity;
using UnityEngine;

[RequireComponent(typeof(UnityClient))]
public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance;

    [Header("Settings")]
    [SerializeField]
    private string ipAdress;
    [SerializeField]
    private int port;

    [Header("References")]
    [SerializeField]
    private LoginManager loginManager;

    public UnityClient Client { get; private set; }

    public ushort PlayerId { get; set; }

    public LobbyInfoData LobbyInfoData { get; set; }

    public delegate void OnConnectedDelegate();
    public event OnConnectedDelegate OnConnected;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
        Client = GetComponent<UnityClient>();
    }

    private void Start()
    {

        Client.ConnectInBackground(IPAddress.Parse(ipAdress), port, false, ConnectCallback);
    }

    private void ConnectCallback(Exception exception)
    {
        if (Client.ConnectionState == ConnectionState.Connected)
        {
            OnConnected?.Invoke();
        }
        else
        {
            Debug.LogError("Unable to connect to server.");
        }
    }
}
