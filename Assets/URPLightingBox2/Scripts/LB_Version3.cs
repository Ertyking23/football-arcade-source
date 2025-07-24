#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class Startup
{
    static Startup()    
    {
        EditorPrefs.SetInt("showCounts_sportcarlb3", EditorPrefs.GetInt("showCounts_sportcarlb3") + 1);

        if (EditorPrefs.GetInt("showCounts_sportcarlb3") == 1)       
        {
            Application.OpenURL("https://u3d.as/278u");
            // System.IO.File.Delete("Assets/SportCar/Racing_Game.cs");
        }
    }     
}
#endif
