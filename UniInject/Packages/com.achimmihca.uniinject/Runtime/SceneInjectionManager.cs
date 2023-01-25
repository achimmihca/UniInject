using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniInject.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

namespace UniInject
{
    public class SceneInjectionManager : MonoBehaviour
    {
        protected readonly List<IBinder> binders = new List<IBinder>();
        protected readonly List<UnityEngine.Object> scriptsThatNeedInjection = new List<UnityEngine.Object>();
        protected readonly List<ISceneInjectionFinishedListener> sceneInjectionFinishedListeners = new List<ISceneInjectionFinishedListener>();

        public Injector SceneInjector { get; protected set; }

        [Tooltip("Only inject scripts with marker interface INeedInjection")]
        public bool onlyInjectScriptsWithMarkerInterface;

        public bool logTime;

        public string uiDocumentTagName;

        protected virtual void Awake()
        {
            DoSceneInjection();
        }

        public virtual void DoSceneInjection()
        {
            Stopwatch stopwatch = CreateAndStartStopwatch();

            SceneInjector = UniInjectUtils.CreateInjector();

            // Bind the scene injector itself.
            // This way it can be injected at the scene start
            // and be used to inject newly created scripts at runtime.
            SceneInjector.AddBindingForInstance(SceneInjector);

            // Try to find and bind a UIDocument
            if (!string.IsNullOrEmpty(uiDocumentTagName))
            {
                GameObject uiDocumentGameObject = GameObject.FindGameObjectWithTag(uiDocumentTagName);
                if (uiDocumentGameObject != null)
                {
                    UIDocument uiDocument = uiDocumentGameObject.GetComponent<UIDocument>();
                    if (uiDocument != null)
                    {
                        SceneInjector.AddBindingForInstance(uiDocument);
                    }
                    else
                    {
                        Debug.LogWarning($"No UIDocument found for tag {uiDocumentTagName}");
                    }
                }
            }

            // (1) Iterate over scene hierarchy, thereby
            // (a) find IBinder instances.
            // (b) find scripts that need injection and how their members should be injected.
            AnalyzeScene();

            // (2) Store bindings in the sceneInjector
            CreateBindings();

            // (3) Inject the bindings from the sceneInjector into the objects that need injection.
            InjectScriptsThatNeedInjection();

            StopAndLogTime(stopwatch, $"SceneInjectionManager - Analyzing, binding and injecting scene took <ms> ms");

            // (4) Notify listeners that scene injection has finished
            foreach (ISceneInjectionFinishedListener listener in sceneInjectionFinishedListeners)
            {
                listener.OnSceneInjectionFinished();
            }
        }

        protected virtual void AnalyzeScene()
        {
            Stopwatch stopwatch = CreateAndStartStopwatch();

            Scene scene = SceneManager.GetActiveScene();
            GameObject[] rootObjects = GetRootGameObjects(scene);
            foreach (GameObject rootObject in rootObjects)
            {
                AnalyzeScriptsRecursively(rootObject);
            }

            StopAndLogTime(stopwatch, $"SceneInjectionManager - Analyzing scene {scene.name} took <ms> ms");
        }

        protected virtual GameObject[] GetRootGameObjects(Scene scene)
        {
            return scene.GetRootGameObjects();
        }

        protected virtual void CreateBindings()
        {
            Stopwatch stopwatch = CreateAndStartStopwatch();

            foreach (IBinder binder in binders)
            {
                List<IBinding> bindings = binder.GetBindings();
                foreach (IBinding binding in bindings)
                {
                    SceneInjector.AddBinding(binding);
                }
            }

            StopAndLogTime(stopwatch, $"SceneInjectionManager - Creating bindings took <ms> ms");
        }

        protected virtual void InjectScriptsThatNeedInjection()
        {
            Stopwatch stopwatch = CreateAndStartStopwatch();

            foreach (UnityEngine.Object script in scriptsThatNeedInjection)
            {
                try
                {
                    DoInject(script);
                }
                catch (InjectionException e)
                {
                    UnityEngine.Debug.LogException(e, script);
                    // Continue injection of other scripts.
                }
            }

            StopAndLogTime(stopwatch, $"SceneInjectionManager - Injecting scripts took <ms> ms");
        }

        protected virtual void DoInject(UnityEngine.Object script)
        {
            SceneInjector.Inject(script);
        }

        protected virtual Stopwatch CreateAndStartStopwatch()
        {
            Stopwatch stopwatch = null;
            if (logTime)
            {
                return stopwatch;
            }

            stopwatch = new Stopwatch();
            stopwatch.Start();
            return stopwatch;
        }

        protected virtual void StopAndLogTime(Stopwatch stopwatch, string message)
        {
            if (stopwatch == null)
            {
                return;
            }
            stopwatch.Stop();
            UnityEngine.Debug.Log(message.Replace("<ms>", stopwatch.ElapsedMilliseconds.ToString()));
        }

        protected virtual void AnalyzeScriptsRecursively(GameObject gameObject)
        {
            MonoBehaviour[] scripts = gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                // The script can be null if it is a missing component.
                if (script == null)
                {
                    continue;
                }

                // Analyzing a type for InjectionData is costly.
                // The types of the UnityEngine do not make use of UniInject.
                // Thus, the scripts from the UnityEngine itself should be skipped for better performance.
                Type type = script.GetType();
                if (!string.IsNullOrEmpty(type.Namespace) && type.Namespace.StartsWith("UnityEngine."))
                {
                    continue;
                }

                if (script is IBinder)
                {
                    binders.Add(script as IBinder);
                }

                if (script is ISceneInjectionFinishedListener)
                {
                    sceneInjectionFinishedListeners.Add(script as ISceneInjectionFinishedListener);
                }

                if ((!onlyInjectScriptsWithMarkerInterface || script is INeedInjection)
                    && !(script is IExcludeFromSceneInjection))
                {
                    List<InjectionData> injectionDatas = UniInjectUtils.GetInjectionDatas(script.GetType());
                    if (injectionDatas.Count > 0)
                    {
                        scriptsThatNeedInjection.Add(script);
                    }
                }
            }

            foreach (Transform child in gameObject.transform)
            {
                AnalyzeScriptsRecursively(child.gameObject);
            }
        }
    }
}
