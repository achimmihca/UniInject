using UnityEngine;

// An instance of this class is injected only when needed, because the Binder is binding the instance lazy.
public class LazyInjectionDemo
{
    private static LazyInjectionDemo instance;
    public static LazyInjectionDemo Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LazyInjectionDemo();
                Debug.Log("Created instance of LazyInjectionDemo");
            }
            return instance;
        }
    }
}
