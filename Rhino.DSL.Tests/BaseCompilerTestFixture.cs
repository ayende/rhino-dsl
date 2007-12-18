namespace Rhino.DSL.Tests
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Reflection;
	using Boo.Lang.Compiler;
	using Boo.Lang.Compiler.IO;
	using Boo.Lang.Compiler.Pipelines;
	using MbUnit.Framework;

	public class BaseCompilerTestFixture
	{
		protected StringWriter consoleOutput;
		private TextWriter oldConsole;
	    private string oldCurrentDir;

	    [SetUp]
		public virtual void SetUp()
		{
		    oldCurrentDir = Environment.CurrentDirectory;
	        Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
			oldConsole = Console.Out;
			consoleOutput = new StringWriter();
			Console.SetOut(consoleOutput);
		}

		[TearDown]
		public void TearDown()
		{
		    Environment.CurrentDirectory = oldCurrentDir;
			Debug.WriteLine(consoleOutput.GetStringBuilder().ToString());
			Console.SetOut(oldConsole);
		}

		protected Assembly Compile(string filename)
		{
			return Compile(filename, CompilerOutputType.Library);
		}

		protected Assembly Compile(string filename, CompilerOutputType compilerOutputType)
		{
			BooCompiler compiler = new BooCompiler();
			compiler.Parameters.OutputType = compilerOutputType;
			compiler.Parameters.GenerateInMemory = true;
			compiler.Parameters.Pipeline = new CompileToMemory();
			AddCompilerSteps(compiler, filename, compiler.Parameters.Pipeline);
			compiler.Parameters.Input.Add(new FileInput(filename));

			CompilerContext run = compiler.Run();
			if (run.Errors.Count > 0)
				throw new CompilerError(run.Errors.ToString(true));
			return run.GeneratedAssembly;
		}

		protected virtual void AddCompilerSteps(BooCompiler compiler, string filename, CompilerPipeline pipeline)
		{
		}
	}
}