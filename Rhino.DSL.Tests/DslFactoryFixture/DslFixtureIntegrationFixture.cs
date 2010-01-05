namespace Rhino.DSL.Tests.DslFactoryFixture
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Boo.Lang.Compiler;
    using Xunit;

    public class DslFixtureIntegrationFixture
    {
        private readonly string path =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DslFactoryFixture\Integration.boo");

        [Fact]
        public void When_file_is_changed_will_automatically_get_new_version()
        {
            DslFactory factory = new DslFactory();
            factory.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            factory.Register<DemoDslBase>(new DemoDslEngine());
            File.WriteAllText(path, "log 'test'");
            
            DemoDslBase demo = factory.Create<DemoDslBase>(path);
            demo.Execute();
            Assert.Equal("test", demo.Messages[0]);

            File.WriteAllText(path,"log 'changed'");
        	for (int i = 0; i < 5; i++)
        	{
				Thread.Sleep(200);//let it time to refresh
				demo = factory.Create<DemoDslBase>(path);
				demo.Execute();
				if(demo.Messages[0]=="changed")
					break;
        	}
			Assert.Equal("changed", demo.Messages[0]);
        }
    }

    public abstract class DemoDslBase
    {
        public List<string> Messages = new List<string>();
        public abstract void Execute();

        public void log(string msg)
        {
            Messages.Add(msg);
        }
    }

    public class DemoDslEngine : DslEngine
    {
        protected override void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, string[] urls)
        {
            pipeline.Insert(1, new ImplicitBaseClassCompilerStep(typeof (DemoDslBase), "Execute"));
        }
    }
}
