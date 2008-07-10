using System;
using System.Collections.Generic;
using System.Text;
using Boo.Lang.Compiler.Ast;

namespace Rhino.DSL
{
	/// <summary>
	/// Transforms the contents of macro blocks specified in the ctor to parameters.
	/// <example>
	/// <code>
	/// mymacro:
	///		1
	///		"something"
	/// 
	/// is transformed into
	/// 
	/// mymacro(1, "something")
	/// </code>
	/// </example>
	/// </summary>
	public class BlockToArgumentsTransformer : DepthFirstTransformer
	{
		private readonly string[] methods;

		/// <summary>
		/// Creates an instance of BlockToArgumentsTransformer
		/// </summary>
		/// <param name="methods">Methods that should have blocks transformed into arguments.</param>
		public BlockToArgumentsTransformer(params string[] methods)
		{
			if (methods == null ||
				methods.Length == 0)
				throw new ArgumentNullException("methods");
			this.methods = methods;
		}

		/// <summary>
		/// Handles macro statements.
		/// </summary>
		/// <param name="node">The node.</param>
		public override void OnMacroStatement(MacroStatement node)
		{
			if (Array.Exists(methods,
							 delegate(string name) { return name.Equals(node.Name); }))
			{
				if (node.Block != null)
				{
					Expression[] expressions = GetExpressionsFromBlock(node.Block);
					foreach (Expression expression in expressions)
						node.Arguments.Add(expression);
					node.Block.Clear();
				}
			}
			base.OnMacroStatement(node);
		}

		private static Expression[] GetExpressionsFromBlock(Block block)
		{
			List<Expression> expressions = new List<Expression>(block.Statements.Count);
			foreach (Statement statement in block.Statements)
			{
				if (statement is ExpressionStatement)
					expressions.Add((statement as ExpressionStatement).Expression);
				else if (statement is MacroStatement)
				{
					MacroStatement macroStatement = statement as MacroStatement;
					if (macroStatement.Arguments.Count == 0 &&
						!macroStatement.Block.HasStatements)
					{
						// Assume it is a reference expression
						ReferenceExpression refExp = new ReferenceExpression(macroStatement.LexicalInfo);
						refExp.Name = macroStatement.Name;
						expressions.Add(refExp);
					}
					else
					{
						// Assume it is a MethodInvocation
						MethodInvocationExpression mie = new MethodInvocationExpression(macroStatement.LexicalInfo);
						mie.Target = new ReferenceExpression(macroStatement.LexicalInfo, macroStatement.Name);
						mie.Arguments = macroStatement.Arguments;

						if (macroStatement.Block.HasStatements)
						{
							// If the macro statement has a block,                      
							// transform it into a block expression and pass that as the last argument                     
							// to the method invocation.
							BlockExpression be = new BlockExpression(macroStatement.LexicalInfo);
							be.Body = macroStatement.Block.CloneNode();

							mie.Arguments.Add(be);
						}

						expressions.Add(mie);
					}
				}
				else
				{
					throw new InvalidOperationException(string.Format("Can not transform block with {0} into argument.",
																	  statement.GetType()));
				}
			}
			return expressions.ToArray();
		}
	}
}
