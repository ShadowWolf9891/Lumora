using UnityEngine;
[System.Serializable] public class DialogueLine { public string speaker; public string text; }
[System.Serializable] public class SceneData { public int id; public DialogueLine[] dialogues; }
[System.Serializable] public class ChapterData { public int id; public SceneData[] scenes; }
[System.Serializable] public class DialogueData { public ChapterData[] chapters; }