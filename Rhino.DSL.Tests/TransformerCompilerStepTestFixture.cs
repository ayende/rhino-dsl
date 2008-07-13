using System;
using System.Collections.Generic;
using System.Text;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using MbUnit.Framework;

namespace Rhino.DSL.Tests
{
	[TestFixture]
	public class TransformerCompilerStepTestFixture
	{
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void CtorRequiresNonNullArgument()
		{
			new TransformerCompilerStep(null);
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CtorRequiresAtLeastOneTransformer()
		{
			new TransformerCompilerStep(new DepthFirstTransformer[] {});
		}

		[Test]
		public void TransformerIsAppliedToCompileUnit()
		{
			StubTransformer transformer = new StubTransformer();

			TransformerCompilerStep transformerStep = new TransformerCompilerStep(transformer);

			CompileUnit unit = new CompileUnit();

			BooCompiler compiler = new BooCompiler();
			compiler.Parameters.Pipeline = new CompilerPipeline();
			compiler.Parameters.Pipeline.Insert(0, transformerStep);
			compiler.Run(unit);

			Assert.IsTrue(transformer.CompileUnitVisited);
		}

		private class StubTransformer: DepthFirstTransformer
		{
			private bool compileUnitVisited = false;

			public bool CompileUnitVisited
			{
				get { return compileUnitVisited; }
			}

			public override void OnCompileUnit(CompileUnit node)
			{
				compileUnitVisited = true;
				base.OnCompileUnit(node);
			}
		}
	}
}
