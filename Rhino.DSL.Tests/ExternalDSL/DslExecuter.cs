using System.Collections;
namespace Rhino.DSL.Tests.ExternalDSL
{
	using System;
	using System.ComponentModel;
	using System.Reflection;

	public class DslExecuter
	{
		private readonly Hashtable parameters = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

		public void AddParameter(string name, object parameter)
		{
			parameters[name] = parameter;
		}

		public object Invoke(ActionExpression expression)
		{
			object obj = parameters[expression.Left];
			if (obj == null)
				throw new InvalidOperationException("Could not find parameter with name: " + expression.Left);
			MethodInfo method = obj.GetType().GetMethod(expression.Operator, BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
			if (method == null)
				throw new InvalidOperationException("Could not find method operator " + expression.Operator + " on " + expression.Left);
			ParameterInfo[] methodParams = method.GetParameters();
			if(methodParams.Length!=1)
				throw new InvalidOperationException(expression.Operator + " should accept a single parameter");
			object converted;
			Type paramType = methodParams[0].ParameterType;
			if(paramType.IsEnum)
			{
				converted = Enum.Parse(paramType, expression.Right,true);
			}
			else
			{
				converted = Convert.ChangeType(expression.Right, paramType, System.Globalization.CultureInfo.InvariantCulture);
			}
			return method.Invoke(obj, new object[] {converted});
		}
	}
}