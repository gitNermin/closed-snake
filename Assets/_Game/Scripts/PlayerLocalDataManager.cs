using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Game
{
    public static class PlayerLocalDataManager
    {
        public static void SaveData<T>(T data)
        {
            string json = JsonUtility.ToJson(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            byte[] base64Bytes = Encoding.UTF8.GetBytes(Convert.ToBase64String(bytes));
            var filepath = $"{Application.persistentDataPath}/playerLocalData/{typeof(T).Name}.dat";
            if (!Directory.Exists($"{Application.persistentDataPath}/playerLocalData/"))
            {
                Directory.CreateDirectory($"{Application.persistentDataPath}/playerLocalData/");
            }
            File.WriteAllBytes(filepath, base64Bytes);
        }

        public static T LoadData<T>()
        {
            var filepath = $"{Application.persistentDataPath}/playerLocalData/{typeof(T).Name}.dat";
            
            if(!File.Exists(filepath))
                return default;
            
            string base64String = File.ReadAllText(filepath);
            string json = Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
            return  JsonUtility.FromJson<T>(json);
        }

        public static void DeleteData<T>()
        {
            var filepath = $"{Application.persistentDataPath}/playerLocalData/{typeof(T).Name}.dat";
            if(File.Exists(filepath))
                File.Delete(filepath);
        }
    }
}