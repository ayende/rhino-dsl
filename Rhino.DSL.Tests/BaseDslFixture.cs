namespace Rhino.DSL.Tests
{
    using System;
    using Xunit;

    public class BaseDslFixture<TDslEngine, TDslBase>
           where TDslEngine : DslEngine, new()
    {
        protected DslFactory factory;

        public BaseDslFixture()
        {
            factory = new DslFactory();
            factory.BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            this.factory.Register<TDslBase>(new TDslEngine());
        }

    }
}