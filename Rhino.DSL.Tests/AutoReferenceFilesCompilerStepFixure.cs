namespace Rhino.DSL.Tests
{
	using System;
	using System.IO;
	using System.Reflection;
	using Boo.Lang.Compiler;
	using Boo.Lang.Compiler.IO;
	using Boo.Lang.Compiler.Pipelines;
	using Xunit;

	public class AutoReferenceFilesCompilerStepFixure : BaseCompilerTestFixture
	{
		[Fact]
		public void CanAddFileReference()
		{
			Assembly asm = Compile(@"hasReferenceToAnotherFile.boo", CompilerOutputType.ConsoleApplication);

			asm.EntryPoint.Invoke(null, new object[1] { null });

			Assert.Contains("From second file", consoleOutput.GetStringBuilder().ToString());
		}

		[Fact]
		public void CanAddImportsFromAnotherFile()
		{

			Assembly asm = Compile(@"hasReferenceToAnotherFile.boo", CompilerOutputType.ConsoleApplication);

			asm.EntryPoint.Invoke(null, new object[1] { null });

            Assert.Contains("Marshal", consoleOutput.GetStringBuilder().ToString());
	
		}

		protected override void AddCompilerSteps(BooCompiler compiler, string filename, CompilerPipeline pipeline)
		{
			pipeline.Insert(1, new AutoReferenceFilesCompilerStep(Path.GetDirectoryName(filename)));
		}
	}
}