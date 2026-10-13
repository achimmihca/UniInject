using System.Diagnostics;
using System.Reflection;
using UniInject;
using UnityEngine;

// This script can be used to analyze all types of its own assembly for members that need injection.
// The loaded information will improve performance later when doing scene injection.
// Thus, such a script can be used to concentrate loading time in a place
// where the user expects it, such as in an initial loading scene or splash screen.
public class AssemblyInjectionDataLoader : MonoBehaviour
{
    private static bool isLoaded;

    void Awake()
    {
        if (isLoaded)
        {
            return;
        }

        AnalyzeTypesInAssembly();

        isLoaded = true;
    }

    private void AnalyzeTypesInAssembly()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        Assembly assembly = Assembly.GetAssembly(this.GetType());
        UniInjectUtils.LoadInjectionDataForTypesInAssembly(assembly);

        stopwatch.Stop();
        UnityEngine.Debug.Log($"Analyzed assembly {assembly} in {stopwatch.ElapsedMilliseconds} ms.");
    }
}