using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Boo.Lang.Compiler;
using Xunit;

namespace Rhino.DSL.Tests
{
	public class MethodSubstitutionBaseClassTestFixture : BaseCompilerTestFixture
	{
		private readonly Assembly assembly;

	    public MethodSubstitutionBaseClassTestFixture()
	    {
            assembly = Compile(@"MethodSubstitutionBaseClass.boo");	        
	    }

		[Fact]
		public void CanCreateAnonymousBaseClass()
		{
			Assert.NotNull(assembly);
		}

		[Fact]
		public void WillCreateTypeWithSameNameAsFile()
		{
			Assert.NotNull(
				assembly.GetType("MethodSubstitutionBaseClass")
				);
		}

		[Fact]
		public void CanExecuteNewTypeAndGetCodeFromFile()
		{
			MyMethodSubstitutionBaseClass instance = (MyMethodSubstitutionBaseClass)assembly.CreateInstance("MethodSubstitutionBaseClass");
			instance.SomeAbstractMethod();
			instance.SomeVirtualMethod();
			Assert.Equal("abstract" + Environment.NewLine + 
				"virtual" + Environment.NewLine,
				consoleOutput.GetStringBuilder().ToString());
		}

		[Fact]
		public void GlobalAssignmentExpressionStatementIsFieldOnType()
		{
			MyMethodSubstitutionBaseClass instance = (MyMethodSubstitutionBaseClass)assembly.CreateInstance("MethodSubstitutionBaseClass");

			FieldInfo fi = instance.GetType().GetField("variableThatShouldBecomeAField", BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.NotNull(fi);
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
