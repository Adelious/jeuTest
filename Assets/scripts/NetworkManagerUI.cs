using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
 using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject networkManager;
    [Header("Bouttons")]
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TextMeshProUGUI portField;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button quitButton;


    private void Awake(){
        serverButton.onClick.AddListener(() => {
            networkManager.GetComponent<UnityTransport>().ConnectionData.Address = portField.text;
            NetworkManager.Singleton.StartServer();
        });
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
        });
        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
        });
        leaveButton.onClick.AddListener(() => {
            Debug.Log("Deconnexion");
        });
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
    }
}
