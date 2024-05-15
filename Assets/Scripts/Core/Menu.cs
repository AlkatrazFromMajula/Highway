using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ScriptableObjects;
using System;

public class Menu : MonoBehaviour
{
    #region Fields

    [Header("Event Channels")]
    [SerializeField] VoidEventChannelSO pauseEventChannel;
    [SerializeField] VoidEventChannelSO beginGameEventChannel;
    [SerializeField] VoidEventChannelSO reloadSceneEventChannel;
    [SerializeField] VoidEventChannelSO quitAppEventChannel;
    [SerializeField] VoidEventChannelSO buyCarEventChannel;
    [SerializeField] VoidEventChannelSO muteAudioEventChannel;
    [SerializeField] VoidEventChannelSO finishEventChannel;
    [SerializeField] VoidEventChannelSO pickCoinEventChannel;
    [SerializeField] IntEventChannelSO swapCarEventChannel;
    [SerializeField] FloatEventChannelSO playerSpeedChangeEventChannel;

    [Header("DataContainers")]
    [SerializeField] EssentialDataManagerSO essentialData;

    [Header("Menu Components")]
    [SerializeField] GameObject garageMenu;
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject interfaceMenu;
    [SerializeField] GameObject resultMenu;

    [Header("Rest")]
    [SerializeField] TextMeshProUGUI coinCounter_Interface_TMP;
    [SerializeField] TextMeshProUGUI coinCounter_Result_TMP;
    [SerializeField] TextMeshProUGUI coinCounter_Garage_TMP;
    [SerializeField] TextMeshProUGUI playerSpeed_TMP;
    [SerializeField] TextMeshProUGUI carPrice_TMP;
    [SerializeField] Button buyButton;
    [SerializeField] Image sceneTransitor;

    int coinCounter;
    bool isPaused;
    bool mute;

    #endregion

    private void OnEnable()
    {
        // listen to event channels
        if (pauseEventChannel != null)
            pauseEventChannel.OnEventRaised += Pause;
        if (reloadSceneEventChannel != null)
            reloadSceneEventChannel.OnEventRaised += ReloadScene;
        if (quitAppEventChannel != null)
            quitAppEventChannel.OnEventRaised += Quit;
        if (buyCarEventChannel != null)
            buyCarEventChannel.OnEventRaised += BuyCar;
        if (swapCarEventChannel != null)
            swapCarEventChannel.OnEventRaised += SwapCar;
        if (finishEventChannel != null)
            finishEventChannel.OnEventRaised += Finish;
        if (playerSpeedChangeEventChannel != null)
            playerSpeedChangeEventChannel.OnEventRaised += UpdatePlayerSpeedLabel;
        if (pickCoinEventChannel != null)
            pickCoinEventChannel.OnEventRaised += UpdateCoinCounter;

        UpdatePlayerSpeedLabel(0);

        UpdateGarage();
        StartCoroutine(OpenScene());
    }

    // Disable input
    private void OnDisable()
    {
        // stop listening to event channels
        if (pauseEventChannel != null)
            pauseEventChannel.OnEventRaised -= Pause;
        if (reloadSceneEventChannel != null)
            reloadSceneEventChannel.OnEventRaised -= ReloadScene;
        if (quitAppEventChannel != null)
            quitAppEventChannel.OnEventRaised -= Quit;
        if (buyCarEventChannel != null)
            buyCarEventChannel.OnEventRaised -= BuyCar;
        if (swapCarEventChannel != null)
            swapCarEventChannel.OnEventRaised -= SwapCar;
        if (finishEventChannel != null)
            finishEventChannel.OnEventRaised -= Finish;
        if (playerSpeedChangeEventChannel != null)
            playerSpeedChangeEventChannel.OnEventRaised -= UpdatePlayerSpeedLabel;
    }

    #region Coroutines

    // Smoothly load scene
    private IEnumerator LoadLevel(string sceneName)
    {
        mute = true;
        // obscure screen
        Color color = sceneTransitor.color;
        sceneTransitor.enabled = true;
        while (color.a < 1)
        {
            color.a += Time.unscaledDeltaTime;
            sceneTransitor.color = color;
            yield return new WaitForEndOfFrame();
        }
        if (color.a != 1) { color.a = 1; }
        sceneTransitor.color = color;
        yield return new WaitForSecondsRealtime(1);
        mute = false;

        muteAudioEventChannel.RaiseEvent();
        SceneManager.LoadScene(sceneName);
    }

    // Smoothly open scene
    private IEnumerator OpenScene()
    {
        // mute any actions while opening scenes 
        mute = true;

        // prepare for scene opening
        Time.timeScale = 0;
        sceneTransitor.enabled = true;
        Color color = sceneTransitor.color;
        color.a = 1;
        sceneTransitor.color = color;

        // smoothly illuminate scene
        yield return new WaitForSecondsRealtime(1);
        while (color.a > 0)
        {
            color.a -= Time.unscaledDeltaTime / 2;
            sceneTransitor.color = color;
            yield return new WaitForEndOfFrame();
        }

        // clamp values
        if (color.a != 0) { color.a = 0; }
        sceneTransitor.color = color;
        sceneTransitor.enabled = false;
        Time.timeScale = 1.0f;

        // allow any actions
        mute = false;
    }

