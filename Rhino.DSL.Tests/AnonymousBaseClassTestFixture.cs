namespace Rhino.DSL.Tests
{
	using System;
	using System.Reflection;
	using Boo.Lang.Compiler;
	using Xunit;

	public class AnonymousBaseClassTestFixture : BaseCompilerTestFixture
	{
		private Assembly assembly;

        public AnonymousBaseClassTestFixture()
		{
			assembly = Compile(@"AnonymousBaseClass.boo");			
		}

		[Fact]
		public void CanCreateAnonymousBaseClass()
		{
			Assert.NotNull(this.assembly);
		}

		[Fact]
		public void WillCreateTypeWithSameNameAsFile()
		{
			Assert.NotNull(
				assembly.GetType("AnonymousBaseClass")
				);	
		}

		[Fact]
		public void CanExecuteNewTypeAndGetCodeFromFile()
		{
			MyAnonymousBaseClass instance = (MyAnonymousBaseClass)assembly.CreateInstance("AnonymousBaseClass");
			instance.Run();

			Assert.Equal(
				"Hello from anonymous base class" + Environment.NewLine, 
				consoleOutput.GetStringBuilder().ToString());
		}

		protected override void AddCompilerSteps(BooCompiler compiler, string filename, CompilerPipeline pipeline)
		{
			compiler.Parameters.Pipeline.Insert(1, 
				new ImplicitBaseClassCompilerStep(typeof (MyAnonymousBaseClass), "Run"));
		}
	}

	public abstract class MyAnonymousBaseClass
	{
		public abstract void Run();
	}
}
