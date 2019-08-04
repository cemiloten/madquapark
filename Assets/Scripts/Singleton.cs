using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component {
    private static T instance;

    public static T Instance {
        get {
            if (instance != null) {
                return instance;
            }

            instance = FindObjectOfType<T>();
            if (instance != null) {
                return instance;
            }

            var go = new GameObject { name = typeof(T).Name };
            instance = go.AddComponent<T>();
            return instance;
        }
    }

    protected virtual void Awake() {
        if (instance == null) {
            instance = this as T;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }
}