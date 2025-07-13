using UnityEngine;
using System.Collections;

public class DebugStartup : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DebugInitialize());
    }
    
    IEnumerator DebugInitialize()
    {
        Debug.Log("DebugStartup: Beginning simple initialization");
        
        yield return null;
        
        // Only create the debug desktop manager
        if (Windows31DesktopManagerDebug.Instance == null)
        {
            GameObject desktopGO = new GameObject("Windows31DesktopManagerDebug");
            desktopGO.AddComponent<Windows31DesktopManagerDebug>();
        }
        
        yield return new WaitForSeconds(2f);
        
        Debug.Log("DebugStartup: Complete");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (Windows31DesktopManagerDebug.Instance != null)
            {
                Debug.Log($"Desktop Manager Status: {Windows31DesktopManagerDebug.Instance.GetDebugInfo()}");
            }
        }
    }
}