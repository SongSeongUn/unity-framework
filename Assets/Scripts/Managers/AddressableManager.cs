using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEngine.U2D;

public class AddressableManager : MonoBehaviour
{
    private static AddressableManager _instance;
    public static AddressableManager Instance => _instance;

    private Dictionary<string, AsyncOperationHandle> _handlesDic = new();
    private Dictionary<string, object> _loadingTasks = new();

    // 게임이 시작될 때 자동 호출
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (_instance == null)
        {
            GameObject go = new("AddressableManager (Auto)");
            _instance = go.AddComponent<AddressableManager>();
            DontDestroyOnLoad(go);
        }
    }

    void Awake()
    {
        // 수동으로 배치했을 경우를 대비한 중복 체크
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private async void LoadAtlasAsync(string name, string path, Action<SpriteAtlas> completeAction)
    {
        try
        {
            SpriteAtlas atlas = await LoadAssetAsync<SpriteAtlas>(path);
            if (atlas == null)
                return;

            completeAction.Invoke(atlas);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            Debug.LogError(name + "  " + path + " 로딩 실패");
        }
    }



    public async UniTask<T> LoadAssetAsync<T>(string path) where T : UnityEngine.Object
    {
        // 1. 이미 로드된 핸들이 있는지 확인 (중복 로드 방지)
        if (_handlesDic.TryGetValue(path, out AsyncOperationHandle handle))
        {
            if (handle.IsValid())
            {
                return handle.Result as T;
            }
            else
            {
                _handlesDic.Remove(path); // 유효하지 않으면 제거
            }
        }

        // 2. 중복 로딩 방지
        if (_loadingTasks.TryGetValue(path, out var taskObj))
            {
                DebugUtils.LogError($"[AddressableManager] {path}는 이미 로딩 중이므로 기존 작업을 대기합니다.");
                return await (UniTask<T>)taskObj;
            }

        AsyncOperationHandle<T> newHandle = default;
        try
        {
            // 어드레서블 로드 시작
            newHandle = Addressables.LoadAssetAsync<T>(path);
            _loadingTasks.Add(path, newHandle.ToUniTask()); // 로딩 중

            // 로드 완료 대기
            T result = await newHandle.ToUniTask();

            // 3. 로드 성공 확인
            if (newHandle.Status == AsyncOperationStatus.Succeeded && result != null)
            {
                // 핸들 저장
                _handlesDic.Add(path, newHandle);
                return result;
            }
            else
            {
                // 성공 상태가 아니라면 즉시 해제
                Addressables.Release(newHandle);
                DebugUtils.LogError($"[AddressableManager] 로드 실패 (Status: {newHandle.Status}): {path}");
                return null;
            }
        }
        catch (Exception e)
        {
            DebugUtils.LogError($"[AddressableManager] 어드레서블 예외 발생: {path}\n에러 메시지: {e.Message}");
            if (newHandle.IsValid()) Addressables.Release(newHandle);
            return null;
        }
        finally
        {
            _loadingTasks.Remove(path); // 로딩 완료
        }
    }

    /// <summary>
    /// 특정 에셋의 핸들을 해제합니다. (메모리 해제)
    /// </summary>
    public void ReleaseAsset(string key)
    {
        if (_handlesDic.TryGetValue(key, out AsyncOperationHandle handle))
        {
            Addressables.Release(handle); // 실제 메모리 해제
            _handlesDic.Remove(key);      // 목록에서 제거
            Debug.Log($"[AddressableManager] 리소스 해제 완료: {key}");
        }
    }

    /// <summary>
    /// 게임 종료 시 모든 핸들 해제
    /// </summary>
    private void OnDestroy()
    {
        foreach (var handle in _handlesDic.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        _handlesDic.Clear();
    }
}