using System;
using System.Collections.Generic;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Rhino.DSL
{
	/// <summary>
	/// Transforms a macro's block statements to arguments.
	/// </summary>
	/// <example>
	/// SomeMacro:
	///		GetInteger()
	///		GetString()
	/// 
	/// Translates to SomeMacro(GetInteger(), GetString())
	/// </example>
	public class MacroBlockToParametersTransformer : DepthFirstTransformer
	{
		private readonly string[] macrosToTransform;

		/// <summary>
		/// Creates an instance of <see cref="MacroBlockToParametersTransformer"/>.
		/// </summary>
		/// <param name="macrosToTransform">Names of the macros to transform.</param>
		public MacroBlockToParametersTransformer(params string[] macrosToTransform)
		{
			if (macrosToTransform == null)
				throw new ArgumentNullException("macrosToTransform");
			this.macrosToTransform = macrosToTransform;
		}

		/// <summary>
		/// Transforms a macro statement.
		/// </summary>
		/// <param name="node">The <see cref="MacroStatement"/> node.</param>
		public override void OnMacroStatement(MacroStatement node)
		{
			string macroName = Array.Find(macrosToTransform, delegate(string name)
			                              	{
			                              		return name.Equals(node.Name);
			                              	});
			if (!String.IsNullOrEmpty(macroName))
			{
				StatementTransformer st = new StatementTransformer(node);
				st.Visit(node);
			}
			else
			{
				base.OnMacroStatement(node);
			}
		}

		private class StatementTransformer : DepthFirstTransformer
		{
			private readonly MacroStatement statement;

			public StatementTransformer(MacroStatement statement)
			{
				if (statement == null)
					throw new ArgumentNullException("statement");
				this.statement = statement;
			}

			public override void OnBlock(Block node)
			{
				if (node.ParentNode == statement)
				{
					foreach (Statement s in node.Statements)
					{
						MacroStatement macroStatement = s as MacroStatement;
						if (macroStatement != null)
						{
							MethodInvocationExpression mie = new MethodInvocationExpression(s.LexicalInfo);
							mie.Target = new ReferenceExpression(macroStatement.Name);
							foreach (Expression arg in macroStatement.Arguments)
							{
								mie.Arguments.Add(arg);
							}

							statement.Arguments.Add(mie);
						}

					}

					RemoveCurrentNode();
				}
				else
				{
					base.OnBlock(node);
				}
			}

			public override void OnExpressionStatement(ExpressionStatement node)
			{
				if (node.ParentNode == statement)
				{
					statement.Arguments.Add(node.Expression);
					RemoveCurrentNode();
				}
				base.OnExpressionStatement(node);
			}
		}
	}
}
