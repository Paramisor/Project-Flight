using UnityEngine;
#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif

public class PermissionsRationaleDialog : MonoBehaviour
{
    const int kDialogWidth = 900;
    const int kDialogHeight = 300;
    private bool windowOpen = true;

    public GUISkin skin;

    void DoMyWindow(int windowID)
    {
        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
        GUI.Label(new Rect(10, 20, kDialogWidth - 20, kDialogHeight - 50), "Please grant app access to view files.");
        GUI.Button(new Rect(10, kDialogHeight - 50, 200, 40), "No");
        if (GUI.Button(new Rect(kDialogWidth - 410, kDialogHeight - 90, 400, 80), "Yes"))
        {
#if PLATFORM_ANDROID
                Permission.RequestUserPermission(Permission.Camera);
#endif
            windowOpen = false;
        }
    }

    void OnGUI()
    {
        GUI.skin = skin;
        if (windowOpen)
        {
            Rect rect = new Rect((Screen.width / 2) - (kDialogWidth / 2), (Screen.height / 2) - (kDialogHeight / 2), kDialogWidth, kDialogHeight);
            GUI.ModalWindow(0, rect, DoMyWindow, "Permissions Request Dialog");
        }
    }
}