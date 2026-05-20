using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Table
{
    public abstract class TableReader
    {
        private string TablePath
        {
            get
            {
#if DEVELOP || MAIN
                return Path.Combine(Application.persistentDataPath, "TableData");
#else
                return "Assets/GameData/JSON";
#endif
            }
        }

        public abstract UniTask InitializeAsync(CancellationToken ct);

        protected async UniTask<List<T>> GetParsedTableData<T>(CancellationToken ct)
        {
            var fileName = Path.Combine(TablePath, $"{typeof(T).Name}.json");
            if (!File.Exists(fileName))
            {
                throw new Exception($"{fileName} 경로에 파일이 없습니다.");
            }

            return await UniTask.RunOnThreadPool(() =>
            {
                // 작업 시작 전 취소 확인
                ct.ThrowIfCancellationRequested();

                var jsonContent = File.ReadAllText(fileName, System.Text.Encoding.UTF8);
                if (jsonContent.Length == 0 || jsonContent == "[]")
                {
                    throw new Exception($"/T_{typeof(T)}.json의 테이블을 읽을 수 없습니다. 테이블 확인해주세요.");
                }
                
                // 부동 소수점 맞추기 
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new FlexibleFloatConvert());

                // 작업 도중 취소 확인
                ct.ThrowIfCancellationRequested();
                return JsonConvert.DeserializeObject<List<T>>(jsonContent, settings);
            }, cancellationToken: ct);
        }
    }

    public class FlexibleFloatConvert : JsonConverter
    {
        // 이 컨버터가 float, List<float>, List<List<float>> 등을 모두 처리하게 설정
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(float) ||
                   objectType == typeof(List<float>) ||
                   objectType == typeof(List<List<float>>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            // 1. 문자열로 감싸진 경우 처리 ("[0.3, 0.02]" 또는 "[[0.1], [0.2]]")
            if (token.Type == JTokenType.String)
            {
                string raw = token.ToString();
                
                if (!raw.Contains("[")) // 단일 float일 때 
                    return Convert.ChangeType(raw, objectType);
                
                // 문자열 안의 JSON 배열을 파싱할 때 
                // JsonConvert 대신 JToken을 써서 무한 루프를 방지.
                return JToken.Parse(raw).ToObject(objectType);
            }

            // 2. 이미 배열 형태인 경우 바로 파싱
            return token.ToObject(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            => serializer.Serialize(writer, value);
    }
}