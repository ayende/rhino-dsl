namespace Rhino.DSL.Tests.OrderDSL
{
	using System.Collections;
	using System.Collections.Specialized;
	using Boo.Lang;
	using Boo.Lang.Compiler.Ast;

	public abstract class BaseOrderActionsDSL
	{
		public delegate bool Condition();
		public delegate void Action();

		protected OrderedDictionary conditionsAndActions = new OrderedDictionary();
		public User User;
		public Order Order;

		public decimal discountPrecentage;
		public bool shouldSuggestUpgradeToPreferred;
		public bool shouldApplyFreeShipping;

		protected void AddDiscountPrecentage(decimal precentage)
		{
			this.discountPrecentage = precentage;
		}

		protected void SuggestUpgradeToPreferred()
		{
			shouldSuggestUpgradeToPreferred = true;
		}

		protected void ApplyFreeShipping()
		{
			shouldApplyFreeShipping = true;
		}

		[Meta]
		public static Expression when(Expression expression, Expression action)
		{
			BlockExpression condition = new BlockExpression();
			condition.Body.Add(new ReturnStatement(expression));
			return new MethodInvocationExpression(
				new ReferenceExpression("When"),
				condition,
				action
			);
		}

		protected void When(Condition condition, Action action)
		{
			conditionsAndActions[condition] = action;
		}

		public abstract void Prepare();

		public void Execute()
		{
			foreach (DictionaryEntry entry in conditionsAndActions)
			{
				Condition condition = (Condition) entry.Key;
				if(condition())
				{
					Action action = (Action) entry.Value;
					action();
					break;
				}
			}
		}
	}
}