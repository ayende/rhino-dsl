namespace Rhino.DSL.Tests.DslFactoryFixture
{
    using System;
    using System.IO;
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

            pipeline.Insert(1, new AnonymousBaseClassCompilerStep(typeof(MyClassWithParams),
                "Hello",
                parameters,
                "System"));
        }
    }
}
