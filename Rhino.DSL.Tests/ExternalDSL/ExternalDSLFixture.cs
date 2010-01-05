namespace Rhino.DSL.Tests.ExternalDSL
{
	using Xunit;

	public class ExternalDSLFixture
	{
		private readonly string sentence =
			@"When a customer is preferred and the order exceeds 1000 then apply a .05 discount and apply free shipping.";

		[Fact]
		public void WillRemoveNoiseWordsFromSentence()
		{
			string withoutNoise = Parser.RemoveNoiseWords(sentence);
			string expected = "When customer is preferred and order exceeds 1000 then apply .05 discount and apply free shipping";
			Assert.Equal(expected, withoutNoise);
		}

		[Fact]
		public void WillHaveTwoActionsInWhenClause()
		{
			WhenThenClause parsed = new Parser().Parse(sentence);
			Assert.Equal(2, parsed.When.Count);
		}

		[Fact]
		public void EachWhenItemShouldBeSplitToObjectOperatorAndValue()
		{
			WhenThenClause parsed = new Parser().Parse(sentence);
			Assert.Equal("customer", parsed.When[0].Left);
			Assert.Equal("is", parsed.When[0].Operator);
			Assert.Equal("preferred", parsed.When[0].Right);
			Assert.Equal("order", parsed.When[1].Left);
			Assert.Equal("exceeds", parsed.When[1].Operator);
			Assert.Equal("1000", parsed.When[1].Right);
		}

		[Fact]
		public void WillHaveTwoActionsInThenClause()
		{
			WhenThenClause parsed = new Parser().Parse(sentence);
			Assert.Equal(2, parsed.Then.Count);
		}

		[Fact]
		public void EachThenItemShouldBeSplitToObjectOperatorAndValue()
		{
			WhenThenClause parsed = new Parser().Parse(sentence);
			Assert.Equal("apply", parsed.Then[0].Left);
			Assert.Equal("discount", parsed.Then[0].Operator);
			Assert.Equal(".05", parsed.Then[0].Right);
			Assert.Equal("apply", parsed.Then[1].Left);
			Assert.Equal("shipping", parsed.Then[1].Operator);
			Assert.Equal("free", parsed.Then[1].Right);
		}

		[Fact]
		public void CanExecuteSimpleAction()
		{
			DslExecuter executer = new DslExecuter();
			executer.AddParameter("book", new DemoBook());
			object result = executer.Invoke(new ActionExpression("book", "pageTo", "51"));
			Assert.Equal(51, result);
		}

		[Fact]
		public void CanExecuteWhenStatement()
		{
			ExternalDSLDemo dsl = new ExternalDSLDemo(sentence);
			dsl.AddParameter("customer", new Customer());
			dsl.AddParameter("order", new Order());
			dsl.Execute();
		}

		[Fact]
		public void CanExecuteThenStatement()
		{
			ExternalDSLDemo dsl = new ExternalDSLDemo(sentence);
			Customer customer = new Customer();
			customer.CustomerStatus = CustomerStatus.Preferred;
			dsl.AddParameter("customer", customer);
			Order order = new Order();
			order.TotalCost = 5000;
			order.ShippingType = ShippingType.Fast;
			dsl.AddParameter("order", order);
			dsl.AddParameter("apply", new ApplyCommands(order));
			dsl.Execute();

			Assert.Equal(4750, order.TotalCost);
			Assert.Equal(ShippingType.Free, order.ShippingType);
		}
	}

	public class ApplyCommands
	{
		private readonly Order order;

		public ApplyCommands(Order order)
		{
			this.order = order;
		}

		public void Discount(double precentage)
		{
			order.TotalCost = order.TotalCost - (order.TotalCost*precentage);
		}

		public void Shipping(ShippingType shipping)
		{
			order.ShippingType = shipping;
		}
	}

	public class Customer
	{
		public CustomerStatus CustomerStatus;

		public bool Is(CustomerStatus status)
		{
			return CustomerStatus == status;
		}
	}

	public class DemoBook
	{
		public int PageTo(int page)
		{
			return page;
		}
	}

	public class Order
	{
		public ShippingType ShippingType;
		public double TotalCost;

		public bool Exceeds(double amount)
		{
			return TotalCost > amount;
		}
	}

	public enum ShippingType
	{
		Free,
		Cheap,
		Fast,
	}


	public enum CustomerStatus
	{
		Preferred,
		Regular
	}

}