using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Common
{
    [ExecuteAlways]
    public class TextExtension : MonoBehaviour
    {
        public enum TextType { Legacy, TextMeshPro }
    
        [HideInInspector] public TextType type = TextType.Legacy;
        public int tableNo = 0; // 테이블의 ID 번호

        private Text _text;
        private TextMeshProUGUI _tmPro;
        
        public bool IsText => _text != null;
        public bool IsTMP => _tmPro != null;
        
        [HideInInspector] public Color ActiveColor = Color.white;
        [HideInInspector] public Color InActiveColor = Color.black;
        
        private bool _isInit = false;
        private void Awake()
        {
            InitComponents();
        }

        public void InitComponents()
        {
            _= TryGetComponent(out _text);
            _= TryGetComponent(out _tmPro);
        }
        
        public void RefreshText()
        {
            // 실제 프로젝트의 데이터 테이블 매니저에서 텍스트를 가져오는 로직
            // string localizedText = DataTableManager.GetText(tableIndex);s
            string localizedText = $"Table Data {tableNo}"; // 임시 데이터

            if (tableNo == 0) return;
            
            if (type == TextType.Legacy)
            {
                if(TryGetComponent(out _text))
                    _text.text = localizedText;
            }
            else
            {
                if(TryGetComponent(out _tmPro))
                    _tmPro.text = localizedText;
            }
        }

        public void SetColor(Color color)
        {
            if (IsText) _text.color = color;
            else if (IsTMP) _tmPro.color = color;
            else DebugUtils.LogError($"Cannot set color for text type {type}");
        }

        private void Update()
        {
#if  UNITY_EDITOR
            // 에디터에서 값이 바뀌었을 때 즉시 반영되도록 호출 
            if (!Application.isPlaying) RefreshText();
#endif
        }
        
#if UNITY_EDITOR
        public void RemoveTextComponent()
        {
            _text = null;
            DestroyImmediate(gameObject.GetComponent<Text>());
            DebugUtils.Log("Removed Text component");
        }

        public void RemoveTMPComponent()
        {
            _tmPro = null;
            DestroyImmediate(gameObject.GetComponent<TextMeshProUGUI>());
            DebugUtils.Log("Removed TMP component");
        }
        
        public void AutoAddComponent()
        {
            if (type == TextType.Legacy)
            {
                if (!TryGetComponent(out Text _))
                {
                    _text = gameObject.AddComponent<Text>();
                    RemoveTMPComponent();
                }
            }
            else
            {
                if (!TryGetComponent(out TextMeshProUGUI _))
                {
                    _tmPro = gameObject.AddComponent<TextMeshProUGUI>();
                    RemoveTextComponent();
                }
            }
        }
#endif
    }
}