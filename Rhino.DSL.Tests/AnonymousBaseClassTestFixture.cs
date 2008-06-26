namespace Rhino.DSL.Tests
{
	using System;
	using System.Reflection;
	using Boo.Lang.Compiler;
	using MbUnit.Framework;

	[TestFixture]
	public class AnonymousBaseClassTestFixture : BaseCompilerTestFixture
	{
		private Assembly assembly;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			assembly = Compile(@"AnonymousBaseClass.boo");			
		}

		[Test]
		public void CanCreateAnonymousBaseClass()
		{
			Assert.IsNotNull(this.assembly);
		}

		[Test]
		public void WillCreateTypeWithSameNameAsFile()
		{
			Assert.IsNotNull(
				assembly.GetType("AnonymousBaseClass")
				);	
		}

		[Test]
		public void CanExecuteNewTypeAndGetCodeFromFile()
		{
			MyAnonymousBaseClass instance = (MyAnonymousBaseClass)assembly.CreateInstance("AnonymousBaseClass");
			instance.Run();

			Assert.AreEqual(
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
