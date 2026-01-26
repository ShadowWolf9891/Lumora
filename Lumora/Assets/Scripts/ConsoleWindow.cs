using System;
using System.Collections.Generic;
using UnityEngine;

public static class ConsoleWindow
{
	//Array of valid user inputs and the type of variable that needs to follow it.
	private static readonly Dictionary<string, Type[]> _validInputs = new Dictionary<string, Type[]>
	{
		{"goto", new Type[]{typeof(float), typeof(float), typeof(float) } },
		{"gt", new Type[]{typeof(float), typeof(float), typeof(float) }},
		{ "loadchapter", new Type[]{typeof(int)}},
		{ "lc", new Type[]{typeof(int)}},
		{ "playerspeed", new Type[]{typeof(float)}},
		{ "ps", new Type[]{typeof(float)}},
		{ "godmode", new Type[] {} },
		{ "gm", new Type[] {} }

	};
	//Dictionary of valid types of objects the user can input and automatically parses them.
	private static Dictionary<Type, Func<string,(bool success, object value)>> parsers = new Dictionary<Type, Func<string, (bool success, object value)>>()
	{
		[typeof(int)] = s => (int.TryParse(s, out var v), v),
		[typeof(float)] = s => (float.TryParse(s, out var v), v),
		[typeof(bool)] = s => (bool.TryParse(s, out var v), v),
	};

	public static bool IsValidUserInput(string userInput, out (string, object[]) validCommand)
    {
		validCommand = new("", null);
		if (userInput == null || userInput == "" || userInput.IndexOf("/") != 0) return false;

		string[] tokens = userInput.TrimStart('/').Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

		if(tokens == null || tokens.Length == 0 ) return false;

		string command = tokens[0].ToLower();

		if (!_validInputs.TryGetValue(command, out var expectedTypes))
			return false;

		if (tokens.Length - 1 != expectedTypes.Length)
			return false;

		object[] args = new object[expectedTypes.Length];

		for ( int i = 0; i < expectedTypes.Length; i++ ) 
		{
			Type expectedType = expectedTypes[i];
			if (!parsers.TryGetValue(expectedType, out var parser)) return false;

			var (success, value) = parser(tokens[i + 1]);
			if (!success) return false;
			args[i] = value;
		}
		validCommand = (command, args);
		return true;
    }
}