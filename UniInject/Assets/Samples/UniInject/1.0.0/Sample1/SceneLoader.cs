using UniInject;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [InjectedInInspector]
    public string sceneName;
    
    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame
            || Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
