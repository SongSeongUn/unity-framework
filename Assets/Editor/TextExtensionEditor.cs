using Common;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Editor
{
    [CustomEditor((typeof(TextExtension)))]
    public class TextExtensionEditor : UnityEditor.Editor
    {
        private void CheckAndFixComponent(TextExtension script)
        {
            if (script.type == TextExtension.TextType.Legacy && script.GetComponent<Text>() == null)
                UpdateComponents(script);
            else if (script.type == TextExtension.TextType.TextMeshPro && script.GetComponent<TextMeshProUGUI>() == null)
                UpdateComponents(script);
        }
        
        private void OnEnable()
        {
            TextExtension script = (TextExtension)target;
            if (script != null) CheckAndFixComponent(script);
        }
        
        public override void OnInspectorGUI()
        {
            TextExtension script = (TextExtension)target;

            // 1. 텍스트 타입 선택 (Radio Button 스타일)
            EditorGUILayout.LabelField("Select Text Component Type", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            script.type = (TextExtension.TextType)EditorGUILayout.EnumPopup("Text Type", script.type);

            if (EditorGUI.EndChangeCheck())
            {
                UpdateComponents(script);
            }

            // 2. 테이블 번호 입력
            EditorGUILayout.Space();
            script.tableNo = EditorGUILayout.IntField("Table Index", script.tableNo);

            // 3. 색상
            Color activeColor = EditorGUILayout.ColorField("ActiveColor", script.ActiveColor);
            Color inActiveColor = EditorGUILayout.ColorField("InActiveColor", script.InActiveColor);
            
            if (activeColor != script.ActiveColor)
            {
                script.ActiveColor = activeColor;
                script.SetColor(activeColor);
            }

            if (inActiveColor != script.InActiveColor)
            {
                script.InActiveColor = inActiveColor;
                script.SetColor(inActiveColor);
            }
      
            if (GUI.changed)
            {
                EditorUtility.SetDirty(script);
                script.RefreshText();
            }
        }

        private void UpdateComponents(TextExtension script)
        {
            // 컴포넌트 정리 로직
            if (script.type == TextExtension.TextType.Legacy)
            {
                if (script.IsTMP) script.RemoveTMPComponent();
            
                if (!script.IsText) script.gameObject.AddComponent<Text>();
            }
            else
            {
                if (script.IsText) script.RemoveTextComponent(); 
            
                if (!script.IsTMP) script.gameObject.AddComponent<TextMeshProUGUI>();
            }
        
            script.InitComponents();
            script.RefreshText();
        }
        
        [MenuItem("GameObject/UI (Canvas)/TextExtension - Text확장", false, 10)]
        static void CreateCustomText(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("TextExtension");
            go.AddComponent<RectTransform>();
            var textExtension = go.AddComponent<TextExtension>(); // 여기서 Reset()이 호출되며 TMP가 자동으로 붙음
            textExtension.AutoAddComponent();
    
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create Localized Text");
            Selection.activeObject = go;
        }
    }
}