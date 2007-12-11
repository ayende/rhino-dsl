namespace Rhino.DSL.Tests.OrderDSL
{
	using System.Reflection;
	using Boo.Lang.Compiler;
	using MbUnit.Framework;

	[TestFixture]
	public class OrderDSLFixture : BaseCompilerTestFixture
	{
		[Test]
		public void CanCompile()
		{
			Assembly compile = Compile(@"OrderDSL\OrderBusinessRules.boo");
			Assert.IsNotNull(compile);
		}

		[Test]
		public void When_User_is_preferred_and_Order_total_cost_is_above_1000_should_apply_free_shipping_and_5_precent_discount()
		{
			BaseOrderActionsDSL instance = (BaseOrderActionsDSL)Compile(@"OrderDSL\OrderBusinessRules.boo").CreateInstance("OrderBusinessRules");
			instance.Order = new Order();
			instance.Order.TotalCost = 5000;
			instance.User = new User();
			instance.User.IsPreferred = true;
			instance.Prepare();
			instance.Execute();

			Assert.AreEqual(5m, instance.discountPrecentage);
			Assert.IsTrue(instance.shouldApplyFreeShipping);
			Assert.IsFalse(instance.shouldSuggestUpgradeToPreferred);
		}

		[Test]
		public void When_User_is_not_preferred_and_order_total_cost_above_100_should_apply_for_free_shipping_and_suggest_upgrade_to_preferred()
		{
			BaseOrderActionsDSL instance = (BaseOrderActionsDSL)Compile(@"OrderDSL\OrderBusinessRules.boo").CreateInstance("OrderBusinessRules");
			instance.Order = new Order();
			instance.Order.TotalCost = 5000;
			instance.User = new User();
			instance.User.IsPreferred = false;
			instance.Prepare();
			instance.Execute();

			Assert.AreEqual(0m, instance.discountPrecentage);
			Assert.IsTrue(instance.shouldApplyFreeShipping);
			Assert.IsTrue(instance.shouldSuggestUpgradeToPreferred);
		}

		[Test]
		public void When_User_is_not_preferred_and_order_total_cost_is_above_500_should_apply_for_free_shipping()
		{
			BaseOrderActionsDSL instance = (BaseOrderActionsDSL)Compile(@"OrderDSL\OrderBusinessRules.boo").CreateInstance("OrderBusinessRules");
			instance.Order = new Order();
			instance.Order.TotalCost = 600;
			instance.User = new User();
			instance.User.IsPreferred = false;
			instance.Prepare();
			instance.Execute();

			Assert.AreEqual(0m, instance.discountPrecentage);
			Assert.IsTrue(instance.shouldApplyFreeShipping);
			Assert.IsFalse(instance.shouldSuggestUpgradeToPreferred);
		}

		protected override void AddCompilerSteps(BooCompiler compiler, string filename, CompilerPipeline pipeline)
		{
			pipeline.Insert(1,
			                new AnonymousBaseClassCompilerStep(typeof (BaseOrderActionsDSL), "Prepare",
			                                                   //default namespaces
			                                                   "Rhino.DSL.Tests.SchedulingDSL"));
		}
	}
}