namespace Rhino.DSL.Tests.SchedulingDSL
{
	using System;
	using System.Reflection;
	using Boo.Lang;
	using Boo.Lang.Compiler;
	using Boo.Lang.Compiler.Ast;
	using Boo.Lang.Compiler.MetaProgramming;
	using Boo.Lang.Compiler.Steps;
	using MbUnit.Framework;

	[TestFixture]
	public class SchedulingDSLFixture : BaseCompilerTestFixture
	{
		private Assembly assembly;

		public override void SetUp()
		{
			base.SetUp();
			assembly = Compile(@"SchedulingDSL\validateWebSiteUp.boo");
		}

		[Test]
		public void CanCompileFile()
		{
			Assert.IsNotNull(assembly);
		}

		[Test]
		public void CanCreateInstanceOfScheduler()
		{
			object instance = assembly.CreateInstance("validateWebSiteUp");
			Assert.IsNotNull(instance);
		}

		[Test]
		public void PreparingInstanceWillCauseValuesToFillFromDSL()
		{
			BaseScheduler instance = (BaseScheduler) assembly.CreateInstance("validateWebSiteUp");
			instance.Prepare();

			Assert.AreEqual(instance.TaskName, "warn if website is not alive");
			Assert.AreEqual(TimeSpan.FromMinutes(3), instance.Repetition);
			Assert.AreEqual(new DateTime(2000, 1, 1), instance.StartingTime);
		}

		[Test]
		public void WhenClause_WillRunCodeFromDSL()
		{
			BaseScheduler instance = (BaseScheduler) assembly.CreateInstance("validateWebSiteUp");
			instance.Prepare();

			WebSite.aliveValue = true; // will cause when to return false

			instance.Run();

			Assert.IsFalse(instance.ActionExecuted);
			Assert.AreEqual("http://example.org", WebSite.requestedUrl);
		}

		[Test]
		public void WhenClause_WhenReturnsTrue_WillCallAction()
		{
			BaseScheduler instance = (BaseScheduler) assembly.CreateInstance("validateWebSiteUp");
			instance.Prepare();

			WebSite.aliveValue = false; // will cause when to return true

			instance.Run();

			Assert.IsTrue(instance.ActionExecuted);

			Assert.AreEqual("admin@example.org", instance.WhoToNotify);
			Assert.AreEqual("server down!", instance.NotifyMessage);
		}

		protected override void AddCompilerSteps(BooCompiler compiler, string filename, CompilerPipeline pipeline)
		{
			pipeline.Insert(1,
			                new AnonymousBaseClassCompilerStep(typeof (BaseScheduler), "Prepare",
			                                                   //default namespaces
			                                                   "Rhino.DSL.Tests.SchedulingDSL"));
		}
	}
}