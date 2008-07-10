using Boo.Lang.Compiler.Ast;
using MbUnit.Framework;

namespace Rhino.DSL.Tests
{
	[TestFixture]
	public class BlockToArgumentsTransformerTestFixture
	{
		[Test]
		public void Expression_statement_is_transformed()
		{
			LiteralExpression exp = new StringLiteralExpression("arg1");
			var doStuffStatement = new ExpressionStatement(exp);

			var fixture = new MacroStatement(new LexicalInfo("test", 1, 1));
			fixture.Name = "DoStuff";
			fixture.Block = new Block();
			fixture.Block.Add(doStuffStatement);

			var transformer = new BlockToArgumentsTransformer("DoStuff");
			transformer.Visit(fixture);

			Assert.AreEqual(exp, fixture.Arguments[0]);
			Assert.IsFalse(fixture.Block.HasStatements, "MacroStatement block should be empty after transformation.");
		}

		[Test]
		public void MacroStatement_with_no_arguments_and_no_block_is_transformed_to_ReferenceExpression()
		{
			const string referenceName = "some_reference";

			var statementInBlock = new MacroStatement();
			statementInBlock.Name = referenceName;

			MacroStatement fixture = GetMacroStatement("DoStuff", statementInBlock);

			var transformer = new BlockToArgumentsTransformer("DoStuff");
			transformer.Visit(fixture);

			Assert.IsAssignableFrom(typeof (ReferenceExpression), fixture.Arguments[0]);
			Assert.AreEqual(referenceName, (fixture.Arguments[0] as ReferenceExpression).Name);
		}

		[Test]
		public void MacroStatement_with_block_is_transformed_to_MethodInvocationExpression()
		{
			const string methodInBlockName = "some_method";
			const string doStuff = "DoStuff";

			Expression argInBlockExpression = new StringLiteralExpression("argInBlock");
			Statement argInBlockStatement = new ExpressionStatement(argInBlockExpression);

			MacroStatement statementInBlock = GetMacroStatement(methodInBlockName, argInBlockStatement);

			MacroStatement doStuffStatement = GetMacroStatement(doStuff, statementInBlock);

			var transformer = new BlockToArgumentsTransformer(doStuff, methodInBlockName);
			transformer.Visit(doStuffStatement);

			var mie = doStuffStatement.Arguments[0] as MethodInvocationExpression;

			Assert.IsNotNull(mie, "Could not cast argument one of MacroStatement to MethodInvocationExpression.");
			Assert.AreEqual(methodInBlockName, (mie.Target as ReferenceExpression).Name);
			Assert.IsAssignableFrom(typeof (BlockExpression), mie.Arguments[0]);
		}

		[Test]
		public void MacroStatement_with_arguments_is_transformed_to_MethodInvocationExpression()
		{
			const string methodInBlockName = "some_method";

			var statementInBlock = new MacroStatement();
			statementInBlock.Name = methodInBlockName;
			statementInBlock.Arguments.Add(new StringLiteralExpression("arg1"));

			MacroStatement fixture = GetMacroStatement("DoStuff", statementInBlock);

			var transformer = new BlockToArgumentsTransformer("DoStuff");
			transformer.Visit(fixture);

			var mie = fixture.Arguments[0] as MethodInvocationExpression;

			Assert.IsNotNull(mie, "Could not cast argument one of MacroStatement to MethodInvocationExpression.");
			Assert.AreEqual(methodInBlockName, (mie.Target as ReferenceExpression).Name);
		}

		private MacroStatement GetMacroStatement(string name, params Statement[] blockStatements)
		{
			var fixture = new MacroStatement();
			fixture.Name = name;
			fixture.Block = new Block();
			foreach (Statement statement in blockStatements)
				fixture.Block.Add(statement);
			return fixture;
		}
	}
}