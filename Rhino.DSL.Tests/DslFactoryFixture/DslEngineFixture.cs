namespace Rhino.DSL.Tests.DslFactoryFixture
{
	using System;
	using System.IO;
	using System.Reflection;
	using Boo.Lang.Compiler;
	using MbUnit.Framework;
	using Boo.Lang.Compiler.Ast;

	[TestFixture]
	public class DslEngineFixture
	{
		private MyDslEngine engine;

		[SetUp]
		public void SetUp()
		{
			engine = new MyDslEngine();
		}

		[TearDown]
		public void TearDown()
		{
			foreach (string file in Directory.GetFiles(Path.GetTempPath(), "*.boocache"))
			{
				File.Delete(file);
			}
		}

		[Test]
		public void When_DSL_engine_is_asked_to_create_a_DSL_it_will_compile_and_return_the_compiler_context()
		{
			string path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DslFactoryFixture\MyDsl.boo"));
			CompilerContext compilerContext = engine.Compile(path);
			Assert.AreEqual(0, compilerContext.Errors.Count);
			Assert.IsNotNull(compilerContext.GeneratedAssembly);
		}

		[Test]
		[ExpectedException(typeof(CompilerError))]
		public void When_compilation_result_in_an_error_should_throw()
		{
			engine.Compile(Path.GetFullPath(@"somethingThatDoesNotExists.boo"));
		}

		[Test]
		public void Dsl_engine_can_take_parameters()
		{
			DslFactory _Factory = new DslFactory();
			_Factory.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			
			_Factory.Register<MyClassWithParams>(new DslEngineWithParameters());
			MyClassWithParams classWithParams = _Factory.Create<MyClassWithParams>("DslFactoryFixture\\ScriptWithParameter.boo");
			Assert.AreEqual("World", classWithParams.Hello("World"));
		}

		[Test]
		public void engine_reuses_first_compile()
		{
			string scriptPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DslFactoryFixture\engine_reuses_first_compile.boo"));
			File.WriteAllText(scriptPath, "print 0");

			string lastCachedFilePath = null;

			engine.CompilerContextCache.AssemblyLoaded += delegate (string filename, Assembly assembly, bool fromCache) 
				{ lastCachedFilePath = filename; };
			engine.Compile(scriptPath);
			Assert.IsFalse(lastCachedFilePath == null);

			lastCachedFilePath = null;

			engine.Compile(scriptPath);
			Assert.IsTrue(lastCachedFilePath == null);

		}

		[Test]
		public void engine_recompiles_on_script_change()
		{
			string scriptPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DslFactoryFixture\engine_recompiles_on_script_change.boo"));
			string lastCachedFilePath = null;

			engine.CompilerContextCache.AssemblyLoaded += delegate (string filename, Assembly assembly, bool fromCache) 
				{ lastCachedFilePath = filename; };
			
			File.WriteAllText(scriptPath, "print 1");
			engine.Compile(scriptPath);
			Assert.IsFalse(lastCachedFilePath == null);
			lastCachedFilePath = null;
			File.WriteAllText(scriptPath, "print 2");
			engine.Compile(scriptPath);
			Assert.IsFalse(lastCachedFilePath == null);
		}

		[Test]
		public void engine_recompiles_on_script_add()
		{
			string scriptPath1 = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DslFactoryFixture\engine_recompiles_on_script_add1.boo"));
			File.WriteAllText(scriptPath1, "def doit1():\n print 1");

			string scriptPath2 = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DslFactoryFixture\engine_recompiles_on_script_add2.boo"));
			File.WriteAllText(scriptPath2, "def doit2():\n print 2");

			string lastCachedFilePath = null;

			engine.CompilerContextCache.AssemblyLoaded += delegate (string filename, Assembly assembly, bool fromCache) 
				{ lastCachedFilePath = filename; };
			
			engine.Compile(scriptPath1);
			Assert.IsFalse(lastCachedFilePath == null);
			lastCachedFilePath = null;
			engine.Compile(scriptPath1, scriptPath2);
			Assert.IsFalse(lastCachedFilePath == null);
		}

		[Test]
		public void engine_recompiles_on_script_remove()
		{
			string scriptPath1 = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DslFactoryFixture\engine_recompiles_on_script_remove1.boo"));
			File.WriteAllText(scriptPath1, "def doit1():\n print 1");

			string scriptPath2 = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DslFactoryFixture\engine_recompiles_on_script_remove2.boo"));
			File.WriteAllText(scriptPath2, "def doit2():\n print 2");

			string lastCachedFilePath = null;

			engine.CompilerContextCache.AssemblyLoaded += delegate (string filename, Assembly assembly, bool fromCache) 
				{ lastCachedFilePath = filename; };

			engine.Compile(scriptPath1, scriptPath2);
			Assert.IsFalse(lastCachedFilePath == null);
			
			lastCachedFilePath = null;
			engine.Compile(scriptPath1);
			Assert.IsFalse(lastCachedFilePath == null);
		}
	}

	public class MyDslEngine : DslEngine
	{

	}

	public abstract class MyClassWithParams
	{
		public virtual string Hello(string input)
		{
			return input;
		}
	}

	public class DslEngineWithParameters : DslEngine
	{
		protected override void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, string[] urls)
		{
			ParameterDeclarationCollection parameters = new ParameterDeclarationCollection();
			ParameterDeclaration newParameterDeclaration =
				new ParameterDeclaration("input", new SimpleTypeReference("System.String"));
			parameters.Add(newParameterDeclaration);

			pipeline.Insert(1, new ImplicitBaseClassCompilerStep(typeof(MyClassWithParams),
				"Hello",
				parameters,
				"System"));
		}
	}
}
