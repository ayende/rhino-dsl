namespace Rhino.DSL.Tests.OrderDSL
{
    using System.Reflection;
    using Boo.Lang.Compiler.Pipelines;
    using Xunit;

    public class OrderDSLFixture : BaseDslFixture<OrderActionsDslEngine, BaseOrderActionsDSL>
    {
        [Fact]
        public void CanCompile()
        {
            Assert.NotNull(factory.Create<BaseOrderActionsDSL>(@"OrderDSL\OrderBusinessRules.boo"));
        }

		[Fact]
		public void CanCompile_WithDifferentOperations()
		{
			BaseOrderActionsDSL dsl = factory.Create<BaseOrderActionsDSL>(@"OrderDSL\OrderBusinessRules_DifferentOperations.boo");
			Assert.NotNull(dsl);
		}


    	[Fact]
        public void
            When_User_is_preferred_and_Order_total_cost_is_above_1000_should_apply_free_shipping_and_5_precent_discount()
        {
            BaseOrderActionsDSL instance =
                factory.Create<BaseOrderActionsDSL>(@"OrderDSL\OrderBusinessRules.boo");
            instance.Order = new Order();
            instance.Order.TotalCost = 5000;
            instance.User = new User();
            instance.User.IsPreferred = true;
            instance.Prepare();
            instance.Execute();

            Assert.Equal(5m, instance.discountPrecentage);
            Assert.True(instance.shouldApplyFreeShipping);
            Assert.False(instance.shouldSuggestUpgradeToPreferred);
        }

        [Fact]
        public void
            When_User_is_not_preferred_and_order_total_cost_above_100_should_apply_for_free_shipping_and_suggest_upgrade_to_preferred
            ()
        {
            BaseOrderActionsDSL instance =
                factory.Create<BaseOrderActionsDSL>(@"OrderDSL\OrderBusinessRules.boo");
            instance.Order = new Order();
            instance.Order.TotalCost = 5000;
            instance.User = new User();
            instance.User.IsPreferred = false;
            instance.Prepare();
            instance.Execute();

            Assert.Equal(0m, instance.discountPrecentage);
            Assert.True(instance.shouldApplyFreeShipping);
            Assert.True(instance.shouldSuggestUpgradeToPreferred);
        }

        [Fact]
        public void When_User_is_not_preferred_and_order_total_cost_is_above_500_should_apply_for_free_shipping()
        {
            BaseOrderActionsDSL instance =
                factory.Create<BaseOrderActionsDSL>(@"OrderDSL\OrderBusinessRules.boo");
            instance.Order = new Order();
            instance.Order.TotalCost = 600;
            instance.User = new User();
            instance.User.IsPreferred = false;
            instance.Prepare();
            instance.Execute();

            Assert.Equal(0m, instance.discountPrecentage);
            Assert.True(instance.shouldApplyFreeShipping);
            Assert.False(instance.shouldSuggestUpgradeToPreferred);
        }
    }
}