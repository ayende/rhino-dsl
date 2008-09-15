namespace Rhino.DSL.Tests.DslFactoryFixture
{
    using System;
    using System.IO;
    using System.Reflection;
    using Boo.Lang.Compiler;
    using MbUnit.Framework;
    using Mocks;

    [TestFixture]
    public class DslFactoryFixture
    {
        private string testUrl = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test");

        private CompilerContext context;
        private DslFactory factory;
        private DslEngine mockedDslEngine;
        private MockRepository mocks;
        private IDslEngineCache mockCache;

        [SetUp]
        public void SetUp()
        {
            factory = new DslFactory();
            mocks = new MockRepository();
            mockedDslEngine = mocks.DynamicMock<DslEngine>();
            mockCache = mocks.DynamicMock<IDslEngineCache>();
        	
			mockCache.WriteLock(null);
        	LastCall.Repeat.Any()
        		.IgnoreArguments()
        		.Do((Action<CacheAction>) ExecuteCachedAction);
			
			mockCache.ReadLock(null);
			LastCall.Repeat.Any()
				.IgnoreArguments()
				.Do((Action<CacheAction>)ExecuteCachedAction);
            
			IDslEngineStorage mockStorage = mocks.DynamicMock<IDslEngineStorage>();
            Assembly assembly = Assembly.GetCallingAssembly();
            context = new CompilerContext();
            context.GeneratedAssembly = assembly;
            mockedDslEngine.Storage = mockStorage;
            mockedDslEngine.Cache = mockCache;

            SetupResult.For(mockStorage.GetMatchingUrlsIn("", ref testUrl)).Return(new string[] { testUrl });
            SetupResult.For(mockStorage.IsUrlIncludeIn(null, null, null))
                .IgnoreArguments()
                .Return(true);
        }

    	private void ExecuteCachedAction(CacheAction action)
    	{
    		action();
    	}

    	[Test]
        public void Can_register_a_dsl_engine_for_base_type()
        {
            factory.Register<IDisposable>(mockedDslEngine);
            Assert.IsTrue(factory.IsRegistered<IDisposable>());
        }

        [Test]
        public void When_requesting_a_DSL_instance_will_first_try_to_get_from_cache()
        {
            using (mocks.Record())
            {
                SetupResult.For(mockedDslEngine.GetTypeForUrl(null, null))
                    .IgnoreArguments()
                    .Return(typeof (DslEngine));
                SetupResult.For(mockedDslEngine.Compile(testUrl)).Return(context);
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(
                    typeof (DslEngine));
                Expect.Call(delegate { mockCache.Set(testUrl, typeof(DslEngine)); })
                    .Repeat.Any();
            }

            factory.Register<IDisposable>(mockedDslEngine);

            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl);
            }
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException),
            "Could not find an engine to process type: System.IDisposable")]
        public void When_try_to_get_a__non_existant_dsl_should_throw()
        {
            factory.Create<IDisposable>(testUrl);
        }

        [Test]
        public void When_request_a_DSL_isntance_will_ask_engine_to_create_it()
        {
            using (mocks.Record())
            {
                SetupResult.For(mockedDslEngine.Compile(testUrl)).Return(context);
                SetupResult.For(mockedDslEngine.GetTypeForUrl(null, null))
                    .IgnoreArguments()
                    .Return(typeof (DslEngine));
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(
                    typeof (DslEngine));
                Expect.Call(delegate { mockCache.Set(testUrl, typeof(DslEngine)); })
                    .Repeat.Any();

                Expect.Call(mockedDslEngine.CreateInstance(typeof (DslEngine))).Return(null);
            }

            factory.Register<IDisposable>(mockedDslEngine);
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl);
            }
        }

        [Test]
        public void When_DSL_engine_successfully_compiled_will_register_all_types_in__the_cache()
        {
            using (mocks.Record())
            {
                Expect.Call(mockedDslEngine.Compile(testUrl)).Return(context);
                Expect.Call(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(typeof (DslEngine));
                Expect.Call(delegate { mockCache.Set(testUrl, typeof(DslEngine)); });
            }

            factory.Register<IDisposable>(mockedDslEngine);
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl);
            }
        }

        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void When_Type_does_not_exist_in_compiled_assembly_should_thorw()
        {
            using (mocks.Record())
            {
                Expect.Call(mockedDslEngine.Compile(testUrl)).Return(context);
                Expect.Call(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(null);
            }

            factory.Register<IDisposable>(mockedDslEngine);
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl);
            }
        }

        [Test]
        public void When_create_a_DSL_instance_should_ask_engine_to_create_new_instance()
        {
            using (mocks.Record())
            {
                SetupResult.For(mockedDslEngine.Compile(testUrl)).Return(context);
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(
                    typeof (MyFake));
                Expect.Call(mockedDslEngine.CreateInstance(typeof (MyFake))).Return(null);
            }

            factory.Register<IDisposable>(mockedDslEngine);
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl);
            }
        }

        [Test]
        public void Should_be_able_to_pass_parameters_to_the_constructor_of_the_Type()
        {
            using (mocks.Record())
            {
                SetupResult.For(mockedDslEngine.Compile(testUrl)).Return(context);
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(
                    typeof (MyFake));
                Expect.Call(mockedDslEngine.CreateInstance(typeof (MyFake), "myParam")).Return(null);
            }

            factory.Register<IDisposable>(mockedDslEngine);
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl, "myParam");
            }
        }

        #region Nested type: MyFake

        public class MyFake
        {
            private readonly string variable;

            public MyFake(string variable)
            {
                this.variable = variable;
            }

            public MyFake()
            {
            }

            public string Variable
            {
                get { return variable; }
            }
        }

        #endregion
    }
}