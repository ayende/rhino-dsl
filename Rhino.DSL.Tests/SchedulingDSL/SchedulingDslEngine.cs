namespace Rhino.DSL.Tests.SchedulingDSL
{
    public class SchedulingDslEngine : DslEngine
    {
        protected override void CustomizeCompiler(Boo.Lang.Compiler.BooCompiler compiler, Boo.Lang.Compiler.CompilerPipeline pipeline, System.Uri[] urls)
        {
            pipeline.Insert(1, new AnonymousBaseClassCompilerStep(typeof (BaseScheduler), "Prepare",
                                                                  "Rhino.DSL.Tests.SchedulingDSL"));
        }
    }
}