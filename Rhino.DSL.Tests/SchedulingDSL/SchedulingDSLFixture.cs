namespace Rhino.DSL.Tests.SchedulingDSL
{
    using System;
    using MbUnit.Framework;

    [TestFixture]
    public class SchedulingDSLFixture : BaseDslFixture<SchedulingDslEngine, BaseScheduler>
	{

		[Test]
		public void CanCompileFile()
		{
            Assert.IsNotNull(factory.Create<BaseScheduler>(@"SchedulingDSL\validateWebSiteUp.boo"));
		}

		[Test]
		public void CanCreateInstanceOfScheduler()
		{
			object instance = factory.Create<BaseScheduler>(@"SchedulingDSL\validateWebSiteUp.boo");
			Assert.IsNotNull(instance);
		}

		[Test]
		public void PreparingInstanceWillCauseValuesToFillFromDSL()
		{
            BaseScheduler instance = factory.Create<BaseScheduler>(@"SchedulingDSL\validateWebSiteUp.boo");
			instance.Prepare();

			Assert.AreEqual(instance.TaskName, "warn if website is not alive");
			Assert.AreEqual(TimeSpan.FromMinutes(3), instance.Repetition);
			Assert.AreEqual(new DateTime(2000, 1, 1), instance.StartingTime);
		}

		[Test]
		public void WhenClause_WillRunCodeFromDSL()
		{
            BaseScheduler instance = factory.Create<BaseScheduler>(@"SchedulingDSL\validateWebSiteUp.boo");
			instance.Prepare();

			WebSite.aliveValue = true; // will cause when to return false

			instance.Run();

			Assert.IsFalse(instance.ActionExecuted);
			Assert.AreEqual("http://example.org", WebSite.requestedUrl);
		}

		[Test]
		public void WhenClause_WhenReturnsTrue_WillCallAction()
		{
			BaseScheduler instance = factory.Create<BaseScheduler>(@"SchedulingDSL\validateWebSiteUp.boo");
			instance.Prepare();

			WebSite.aliveValue = false; // will cause when to return true

			instance.Run();

			Assert.IsTrue(instance.ActionExecuted);

			Assert.AreEqual("admin@example.org", instance.WhoToNotify);
			Assert.AreEqual("server down!", instance.NotifyMessage);
		}
	}
}