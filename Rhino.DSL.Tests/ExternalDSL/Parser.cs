namespace Rhino.DSL.Tests.ExternalDSL
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	public class Parser
	{
		private static readonly string[] noise = {
		                                         	"a",
		                                         	"an",
		                                         	"the",
		                                         };

		public WhenThenClause Parse(string text)
		{
			text = RemoveNoiseWords(text);

			return SplitToWhenThenClause(text);
		}

		public WhenThenClause SplitToWhenThenClause(string text)
		{
			if (text.StartsWith("when", StringComparison.InvariantCultureIgnoreCase) == false)
				throw new InvalidOperationException("statement should start with a when");
			int thenIndex = text.IndexOf("then", StringComparison.InvariantCultureIgnoreCase);
			if (thenIndex == -1)
				throw new InvalidOperationException("statement should have a then");
			WhenThenClause clause = new WhenThenClause();
			string whenClause = text.Substring(4, thenIndex - 4).Trim();
			ParseClause(whenClause,
						delegate(string[] parts)
						{
	            			ActionExpression item = new ActionExpression();
	            			item.Left = parts[0];
	            			item.Operator = parts[1];
	            			item.Right = parts[2];
	            			clause.When.Add(item);
						});
			string thenClause = text.Substring(thenIndex + 4);
			ParseClause(thenClause,
						delegate(string[] parts)
						{
	            			ActionExpression item = new ActionExpression();
	            			item.Left = parts[0];
	            			item.Right= parts[1];
	            			item.Operator = parts[2];
	            			clause.Then.Add(item);
						});

			return clause;
		}


		private static void ParseClause(string clause,
										Action<string[]> action)
		{
			foreach (string subClauses in
				clause.Split(new string[] { "and" }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (subClauses.Trim().Length == 0)
					continue;

				string[] parts = subClauses.Split(new char[] {' ', '\t'}, StringSplitOptions.RemoveEmptyEntries);
				if (parts.Length != 3)
					throw new InvalidOperationException("A clause should have three parts [object] [operation] [value], found: " +
														subClauses);

				action(parts);
			}
		}

		public static string RemoveNoiseWords(string text)
		{
			Regex remove = new Regex(@"(^|\s+)(" +
			                         string.Join("|", noise)
			                         + @")($|\s+)",
			                         RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
			text = remove.Replace(text, " ");
			return text.Trim(' ','.','\t');
		}
	}

	public class WhenThenClause
	{
		public IList<ActionExpression> Then = new List<ActionExpression>();
		public IList<ActionExpression> When = new List<ActionExpression>();
	}

	public class ActionExpression
	{
		public ActionExpression()
		{

		}


		public ActionExpression(string left, string @operator, string right)
		{
			Left = left;
			Operator = @operator;
			Right = right;
		}

		public string Left;
		public string Operator;
		public string Right;
	}
}