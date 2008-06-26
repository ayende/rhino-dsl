namespace Rhino.DSL.Tests.FeaturesDSL
{
    using Boo.Lang.Compiler;
    using MbUnit.Framework;

    [TestFixture]
    public class WithActionsDSLFixture : BaseDslFixture<WithActionsDslEngine,WithAction>
    {
		[Test]
		public void UsingWrongCase()
		{
			WithAction[] actions = factory.CreateAll<WithAction>(@"FeaturesDSL/hasAction.boo");
			Assert.AreEqual(1, actions.Length);
		}

        [Test]
        public void CanTryToCreateNonExistingScript()
        {
            WithAction action = factory.TryCreate<WithAction>(@"FeaturesDSL/DoesNotExists.boo");
            Assert.IsNull(action);
        }

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
        protected override void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, string[] urls)
        {
            pipeline.Insert(1,
                            new ImplicitBaseClassCompilerStep(typeof(WithAction), "Execute",
                                                               //default namespaces
                                                               "Rhino.DSL.Tests.FeaturesDSL"));
            pipeline.Insert(2, new UseSymbolsStep());
        }
    }

    public abstract class WithAction
    {
        protected WithAction()
        {
            
        }

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
