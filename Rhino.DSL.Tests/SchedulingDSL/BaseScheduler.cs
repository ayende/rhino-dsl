namespace Rhino.DSL.Tests.SchedulingDSL
{
	using System;
	using Boo.Lang;
	using Boo.Lang.Compiler.Ast;
	using Boo.Lang.Compiler.MetaProgramming;

	public abstract class BaseScheduler
	{
		protected TimeSpan repetition;
		protected DateTime startingTime;
		protected ConditionDelegate condition;
		protected ActionDelegate action;
		protected string whoToNotify;
		protected string notifyMessage;
		private string taskName;
		private bool actionExecuted = false;

		public abstract void Prepare();

		public void Run()
		{
            //in real code, we would probably use this method
            //to register this class in a scheduling engine
			if (condition())
			{
				actionExecuted = true;
				action();
			}
		}

		public delegate bool ConditionDelegate();

		public delegate void ActionDelegate();

		public void task(string name, ActionDelegate taskDelegate)
		{
			this.taskName = name;
			taskDelegate();
		}

		public void then(ActionDelegate actionDelegate)
		{
			this.action = actionDelegate;
		}

		public void every(TimeSpan timeSpan)
		{
			this.repetition = timeSpan;
		}

		public void starting(DateTime date)
		{
			this.startingTime = date;
		}

		[Meta]
		public static Expression when(Expression expression)
		{
			BlockExpression right = new BlockExpression();
			right.Body.Add(new ReturnStatement(expression));
			return new BinaryExpression(
				BinaryOperatorType.Assign,
				new ReferenceExpression("condition"),
				right
			);
		}

		public void notify(string who, string message)
		{
			this.whoToNotify = who;
			this.notifyMessage = message;
		}

		public DateTime now
		{
			// this is a fake, just for testing, would probably be
			// DateTime.Now in real code
			get { return new DateTime(2000, 1, 1); }
		}

		[Extension]
		public static TimeSpan Minutes(int number)
		{
			return TimeSpan.FromMinutes(number);
		}

		#region Exposed For Testing

		public TimeSpan Repetition
		{
			get { return repetition; }
			set { repetition = value; }
		}

		public DateTime StartingTime
		{
			get { return startingTime; }
			set { startingTime = value; }
		}

		public ConditionDelegate Condition
		{
			get { return condition; }
			set { condition = value; }
		}

		public ActionDelegate Action
		{
			get { return action; }
			set { action = value; }
		}


		public string WhoToNotify
		{
			get { return whoToNotify; }
			set { whoToNotify = value; }
		}

		public string NotifyMessage
		{
			get { return notifyMessage; }
			set { notifyMessage = value; }
		}

		public string TaskName
		{
			get { return taskName; }
			set { taskName = value; }
		}

		public bool ActionExecuted
		{
			get { return actionExecuted; }
			set { actionExecuted = value; }
		}

		#endregion
	}
}