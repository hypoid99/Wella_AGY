using System;
using System.IO;
using System.Text.Json;

namespace Wella.Common
{
    public class StorageManager
    {
        public StorageManager()
        {
            AppConfig.EnsureDataDirectoryExists();
        }

        /// <summary>
        /// 객체 데이터를 JSON 문자열 포맷으로 변환하여 지정된 텍스트 파일 경로에 덮어씁니다. (안전 쓰기 메커니즘 포함)
        /// </summary>
        public void SaveToFile<T>(string filePath, T data)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(data, options);

                // 안전 저장: 백업 파일 생성 후 쓰기
                string tempPath = filePath + ".tmp";
                string backupPath = filePath + ".bak";

                File.WriteAllText(tempPath, jsonString);

                if (File.Exists(filePath))
                {
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                    File.Move(filePath, backupPath);
                }

                File.Move(tempPath, filePath);

                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StorageManager Save Error] {ex.Message}");
            }
        }

        /// <summary>
        /// 지정된 텍스트 파일로부터 JSON 문자열을 읽고 구조화된 데이터 객체로 변환하여 로드합니다.
        /// </summary>
        public T LoadFromFile<T>(string filePath, T defaultValue)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return defaultValue;
                }

                string jsonString = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(jsonString))
                {
                    return defaultValue;
                }

                T result = JsonSerializer.Deserialize<T>(jsonString);
                return result != null ? result : defaultValue;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[StorageManager Load Error] {ex.Message}");
                return defaultValue;
            }
        }
    }
}
