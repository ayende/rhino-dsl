namespace Rhino.DSL.Tests.OrderDSL
{
	public class User
	{
		bool isPreferred;

		public bool IsPreferred
		{
			get { return isPreferred; }
			set { isPreferred = value; }
		}

		public bool IsNotPreferred
		{
			get { return isPreferred == false; }
		}
	}
}