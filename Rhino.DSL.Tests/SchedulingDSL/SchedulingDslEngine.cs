namespace Rhino.DSL.Tests.SchedulingDSL
{
    using Boo.Lang.Compiler;

    public class SchedulingDslEngine : DslEngine
    {
        protected override void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, string[] urls)
        {
            pipeline.Insert(1, new ImplicitBaseClassCompilerStep(typeof (BaseScheduler), "Prepare",
                                                                  "Rhino.DSL.Tests.SchedulingDSL"));
        }
    }
}
