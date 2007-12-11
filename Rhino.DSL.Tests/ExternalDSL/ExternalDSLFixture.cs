namespace Rhino.DSL.Tests.ExternalDSL
{
	using MbUnit.Framework;

	[TestFixture]
	public class ExternalDSLFixture
	{
		private readonly string sentence =
			@"When a customer is preferred and the order exceeds 1000 then apply a .05 discount and apply free shipping.";

		[Test]
		public void WillRemoveNoiseWordsFromSentence()
		{
			string withoutNoise = Parser.RemoveNoiseWords(sentence);
			string expected = "When customer is preferred and order exceeds 1000 then apply .05 discount and apply free shipping";
			Assert.AreEqual(expected, withoutNoise);
		}

		[Test]
		public void WillHaveTwoActionsInWhenClause()
		{
			WhenThenClause parsed = new Parser().Parse(sentence);
			Assert.AreEqual(2, parsed.When.Count);
		}

		[Test]
		public void EachWhenItemShouldBeSplitToObjectOperatorAndValue()
		{
			WhenThenClause parsed = new Parser().Parse(sentence);
			Assert.AreEqual("customer", parsed.When[0].Left);
			Assert.AreEqual("is", parsed.When[0].Operator);
			Assert.AreEqual("preferred", parsed.When[0].Right);
			Assert.AreEqual("order", parsed.When[1].Left);
			Assert.AreEqual("exceeds", parsed.When[1].Operator);
			Assert.AreEqual("1000", parsed.When[1].Right);
		}

		[Test]
		public void WillHaveTwoActionsInThenClause()
		{
			WhenThenClause parsed = new Parser().Parse(sentence);
			Assert.AreEqual(2, parsed.Then.Count);
		}

		[Test]
		public void EachThenItemShouldBeSplitToObjectOperatorAndValue()
		{
			WhenThenClause parsed = new Parser().Parse(sentence);
			Assert.AreEqual("apply", parsed.Then[0].Left);
			Assert.AreEqual("discount", parsed.Then[0].Operator);
			Assert.AreEqual(".05", parsed.Then[0].Right);
			Assert.AreEqual("apply", parsed.Then[1].Left);
			Assert.AreEqual("shipping", parsed.Then[1].Operator);
			Assert.AreEqual("free", parsed.Then[1].Right);
		}

		[Test]
		public void CanExecuteSimpleAction()
		{
			DslExecuter executer = new DslExecuter();
			executer.AddParameter("book", new DemoBook());
			object result = executer.Invoke(new ActionExpression("book", "pageTo", "51"));
			Assert.AreEqual(51, result);
		}

		[Test]
		public void CanExecuteWhenStatement()
		{
			ExternalDSLDemo dsl = new ExternalDSLDemo(sentence);
			dsl.AddParameter("customer", new Customer());
			dsl.AddParameter("order", new Order());
			dsl.Execute();
		}

		[Test]
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

			Assert.AreEqual(4750, order.TotalCost);
			Assert.AreEqual(ShippingType.Free, order.ShippingType);
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