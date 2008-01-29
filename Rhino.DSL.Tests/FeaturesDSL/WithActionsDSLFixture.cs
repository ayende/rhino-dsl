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
    }

    public class WithActionsDslEngine : DslEngine
    {
        protected override void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, System.Uri[] urls)
        {
            pipeline.Insert(1,
                            new AnonymousBaseClassCompilerStep(typeof(WithAction), "Execute",
                                                               //default namespaces
                                                               "Rhino.DSL.Tests.FeaturesDSL"));
        }
    }

    public abstract class WithAction
    {
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