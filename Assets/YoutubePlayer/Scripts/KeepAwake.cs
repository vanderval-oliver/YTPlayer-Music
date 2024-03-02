using UnityEngine;

public class KeepAwake : MonoBehaviour
{
    private AndroidJavaObject wakeLock;

    void Start()
    {
        // Somente se a plataforma for Android
        if (Application.platform == RuntimePlatform.Android)
        {
            // Obtém o contexto atual
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            // Obtém um serviço de energia
            AndroidJavaClass powerManager = new AndroidJavaClass("android.os.PowerManager");
            string wakeLockType = powerManager.GetStatic<string>("FULL_WAKE_LOCK");

            // Cria um WakeLock
            wakeLock = powerManager.CallStatic<AndroidJavaObject>("newWakeLock", wakeLockType, "MyWakeLock");
            wakeLock.Call("acquire");
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Libera o WakeLock quando o aplicativo for pausado
        if (wakeLock != null)
        {
            wakeLock.Call("release");
        }
    }
}
