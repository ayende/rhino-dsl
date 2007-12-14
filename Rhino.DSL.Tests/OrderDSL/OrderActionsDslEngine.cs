namespace Rhino.DSL.Tests.OrderDSL
{
    using Boo.Lang.Compiler;

    public class OrderActionsDslEngine : DslEngine
    {
        protected override void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, System.Uri[] urls)
        {
            pipeline.Insert(1,
                            new AnonymousBaseClassCompilerStep(typeof (BaseOrderActionsDSL), "Prepare",
                                                               //default namespaces
                                                               "Rhino.DSL.Tests.SchedulingDSL"));
        }
    }
}