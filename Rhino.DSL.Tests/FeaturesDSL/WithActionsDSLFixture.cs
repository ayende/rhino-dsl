namespace Rhino.DSL.Tests.FeaturesDSL
{
    using Boo.Lang.Compiler;
    using MbUnit.Framework;

    [TestFixture]
    public class WithActionsDSLFixture : BaseDslFixture<WithActionsDslEngine,WithAction>
    {
        [Test]
        public void CanUseGeneratedProperty()
        {
            string action = base.factory.Create<WithAction>(@"FeaturesDSL/HasAction.boo").Action;
            Assert.AreEqual("/action/with/name", action);
        }

        [Test]
        public void CanPassParametersViaCtor()
        {
            string name = base.factory.Create<WithAction>(@"FeaturesDSL/HasAction.boo", "test ctor").Name;
            Assert.AreEqual("test ctor", name);
        }

        [Test]
        public void CanUseSymbols()
        {
            WithAction action = factory.Create<WithAction>(@"FeaturesDSL/HasAction.boo", "test ctor");
            using(ConsoleRecorder recorder = new ConsoleRecorder())
            {
                action.Execute();
                recorder.AssertEquals("test\r\n");
            }
        }
    }

    public class WithActionsDslEngine : DslEngine
    {
        protected override void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, System.Uri[] urls)
        {
            pipeline.Insert(1,
                            new AnonymousBaseClassCompilerStep(typeof(WithAction), "Execute",
                                                               //default namespaces
                                                               "Rhino.DSL.Tests.FeaturesDSL"));
            pipeline.Insert(2, new UseSymbolsStep());
        }
    }

    public abstract class WithAction
    {
        protected WithAction(string name)
        {
            this.Name = name;
        }

        public string Name;

        public abstract string Action { get; }
        public abstract string Execute();
    }

    public class ActionMacro : GeneratePropertyMacro
    {
        public ActionMacro() : base("Action")
        {

        }
    }
}