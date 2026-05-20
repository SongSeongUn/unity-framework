using System;
using Managers;
using UnityEngine;

namespace UI
{
    public enum UIWindowType
    {
        Page,
        Popup
    }
    
    public abstract class UIBase : MonoBehaviour
    {
        public UIWindowType UIType;
        private Canvas _canvas;
        public Canvas Canvas 
        {
            get
            {
                if(_canvas == null) TryGetComponent(out _canvas);
                return _canvas;
            }  
        }
        
        public int SortOrder { get { return _canvas.sortingOrder; } set { _canvas.sortingOrder = value; }}
        
        public abstract void Refresh();

        public virtual void Open() => gameObject.SetActive(true);
        public virtual void Close() => gameObject.SetActive(false);

        protected virtual void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("UI");;
        }

        public virtual void Init()
        {
            if (UIManager.Instance == null) return;
            
            UIManager.Instance.SetUIParentRoot(this);
            
            if(_canvas == null) TryGetComponent(out _canvas);
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = UIManager.Instance.UICamera;
        }

        protected virtual void OnDestroy()
        {
            string cleanName = gameObject.name.Replace(" (Clone)", "").Replace("(Clone)", "").Trim();
            string assetPath = $"UI/{cleanName}.prefab";
            
            // 로딩 은 씬 이동 이후 다시 사용하기 때문에 메모리 해제 X
            if (!cleanName.Contains("Loading"))
                AddressableManager.Instance.ReleaseAsset(assetPath);
        }
    }

    public class UIPage : UIBase
    {
        public override void Refresh()
        {
            throw new System.NotImplementedException();
        }
    }

    public class UIPopup : UIBase
    {
        public override void Refresh()
        {
            throw new System.NotImplementedException();
        }
    }
}