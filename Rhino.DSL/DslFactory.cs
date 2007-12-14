namespace Rhino.DSL
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.IO;

    public class DslFactory
    {
        private readonly IDictionary<Type, DslEngine> typeToDslEngine = new Dictionary<Type, DslEngine>();

        public void Register<TDslBase>(DslEngine engine)
        {
            typeToDslEngine.Add(typeof(TDslBase), engine);
        }

        public TDslBase Create<TDslBase>(string url, params object[] parameters)
        {
            return Create<TDslBase>(new Uri(Path.GetFullPath(url)), parameters);
        }
        public TDslBase Create<TDslBase>(Uri url, params object[] parameters)
        {
            DslEngine engine;
            if (typeToDslEngine.TryGetValue(typeof(TDslBase), out engine) == false)
                throw new InvalidOperationException("Could not find an engine to process type: " + typeof(TDslBase));
            Type type = engine.GetFromCache(url);
            if (type == null)
            {
                Uri[] urls = GetUrls(engine, url);
                Assembly assembly = engine.Compile(urls).GeneratedAssembly;
                RegisterBatchInCache(engine, urls, assembly);
                //find the type that we searched for
                //we may have a race condition with the cache, so we force
                //it to go through the assemly instead of the cache
                type = engine.GetTypeForUrl(assembly, url);
            }
            return (TDslBase)engine.CreateInstance(type, parameters);
        }

        private static void RegisterBatchInCache(DslEngine engine, IEnumerable<Uri> urls, Assembly compile)
        {
            foreach (Uri batchUrl in urls)
            {
                Type type = engine.GetTypeForUrl(compile, batchUrl);
                if (type == null)
                    throw new InvalidOperationException("Could not find the generated type for: " + batchUrl);
                engine.SetInCache(batchUrl, type);
            }
        }

        private static Uri[] GetUrls(DslEngine engine, Uri url)
        {
            Uri[] matchingUrls = engine.GetMatchingUrlsIn(Path.GetDirectoryName(url.AbsolutePath));
            List<Uri> urls = new List<Uri>();
            if (matchingUrls != null)
            {
                urls.AddRange(matchingUrls);
            }
            if (urls.Contains(url) == false)
                urls.Add(url);
            return urls.ToArray();
        }

        public bool IsRegistered<TDslBase>()
        {
            return typeToDslEngine.ContainsKey(typeof(TDslBase));
        }
    }
}
