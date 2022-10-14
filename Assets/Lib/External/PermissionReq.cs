using UnityEngine;

#if PLATFORM_ANDROID
using UnityEngine.Android;
#endif
public class PermissionReq : MonoBehaviour
{
    GameObject dialog = null;
    public GUISkin skin;

    public bool hasAccess
    {
        get
        {

#if PLATFORM_ANDROID
                 return Permission.HasUserAuthorizedPermission(Permission.Camera); 
#else
            return false;
#endif
        }
    }
    void Start()
    {
#if PLATFORM_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
            dialog = new GameObject();
        }
#endif
    }


    void OnGUI()
    {
        GUI.skin = skin;
#if PLATFORM_ANDROID
                if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
                {
                    // The user denied permission to use the microphone.
                    // Display a message explaining why you need it with Yes/No buttons.
                    // If the user says yes then present the request again
                    // Display a dialog here.
                    dialog.AddComponent<PermissionsRationaleDialog>().skin = skin;
                    return;
                }
                else if (dialog != null)
                {
                    Destroy(dialog);
                }
#endif

        // Now you can do things with the microphone
    }
}