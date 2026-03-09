using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ConsoleWindow
{
	private static readonly string[][] _aliases = new string[][]
	{
		new string[]{ "goto","gt", "tp", "teleport" },
		new string[]{ "loadchapter","lc","load"},
		new string[]{ "playerspeed","ps", "speed" }, //Not Implemented
		new string[]{ "godmode", "gm", "god" },
		new string[]{ "deletesave", "delsave" },
		new string[]{"enablesaving", "enablesave"}
	};
	//Array of valid user inputs and the type of variable that needs to follow it.
	private static readonly Dictionary<string[], Type[]> _validInputs = new()
	{
		{_aliases[0], new Type[]{typeof(float), typeof(float), typeof(float) } },
		{ _aliases[1], new Type[]{typeof(int)}},
		{ _aliases[2], new Type[]{typeof(float)}},
		{_aliases[3], new Type[] {typeof(bool)} },
		{_aliases[4], new Type[] {} }
	};
	/// <summary>
	/// Call this when the user enters something into the console window and execute the event if it valid.
	/// Does not have variables like EventsToRaiseOnComplete, and will not be marked as complete itself.
	/// Returns an error message if invalid.
	/// </summary>
	/// <param name="userInput"></param>
	public static string DoConsoleCommand(string userInput)
	{
		string errorMessage = IsValidUserInput(userInput, out (string, object[]) validUserInput);
		if (errorMessage != "") { return errorMessage; }

		GameEventType e = (validUserInput.Item1) switch
		{
			"goto" => new TeleportPlayerEvent(validUserInput.Item1, new Vector3
			((float)validUserInput.Item2[0],
			(float)validUserInput.Item2[1],
			(float)validUserInput.Item2[2])),
			"loadchapter" => new LoadSceneEvent(validUserInput.Item1, (int)validUserInput.Item2[0]),
			"playerspeed" => default, //Do this later
			"godmode" => new GodModeEvent(validUserInput.Item1, (bool)validUserInput.Item2[0]),
			"deletesave" => new DeleteSaveEvent(validUserInput.Item1),
			"enablesaving" => new EnableSaveEvent(validUserInput.Item1),
			_ => default
		} ;

		RaiseConsoleCommand(e);
		return $"Successfully executed '{userInput}'.";
	}
	private static string IsValidUserInput(string userInput, out (string, object[]) validCommand)
    {
		validCommand = new("", null);
		if (userInput == null || userInput == "" || userInput.IndexOf("/") != 0) return $"'{userInput}' is null or empty.";

		string[] tokens = userInput.TrimStart('/').Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

		if(tokens == null || tokens.Length == 0 ) return $"'{userInput}' cannot be split into parameters correctly.";

		string command = tokens[0].ToLower();
		Type[] expectedTypes = null;
		foreach (var aliases in _validInputs)
		{
			if (aliases.Key.Contains(command))
			{
				command = aliases.Key[0];
				expectedTypes = aliases.Value;
				break;
			}
		}

		if(expectedTypes == null) return $"'{userInput}' parameters are null for command {command}.";
		if (tokens.Length - 1 != expectedTypes.Length) return $"'{userInput}' parameter amount is not equal to the expected number of parameters.";
			
		object[] args = new object[expectedTypes.Length];

		for ( int i = 0; i < expectedTypes.Length; i++ ) 
		{
			Type expectedType = expectedTypes[i];
			if (!parsers.TryGetValue(expectedType, out var parser)) return $"'{userInput}' does not have a parameter of {expectedType}.";

			var (success, value) = parser(tokens[i + 1]);
			if (!success) return $"'{value}' cannot be parsed as {expectedType}."; ;
			args[i] = value;
		}
		validCommand = (command, args);
		return "";
    }
	private static void RaiseConsoleCommand<T>(T consoleEvent) where T : GameEventType
	{
		EventManager.Instance.EventQueue.Enqueue(consoleEvent);
	}

	//Dictionary of valid types of objects the user can input and automatically parses them.
	private static Dictionary<Type, Func<string, (bool success, object value)>> parsers = new Dictionary<Type, Func<string, (bool success, object value)>>()
	{
		[typeof(int)] = s => (int.TryParse(s, out var v), v),
		[typeof(float)] = s => (float.TryParse(s, out var v), v),
		[typeof(bool)] = s => (bool.TryParse(s, out var v), v),
	};
}