namespace Rhino.DSL.Tests.DslFactoryFixture
{
    using System.IO;
    using Boo.Lang.Compiler;
    using DSL;
    using MbUnit.Framework;
    using Mocks;
    using System;
    using System.Reflection;

    [TestFixture]
	public class DslFactoryFixture
	{
	    private DslFactory factory;
        private MockRepository mocks;
        private DslEngine mockedDslEngine;
        private readonly Uri testUrl = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test"));
        private CompilerContext context;

        [SetUp]
	    public void SetUp()
	    {
	        factory = new DslFactory();
            mocks = new MockRepository();
            mockedDslEngine = mocks.DynamicMock<DslEngine>();

            Assembly assembly = Assembly.GetCallingAssembly();
            context = new CompilerContext();
            context.GeneratedAssembly = assembly;
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
                Expect.Call(mockedDslEngine.GetFromCache(testUrl)).Return(null);
                SetupResult.For(mockedDslEngine.GetTypeForUrl(null, null))
                    .IgnoreArguments()
                    .Return(typeof(DslEngine));
                SetupResult.For(mockedDslEngine.Compile(testUrl)).Return(context);
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(typeof(DslEngine));
                Expect.Call(delegate { mockedDslEngine.SetInCache(testUrl, typeof(DslEngine)); })
                    .Repeat.Any();
     
            }
            
            factory.Register<IDisposable>(mockedDslEngine);
	        
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl);    
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), "Could not find an engine to process type: System.IDisposable")]
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
                   .Return(typeof(DslEngine));
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(typeof(DslEngine));
                Expect.Call(delegate { mockedDslEngine.SetInCache(testUrl, typeof(DslEngine)); })
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
                Expect.Call(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(typeof(DslEngine));
                Expect.Call(delegate { mockedDslEngine.SetInCache(testUrl, typeof (DslEngine)); });
            }

            factory.Register<IDisposable>(mockedDslEngine);
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl);
            }
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
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
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(typeof(MyFake));
                Expect.Call(mockedDslEngine.CreateInstance(typeof(MyFake))).Return(null);
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
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(typeof(MyFake));
                Expect.Call(mockedDslEngine.CreateInstance(typeof(MyFake),"myParam")).Return(null);
            }

            factory.Register<IDisposable>(mockedDslEngine);
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl,"myParam");
            }    
        }

        [Test]
        public void WillPerformBatchCompilationForEngine()
        {
            using (mocks.Record())
            {
                SetupResult.For(mockedDslEngine.GetMatchingUrlsIn(AppDomain.CurrentDomain.BaseDirectory)).Return(new Uri[]
                                                                                                          {
                                                                                                              new Uri(Path.GetFullPath("foo")),new Uri(Path.GetFullPath("bar"))
                                                                                                          });
                Expect.Call(mockedDslEngine.Compile(new Uri(Path.GetFullPath("foo")),new Uri(Path.GetFullPath("bar")), testUrl))
                    .Return(context);
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl)).Return(typeof(MyFake));
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, new Uri(Path.GetFullPath("foo")))).Return(typeof(MyFake));
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, new Uri(Path.GetFullPath("bar")))).Return(typeof(MyFake));
                SetupResult.For(mockedDslEngine.CreateInstance(typeof(MyFake), "myParam")).Return(null);
            }

            factory.Register<IDisposable>(mockedDslEngine);
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl, "myParam");
            }    
        }

        [Test]
        public void When_factory_created_assembly_should_register_all_types_that_were_involved_IndexerProviderAttribute_batch()
        {
            using (mocks.Record())
            {
                SetupResult.For(mockedDslEngine.GetMatchingUrlsIn(AppDomain.CurrentDomain.BaseDirectory))
                    .Return(new Uri[]
                    {
                        new Uri(Path.GetFullPath("foo")),new Uri(Path.GetFullPath("bar"))
                    });
                SetupResult.For(mockedDslEngine.Compile(new Uri(Path.GetFullPath("foo")), new Uri(Path.GetFullPath("bar")), testUrl))
                    .Return(context);
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, testUrl))
                    .Return(typeof(MyFake));
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, new Uri(Path.GetFullPath("foo"))))
                    .Return(typeof(MyFake));
                SetupResult.For(mockedDslEngine.GetTypeForUrl(context.GeneratedAssembly, new Uri(Path.GetFullPath("bar"))))
                    .Return(typeof(MyFake));

                Expect.Call(delegate { mockedDslEngine.SetInCache(testUrl, typeof(MyFake)); });
                Expect.Call(delegate { mockedDslEngine.SetInCache(new Uri(Path.GetFullPath("bar")), typeof(MyFake)); });
                Expect.Call(delegate { mockedDslEngine.SetInCache(new Uri(Path.GetFullPath("foo")), typeof(MyFake)); });

                SetupResult.For(mockedDslEngine.CreateInstance(typeof(MyFake), "myParam")).Return(null);
            }

            factory.Register<IDisposable>(mockedDslEngine);
            using (mocks.Playback())
            {
                factory.Create<IDisposable>(testUrl, "myParam");
            }    
        }

        public class MyFake
        {
            private readonly string variable;

            public string Variable
            {
                get { return variable; }
            }

            public MyFake(string variable)
            {
                this.variable = variable;
            }

            public MyFake()
            {
            }
        }
	}
}