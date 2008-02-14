namespace Rhino.DSL.Tests.DslFactoryFixture
{
    using System;
    using System.IO;
    using Boo.Lang.Compiler;
    using MbUnit.Framework;

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
    }

    public class MyDslEngine : DslEngine
    {
        
    }
}