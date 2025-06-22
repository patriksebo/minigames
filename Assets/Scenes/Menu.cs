using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    NetworkManager manager;
    public GameObject menuCamera;
    public GameObject menuCanvas;
    public InputField iptext;

    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<NetworkManager>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartGameHost()
    {
        manager.StartHost();
        menuCamera.SetActive(false);
        menuCanvas.SetActive(false);
    }
    public void StartGameClient()
    {
        manager.networkAddress = iptext.text;
        Debug.Log(iptext.text);
        manager.StartClient();
        menuCamera.SetActive(false);
        menuCanvas.SetActive(false);
    }
    public void Quit()
    {
        Application.Quit();
    }
}
