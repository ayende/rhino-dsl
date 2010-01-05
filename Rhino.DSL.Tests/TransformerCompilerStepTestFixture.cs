using System;
using Boo.Lang.Compiler;
using Boo.Lang.Compiler.Ast;
using Xunit;

namespace Rhino.DSL.Tests
{
	public class TransformerCompilerStepTestFixture
	{
		[Fact]
		public void CtorRequiresNonNullArgument()
		{
            Assert.Throws<ArgumentNullException>(()=>new TransformerCompilerStep(null));
		}

		[Fact]
		public void CtorRequiresAtLeastOneTransformer()
		{
            Assert.Throws<ArgumentException>(() => new TransformerCompilerStep(new DepthFirstTransformer[] { }));
        }

		[Fact]
		public void TransformerIsAppliedToCompileUnit()
		{
			StubTransformer transformer = new StubTransformer();

			TransformerCompilerStep transformerStep = new TransformerCompilerStep(transformer);

			CompileUnit unit = new CompileUnit();

			BooCompiler compiler = new BooCompiler();
			compiler.Parameters.Pipeline = new CompilerPipeline();
			compiler.Parameters.Pipeline.Insert(0, transformerStep);
			compiler.Run(unit);

			Assert.True(transformer.CompileUnitVisited);
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
