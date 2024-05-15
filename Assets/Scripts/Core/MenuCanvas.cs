using UnityEngine;


public class MenuCanvas : MonoBehaviour
{
    [SerializeField] private MenuType menuType;

    public enum MenuType { Garage, Pause, Result ,Interface }

    /// <summary>
    /// Gets menu type of canvas
    /// </summary>
    /// <returns> Menue type this canvas represents </returns>
    public MenuType GetMenuType() { return menuType; }
}


