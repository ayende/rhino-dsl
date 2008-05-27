using System.Threading;
using Boo.Lang.Compiler.Steps;

namespace Rhino.DSL.Tests.OrderDSL
{
    using Boo.Lang.Compiler;

    public class OrderActionsDslEngine : DslEngine
    {
        protected override void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, string[] urls)
        {
            pipeline.Insert(1,
                            new AnonymousBaseClassCompilerStep(typeof (BaseOrderActionsDSL), "Prepare",
                                                               //default namespaces
                                                               "Rhino.DSL.Tests.SchedulingDSL"));
        	pipeline.InsertBefore(typeof (ProcessMethodBodiesWithDuckTyping),
							 new UnderscorNamingConventionsToPascalCaseCompilerStep());
        }
    }
}