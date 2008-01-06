namespace Rhino.DSL.Tests
{
    using System.Xml;
    using Boo.Lang.Compiler;
    using MbUnit.Framework;

    [TestFixture]
    public class AutoImportTextFixture : BaseCompilerTestFixture
    {
        [Test]
        public void Can_automatically_import_namespaces()
        {
            Compile("AutoImport.boo");
        }

        protected override void AddCompilerSteps(BooCompiler compiler, string filename, CompilerPipeline pipeline)
        {
            compiler.Parameters.References.Add(typeof(XmlDocument).Assembly);
            pipeline.Insert(1, new AutoImportCompilerStep("System.Xml"));
        }
    }
}