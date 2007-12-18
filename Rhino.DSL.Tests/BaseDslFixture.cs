namespace Rhino.DSL.Tests
{
    using System;
    using MbUnit.Framework;

    public class BaseDslFixture<TDslEngine, TDslBase>
           where TDslEngine : DslEngine, new()
    {
        protected DslFactory factory;

        [SetUp]
        public void SetUp()
        {
            factory = new DslFactory();
            factory.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            this.factory.Register<TDslBase>(new TDslEngine());
        }

    }
}