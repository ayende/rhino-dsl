namespace Rhino.DSL.Tests.SchedulingDSL
{
	public class WebSite
	{
		public static bool aliveValue;
		public static string requestedUrl;

		public WebSite(string url)
		{
			requestedUrl = url;
		}

		public bool IsAlive
		{
			get { return aliveValue; }
		}
	}
}