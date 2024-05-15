using ScriptableObjects;
using TMPro;
using UnityEngine;

public class Interface : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] FloatEventChannelSO playerSpeedChangeEventChannel;
    [SerializeField] VoidEventChannelSO collectCoinEventChannel;

    [Header("Interface Components")]
    [SerializeField] TextMeshProUGUI coinCounter;
    [SerializeField] TextMeshProUGUI playerSpeed;

    int collectedCoins;

    void OnEnable()
    {
        if (playerSpeedChangeEventChannel != null)
            playerSpeedChangeEventChannel.OnEventRaised += UpdateSpeed;
        if (collectCoinEventChannel != null)
            collectCoinEventChannel.OnEventRaised += AddCoin;
    }

    void OnDisable()
    {
        if (playerSpeedChangeEventChannel != null)
            playerSpeedChangeEventChannel.OnEventRaised -= UpdateSpeed;
        if (collectCoinEventChannel != null)
            collectCoinEventChannel.OnEventRaised -= AddCoin;
    }

    public void AddCoin() 
    {
        collectedCoins++;
        coinCounter.text = collectedCoins.ToString();
    }

    public void UpdateSpeed(float speed)
    {
        playerSpeed.text = "Suspect Speed: " + Mathf.Round(speed).ToString() + " km/h";
    }
}
