using System;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;

namespace Rhino.DSL
{
	///<summary>
	/// Allow to use underscore separated names, which will be translated to pascal case names.
	/// pascal_case -> PascalCase.
	/// All names that contains an underscores will go through this treatment.
	///</summary>
	/// <example>
	/// You can  enable this behavior using the following statement
	/// <code>
	/// compiler.Parameters.Pipeline
	///		.Replace(typeof (ProcessMethodBodiesWithDuckTyping),
	/// 				 new ProcessMethodBodiesWithDslNamesAndDuckTyping());
	/// </code>
	/// </example>
	public class ProcessMethodBodiesWithDslNamesAndDuckTyping : ProcessMethodBodiesWithDuckTyping
	{
		/// <summary>
		/// Called when we encounter a reference expression
		/// </summary>
		/// <param name="node">The node.</param>
		public override void OnReferenceExpression(ReferenceExpression node)
		{
			if(node.Name.Contains("_"))
				SetNodeNameToPascalCase(node);
			base.OnReferenceExpression(node);
		}

		/// <summary>
		/// Called when we encounters a member reference expression
		/// </summary>
		/// <param name="node">The node.</param>
		public override void OnMemberReferenceExpression(MemberReferenceExpression node)
		{
			if (node.Name.Contains("_"))
				SetNodeNameToPascalCase(node);
			base.OnMemberReferenceExpression(node);
		}

		/// <summary>
		/// Sets the node name to pascal case.
		/// </summary>
		/// <param name="node">The node.</param>
		private static void SetNodeNameToPascalCase(ReferenceExpression node)
		{
			string[] parts = node.Name.Split(new char[] { '_' },StringSplitOptions.RemoveEmptyEntries);
			StringBuilder name = new StringBuilder();
			foreach (var part in parts)
			{
				name.Append(char.ToUpperInvariant(part[0]))
					.Append(part.Substring(1));
			}
			node.Name = name.ToString();
		}
	}
}