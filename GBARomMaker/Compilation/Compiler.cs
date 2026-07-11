using GBARomMaker.Rom.Operations;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace GBARomMaker.Compilation;

public class Compiler {
	public Operation GetOperationForLine(string line) {
		line = line.Split('@', 2)[0].Trim();
		string[] tokens = Regex
			.Matches(line, @"[A-Za-z_][A-Za-z0-9_]*|0x[0-9A-Fa-f]+|\d+|[^\s]")
			.Select(match => match.Value)
			.ToArray();
		var operation = tokens[0];
		switch (operation.ToLower()) {
			case "ldr": {
				var dest = tokens[1];
				var destinationRegister = int.Parse(dest[1].ToString());
				var seperator = tokens[2];
				if (seperator != ",") throw new Exception("Expected a comma between arguments");
				var source = tokens[3];
				if (source == "=") {
					// This is actual a psudocommand for MOV
					var value = tokens[4];
					return new Move {
						Register = destinationRegister,
						Value = value
					};

				}
				if (tokens.Length >= 4) throw new Exception("Command not recognized, too many arguments...");
				throw new NotImplementedException();
			}

		}
		
		throw new NotImplementedException();
	}
}
