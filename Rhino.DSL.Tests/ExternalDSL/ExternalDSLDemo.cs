using System;
using System.Collections.Generic;
using System.Text;

namespace Rhino.DSL.Tests.ExternalDSL
{
	public class ExternalDSLDemo
	{
		private readonly WhenThenClause parsed;
		private readonly DslExecuter executer = new DslExecuter();

		public ExternalDSLDemo(string text)
		{
			parsed = new Parser().Parse(text);
		}

		public void AddParameter(string name, object parameter)
		{
			executer.AddParameter(name, parameter);
		}

		public void Execute()
		{
			bool result = true;
			foreach (ActionExpression expression in parsed.When)
			{
				bool clauseResult = (bool) this.executer.Invoke(expression);
				result &= clauseResult;
			}

			if(result)
			{
				foreach (ActionExpression expression in parsed.Then)
				{
					executer.Invoke(expression);
				}
			}
		}
	}
}
