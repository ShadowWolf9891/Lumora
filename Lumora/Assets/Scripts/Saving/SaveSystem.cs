using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static bool HasSaved => File.Exists(GetPath());
    public static void Save(GameSaveData data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(GetPath(), json);
    }
    public static GameSaveData Load()
    {
        if(!File.Exists(GetPath())) return null;
        string json = File.ReadAllText(GetPath());
        return JsonConvert.DeserializeObject<GameSaveData>(json);
    }

    private static string GetPath()
    {
		return Path.Combine(Application.persistentDataPath, "save_01.json");
	}
}

public interface ISaveable
{
	void Save(GameSaveData data);
    void Load(GameSaveData data);
}

[Serializable]
public struct SerializableVector3
{
	public float x;
	public float y;
	public float z;

	public SerializableVector3(Vector3 v)
	{
		x = v.x;
		y = v.y;
		z = v.z;
	}

	public Vector3 ToVector3()
		=> new Vector3(x, y, z);
}