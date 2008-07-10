using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Boo.Lang.Compiler;
using MbUnit.Framework;

namespace Rhino.DSL.Tests
{
	[TestFixture]
	public class MethodSubstitutionBaseClassTestFixture : BaseCompilerTestFixture
	{
		private Assembly assembly;

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			assembly = Compile(@"MethodSubstitutionBaseClass.boo");
		}

		[Test]
		public void CanCreateAnonymousBaseClass()
		{
			Assert.IsNotNull(assembly);
		}

		[Test]
		public void WillCreateTypeWithSameNameAsFile()
		{
			Assert.IsNotNull(
				assembly.GetType("MethodSubstitutionBaseClass")
				);
		}

		[Test]
		public void CanExecuteNewTypeAndGetCodeFromFile()
		{
			MyMethodSubstitutionBaseClass instance = (MyMethodSubstitutionBaseClass)assembly.CreateInstance("MethodSubstitutionBaseClass");
			instance.SomeAbstractMethod();
			instance.SomeVirtualMethod();
			Assert.AreEqual("abstract" + Environment.NewLine + 
				"virtual" + Environment.NewLine,
				consoleOutput.GetStringBuilder().ToString());
		}

		[Test]
		public void GlobalAssignmentExpressionStatementIsFieldOnType()
		{
			MyMethodSubstitutionBaseClass instance = (MyMethodSubstitutionBaseClass)assembly.CreateInstance("MethodSubstitutionBaseClass");

			FieldInfo fi = instance.GetType().GetField("variableThatShouldBecomeAField", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.IsNotNull(fi);
		}

		protected override void AddCompilerSteps(BooCompiler compiler, string filename, CompilerPipeline pipeline)
		{
			compiler.Parameters.AddAssembly(typeof(Boo.Lang.Compiler.Ast.DepthFirstTransformer).Assembly);
			compiler.Parameters.Pipeline.Insert(1,
				new MethodSubstitutionBaseClassCompilerStep(typeof(MyMethodSubstitutionBaseClass), 
					"System", 
					"Boo.Lang.Compiler.Ast.DepthFirstTransformer"));
		}
	}

	public abstract class MyMethodSubstitutionBaseClass
	{
		public abstract void SomeAbstractMethod();
		public virtual void SomeVirtualMethod()
		{
			
		}
	}
}
