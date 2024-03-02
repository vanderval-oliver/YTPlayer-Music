using UnityEngine;

public class MusicNotification : MonoBehaviour
{
    AndroidJavaObject notificationService;

    void Start()
    {
        notificationService = new AndroidJavaObject("com.example.NotificationService");
        notificationService.Call("Init");  // Inicializar o serviço
    }

    public void ShowNotification(string songTitle, string artist)
    {
        if (notificationService != null)
        {
            notificationService.Call("ShowNotification", songTitle, artist);
        }
    }

    public void UpdateNotification(string songTitle)
    {
        if (notificationService != null)
        {
            notificationService.Call("UpdateNotification", songTitle);
        }
    }

    public void ClearNotification()
    {
        if (notificationService != null)
        {
            notificationService.Call("ClearNotification");
        }
    }
}
