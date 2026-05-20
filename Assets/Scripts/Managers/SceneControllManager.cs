using System;
using Common;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;

namespace Managers
{
    public class SceneControllManager : Singleton<SceneControllManager>
    {
        public async UniTask LoadScene(string sceneName, Action finishEvent = null)
        {
            var handle = Addressables.LoadSceneAsync(sceneName);
            
            while (!handle.IsDone)
            {
                float progress = handle.PercentComplete;
                // 로딩바 UI 업데이트 로직
                await UniTask.Yield();
            }
            
            DebugUtils.Log($"{sceneName} 씬 로드 완료");
        }
    }
}