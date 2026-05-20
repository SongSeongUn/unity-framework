using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Data;
using Newtonsoft.Json;
using System.Text;
using ExcelDataReader;
using UnityEngine;


namespace Editor
{
    public class TableParserToolWindow:OdinEditorWindow
    {
     [MenuItem("Tools/데이터 변환 툴 (Excel To Json And Class)")]
        public static void ShowWindow()
        {
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var window = GetWindow<TableParserToolWindow>("Table Converter");
            window.Show();
        }
        
        [Title("폴더 일괄 변환 (xlsx -> JSON & Generate Class)")]
        [InfoBox("지정한 폴더 내의 모든 xlsx 파일을 읽어 타입(숫자/문자/bool)에 맞게 Json과 Class로 적용 합니다.")]
        [LabelText("xlsx 경로")]
        [FolderPath]
        public string tableFolderPath = string.Empty; 
        public string jsonFolderPath = "Assets/GameData/JSON";

        [Button("🚀 모든 xlsx JSON & Class으로 적용", ButtonSizes.Large), GUIColor(0, 1, 0.5f)]
        public void ConvertAllCSVs()
        {
            if (!tableFolderPath.Contains("Table") || string.IsNullOrEmpty(tableFolderPath))
            {
                EditorUtility.DisplayDialog("에러", "xlsx 폴더 경로가 올바르지 않습니다.", "확인");
                return;
            }

            if (!Directory.Exists(jsonFolderPath)) Directory.CreateDirectory(jsonFolderPath);

            string[] files = Directory.GetFiles(tableFolderPath, "*.xlsx");
            int successCount = 0;

            foreach (string filePath in files)
            {
                if (filePath.Contains("Example")) continue;
                if (ConvertSingleFile(filePath)) successCount++;
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("변환 완료", $"{successCount}개의 파일이 JSON & Class로 적용 되었습니다.", "확인");
        }

        // ========================================================
        // 1. JSON 변환
        // ========================================================
        private bool ConvertSingleFile(string fullPath)
        {
            try
            {
                string tableName = "";
                List<Dictionary<string, object>> tableData = new List<Dictionary<string, object>>();
                using (var fs = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = ExcelReaderFactory.CreateReader(fs))
                    {
                        if (sr == null || sr.RowCount < 2) return false;

                        DataSet result = sr.AsDataSet(); // 전체 시트를 데이터셋으로 변환
                        DataTable table = result.Tables[0];    // 첫 번째 시트 선택
                        tableName = table.TableName;
                        
                        var columns =  table.Rows[0];
                        var types =  table.Rows[1];
                        
                        for (var i = 2; i < table.Rows.Count; i++)
                        {
                            var rowData = table.Rows[i];
                            Dictionary<string, object> values =  new Dictionary<string, object>();
                            for (var j = 0; j < rowData.ItemArray.Length; j++)
                            {
                                var value = ParseValue(types[j].ToString(),rowData.ItemArray[j].ToString());
                                values.Add(columns.ItemArray[j].ToString(), value);
                            }
                            tableData.Add(values);
                        }

                        GenerateClassFile(table, tableName);
                    }
                }

                // JsonConvert가 object에 담긴 진짜 타입(int, float, string)을 보고 알아서 포맷팅해줍니다.
                string jsonResult = JsonConvert.SerializeObject(tableData, Formatting.Indented);

                string fileName = "T_" + tableName + ".json";
                string savePath = Path.Combine(jsonFolderPath, fileName);
                File.WriteAllText(savePath, jsonResult, Encoding.UTF8);

                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[생성 실패] {fullPath} : {e.Message}");
                return false;
            }
        }
        
        private object ParseValue(string type, string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            // switch (type)
            // {
            //     case "int":
            //         return int.Parse(input);
            //     case "float":
            //         return float.Parse(input);
            //     case "bool":
            //         return bool.Parse(input);
            //     case "List<int>":
            //         return input;
            //     case "List<List<int>>":
            //         {
            //             
            //         }
            //         return input
            //     case 
            //     default:
            //         return input;
            // }
            //
            // if (type == "int") return int.Parse(input);
            //
            // if (type == "float") return float.Parse(input);
            //
            // if (type == "bool") return bool.Parse(input);
            //
            // if (type == "List<string>")
            // {
            //     
            // }
            
            
            //if (type == "string") return input;
            
            

            // 1. Bool 체크 (true / false)
            if (bool.TryParse(input, out bool bValue)) return bValue;
            
            // 2. Int 체크 (정수)
            if (int.TryParse(input, out int iValue)) return iValue;
            
            // 3. Float 체크 (소수점) - 다국어 설정(,) 문제 방지를 위해 InvariantCulture 사용
            if (float.TryParse(input, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float fValue)) 
                return fValue;
            
            // 4. 배열 체크
            if (type.Contains("List<int>")) 
                return JsonConvert.DeserializeObject<List<decimal>>(input);
            
            if(type.Contains("List<List<int>>"))
                return JsonConvert.DeserializeObject<List<List<decimal>>>(input);
            
            if(type.Contains("List<float>"))
                return JsonConvert.DeserializeObject<List<decimal>>(input);
            
            if (type.Contains("List<List<float>>"))
                return JsonConvert.DeserializeObject<List<List<decimal>>>(input);

            // 4. 전부 아니면 그냥 문자열(String)로 반환
            return input;
        }

        private void GenerateClassFile(DataTable table, string className)
        {
            StringBuilder sb = new StringBuilder();

            // 1. 네임스페이스 및 선언부 작성
            sb.Append("using System.Collections.Generic;");
            sb.AppendLine("\n");
            sb.AppendLine("namespace Table.Models");
            sb.AppendLine("{");
            sb.AppendLine("    [System.Serializable]");
            sb.AppendLine($"    public class T_{className}");
            sb.AppendLine("    {");
            
            // 2. 컬럼명(0행)과 타입(1행)을 읽어서 필드 생성
            // 엑셀 시트 기준: 0번째 가로줄은 컬럼명(No, Name...), 1번째 가로줄은 타입(int, string...)
            DataRow headerRow = table.Rows[0];
            DataRow typeRow = table.Rows[1];

            for (int i = 0; i < table.Columns.Count; i++)
            {
                string columnName = headerRow[i].ToString();
                string typeName = typeRow[i].ToString();

                if (string.IsNullOrEmpty(columnName)) continue;

                // 예: public int No; 형태의 문자열 생성
                sb.AppendLine($"        public {typeName} {columnName};");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            // 3. 파일로 저장 (유니티 프로젝트의 Assets 폴더 내부)
            string savePath = $"Assets/Scripts/Table/Models/T_{className}.cs";
            File.WriteAllText(savePath, sb.ToString());

            Debug.Log($"{className}.cs 파일이 생성되었습니다!\n 경로 : {savePath}");
        }
    }
}