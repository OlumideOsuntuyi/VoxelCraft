using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

[System.Serializable]
public class DictionarySerializer<TKey, TValue>
{
    private Dictionary<TKey, TValue> dictionary;

    public DictionarySerializer(Dictionary<TKey, TValue> dictionary)
    {
        this.dictionary = dictionary;
    }

    public void SaveToFile(string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream fileStream = File.Create(filePath))
        {
            formatter.Serialize(fileStream, dictionary);
        }
    }

    public static Dictionary<TKey, TValue> LoadFromFile(string filePath)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        if (File.Exists(filePath))
        {
            using (FileStream fileStream = File.Open(filePath, FileMode.Open))
            {
                Debug.Log($"opened {typeof(Dictionary<TKey, TValue>)} file of size {fileStream.Length}");
                return (Dictionary<TKey, TValue>)formatter.Deserialize(fileStream);
            }
        }
        else
        {
            Debug.LogWarning("File not found. Creating a new dictionary.");
            return new Dictionary<TKey, TValue>();
        }
    }
}