    // Smoothly pause game
    private IEnumerator SmoothPause()
    {
        // prohibit any actions while pausing
        mute = true;
        isPaused = true;

        // prepare canvas, vignette and audio for pausing
        pauseMenu.SetActive(true);
        CanvasScaler scaler = pauseMenu.GetComponent<CanvasScaler>();
        //pauseMenu.GetComponentInChildren<Button>().Select();
        if (scaler.scaleFactor != 0) { scaler.scaleFactor = 0; }

        // smoothly pause game
        while (scaler.scaleFactor < 1)
        {
            scaler.scaleFactor += Time.unscaledDeltaTime * 6;
            if (Time.timeScale - Time.unscaledDeltaTime * 6 > 0) { Time.timeScale -= Time.unscaledDeltaTime * 6; }
            yield return new WaitForEndOfFrame();
        }

        // clamp values
        if (scaler.scaleFactor != 1) { scaler.scaleFactor = 1; }
        if (Time.timeScale != 0.0f) { Time.timeScale = 0.0f; }

        // show mouse cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // allow any actions
        mute = false;
    }

    // Smoothly resume game
    private IEnumerator SmoothResume()
    {
        // prohibit any actions while resuming
        mute = true;
        isPaused = false;

        // hide mouse coursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // prepare canvas and vignette
        CanvasScaler scaler = pauseMenu.GetComponent<CanvasScaler>();


        // smoothly unpause
        while (scaler.scaleFactor > 0.01f)
        {
            scaler.scaleFactor -= Time.unscaledDeltaTime * 6;
            if (Time.timeScale < 1) Time.timeScale += Time.unscaledDeltaTime * 6;
            yield return new WaitForEndOfFrame();
        }

        // clamp values
        if (scaler.scaleFactor != 0) { scaler.scaleFactor = 0; }
        if (Time.timeScale != 1.0f) { Time.timeScale = 1.0f; }
        pauseMenu.SetActive(false);

        // allow any actions
        mute = false;
    }

    // Smoothly anounce result
    private IEnumerator SmoothResult()
    {
        // mute any actions while anouncing
        mute = true;
        yield return new WaitForSecondsRealtime(2);
        interfaceMenu.SetActive(false);

        // show mouse cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // prepare canvas, label ect. for pausing
        CanvasScaler scaler = resultMenu.GetComponent<CanvasScaler>();
        resultMenu.SetActive(true);
        //resultMenu.GetComponentInChildren<Button>().Select();
        if (scaler.scaleFactor != 0) { scaler.scaleFactor = 0; }

        // smoothly pause
        while (scaler.scaleFactor < 1)
        {
            scaler.scaleFactor += Time.deltaTime * 6;
            yield return new WaitForEndOfFrame();
        }

        // clamp values
        if (scaler.scaleFactor != 1) { scaler.scaleFactor = 1; }
        Time.timeScale = 0.0f;
    }


    #endregion

    // Reload current scene
    void ReloadScene() 
    {
        StartCoroutine(LoadLevel(SceneManager.GetActiveScene().name)); 
    }

    // Quit application
    void Quit() 
    { 
        Application.Quit(); 
    }

    // Pause game if it's not already paused, otherwise unpause it
    void Pause()
    {
        if (!isPaused && !mute) 
        {
            if (muteAudioEventChannel != null)
                muteAudioEventChannel.RaiseEvent();
            StartCoroutine(SmoothPause()); 
        }
        else if (isPaused && !mute) 
        {
            if (muteAudioEventChannel != null)
                muteAudioEventChannel.RaiseEvent();
            StartCoroutine(SmoothResume());  
        }
    }

    void Finish()
    {
        interfaceMenu.SetActive(false);
        coinCounter_Result_TMP.text = coinCounter_Interface_TMP.text;
        essentialData.Budget += Int32.Parse(coinCounter_Result_TMP.text);

        if (pauseMenu.activeInHierarchy)
        {
            pauseMenu.SetActive(false);
            resultMenu.SetActive(true);
        }
        else
        {
            if (muteAudioEventChannel != null)
                muteAudioEventChannel.RaiseEvent();
            StartCoroutine(SmoothResult());
        }

    }

    public void BeginGame()
    {
        if (essentialData.GetSelectedCar().IsBought && essentialData.TrySpawnSelectedCar(true))
        {
            if (beginGameEventChannel != null)
                beginGameEventChannel.RaiseEvent();

            garageMenu.SetActive(false);
            interfaceMenu.SetActive(true);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void BuyCar()
    {
        if (essentialData.TryBuySelectedCar())
        {
            UpdateGarage();
        }
    }

    void SwapCar(int direction)
    {
        direction = Mathf.Clamp(direction, -1, 1);
        if (essentialData.TrySwapSelectedCar(direction))
        {
            UpdateGarage();
        }
    }

    void UpdateGarage()
    {
        coinCounter_Garage_TMP.text = essentialData.Budget.ToString();
        if (essentialData.TrySpawnSelectedCar(false))
        {
            if (essentialData.GetSelectedCar().IsBought)
            {
                buyButton.gameObject.SetActive(false);
                carPrice_TMP.gameObject.SetActive(false);
            }
            else
            {
                buyButton.gameObject.SetActive(true);
                carPrice_TMP.gameObject.SetActive(true);
                carPrice_TMP.text = essentialData.GetSelectedCar().Price.ToString();
            }
        }
        else
        {
            Debug.LogWarning("Car could not be spawned");
        }
    }

    void UpdatePlayerSpeedLabel(float value)
    {
        value = Mathf.Round(value * 100);
        playerSpeed_TMP.text = "Suspect Speed: " + value + "%";
    }

    void UpdateCoinCounter()
    {
        coinCounter++;
        coinCounter_Interface_TMP.text = coinCounter.ToString();
    }
}