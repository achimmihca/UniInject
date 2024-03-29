﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniInject.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace UniInject
{
    public class SceneInjectionManager : MonoBehaviour
    {
        public ESceneInjectionStatus SceneInjectionStatus { get; protected set; } = ESceneInjectionStatus.Pending;
        [Tooltip("Only inject scripts with marker interface INeedInjection")]
        public bool onlyInjectScriptsWithMarkerInterface;
        public bool logTime;
        public Injector SceneInjector { get; protected set; }

        protected readonly List<IBinder> binders = new List<IBinder>();
        protected readonly List<UnityEngine.Object> scriptsThatNeedInjection = new List<UnityEngine.Object>();
        protected readonly List<ISceneInjectionFinishedListener> sceneInjectionFinishedListeners = new List<ISceneInjectionFinishedListener>();

        protected virtual void Awake()
        {
            if (SceneInjectionStatus != ESceneInjectionStatus.Pending)
            {
                return;
            }

            DoSceneInjection();
        }

        public virtual void DoSceneInjection()
        {
            if (SceneInjectionStatus != ESceneInjectionStatus.Pending)
            {
                Debug.LogWarning("Attempt to redo scene injection.");
                return;
            }
            SceneInjectionStatus = ESceneInjectionStatus.Started;

            Stopwatch stopwatch = CreateAndStartStopwatch();

            SceneInjector = UniInjectUtils.CreateInjector();

            // Bind the scene injector itself.
            // This way it can be injected at the scene start
            // and be used to inject newly created scripts at runtime.
            SceneInjector.AddBindingForInstance(SceneInjector);

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
            SceneInjectionStatus = ESceneInjectionStatus.Finished;
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
                    DoAddBinding(binder, binding);
                }
            }

            StopAndLogTime(stopwatch, $"SceneInjectionManager - Creating bindings took <ms> ms");
        }

        protected virtual void DoAddBinding(IBinder binder, IBinding binding)
        {
            try
            {
                SceneInjector.AddBinding(binding, RebindingBehavior.Throw);
            }
            catch (RebindingException ex)
            {
                Debug.LogWarning($"{ex.Message} while processing {binder}");
            }
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
            if (!logTime)
            {
                return null;
            }

            Stopwatch stopwatch = new Stopwatch();
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
