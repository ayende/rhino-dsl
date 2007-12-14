namespace Rhino.DSL.Tests
{
    using MbUnit.Framework;

    public class BaseDslFixture<TDslEngine, TDslBase>
           where TDslEngine : DslEngine, new()
    {
        protected DslFactory factory;

        [SetUp]
        public void SetUp()
        {
            factory = new DslFactory();
            this.factory.Register<TDslBase>(new TDslEngine());
        }

    }
}