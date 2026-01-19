using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
		[typeof(double)] = s => (double.TryParse(s, out var v), v),
		[typeof(bool)] = s => (bool.TryParse(s, out var v), v),
	};

	public static bool IsValidUserInput(string userInput, out string validCommand)
    {
		validCommand = string.Empty;
		if (userInput == null || userInput == "" || userInput.IndexOf("/") != 0) return false;

		string parsedCommand = userInput.ToString().Substring(0, userInput.IndexOf(' '));

		if(!_validInputs.ContainsKey(parsedCommand)) return false;

		//No additonal command information
		if (_validInputs[parsedCommand].Length == 0)
		{
			validCommand = userInput;
			return true;
		}
		if (_validInputs[parsedCommand].Length == 1)
		{
			string subString = userInput.ToString().Substring(userInput.IndexOf(' ') + 1, -1).Trim();
			foreach (var parser in parsers)
			{
				var (success, value) = parser.Value(subString);
				if(success)
				{
					validCommand = userInput;
					return true;
				}
			}
		}

		for (int i = 0; i < _validInputs[parsedCommand].Length; i++)
		{
			string subString = "";
			if (i == 0)
			{
				subString = userInput.ToString().Substring(userInput.IndexOf(' ', 0) + 1);
			}
			else
			{
				subString = userInput.ToString().Substring(userInput.IndexOf(',', i) + 1);
			}
			foreach(var parser in parsers)
			{
				var (success, value) = parser.Value(subString);

				if(success) { continue; }
				else { return false; }
			}
		}

		return true;
    }
}