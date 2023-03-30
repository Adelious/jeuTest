using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
 using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("Bouttons")]
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TextMeshProUGUI ipField;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button quitButton;


    private void Awake(){
        serverButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartServer();
        });
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        clientButton.onClick.AddListener(() => {
            if (ipField.text.ToString().Remove(ipField.text.Length-1) != null) {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                    ipField.text.ToString().Remove(ipField.text.Length-1),  
                    (ushort)7777
                );
                NetworkManager.Singleton.StartClient();
            }
        });
        leaveButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
        });
        quitButton.onClick.AddListener(() => {
            NetworkManager.Singleton.Shutdown();
            Application.Quit();
        });
    }
}
