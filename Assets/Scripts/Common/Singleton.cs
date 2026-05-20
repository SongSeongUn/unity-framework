using Sirenix.OdinInspector;
using UnityEngine;

namespace Common
{
    public class Singleton<T> where T : class, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new T();
                return _instance;
            }
        }

        public static bool IsValid() => Instance != null;
    }

    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting = false;

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    DebugUtils.LogWarning($"[MonoSingleton] {typeof(T).Name} 인스턴스가 종료 중에 요청되었습니다. null을 반환합니다.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        // 씬 배치 확인
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null)
                        {
                            GameObject obj = new GameObject(typeof(T).Name);
                            _instance = obj.AddComponent<T>();

                            DontDestroyOnLoad(obj);
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                // 중복 생성된 경우 파괴
                Destroy(gameObject);
            }
        }

        public static bool IsValid() => Instance != null;

        protected virtual void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        protected virtual void OnApplicationQuit()
        {
            _instance = null;
            _isQuitting = true;
        }
    }

    /// <summary>
    /// Scene에 종속 된 Singleton
    /// Scene이 파괴되면 같이 삭제
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSceneSingleton<T> : MonoBehaviour where T : MonoSceneSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        Debug.LogWarning($"[SceneSingleton] {typeof(T).Name}가 씬에 없습니다.");
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null) _instance = this as T;
            else if (_instance != this) Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}