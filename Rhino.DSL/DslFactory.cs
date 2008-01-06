namespace Rhino.DSL
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.IO;

    /// <summary>
    /// Manage the creation of DSL instances, cache and creates them.
    /// </summary>
    public class DslFactory
    {
        private readonly IDictionary<Type, DslEngine> typeToDslEngine = new Dictionary<Type, DslEngine>();
        private string baseDirectory;

        /// <summary>
        /// The base directory to read all the relative url from.
        /// </summary>
        public string BaseDirectory
        {
            get { return baseDirectory; }
            set { baseDirectory = value; }
        }

        ///<summary>
        /// Register a new DSL engine that is tied to a specific base type
        ///</summary>
        public void Register<TDslBase>(DslEngine engine)
        {
            typeToDslEngine.Add(typeof(TDslBase), engine);
        }

        /// <summary>
        /// Create a new DSL instance
        /// </summary>
        /// <typeparam name="TDslBase">The base type of the DSL</typeparam>
        /// <param name="url">The url to read the DSL file from</param>
        /// <param name="parameters">optional ctor parameters</param>
        /// <returns>The dsl instance</returns>
        public TDslBase Create<TDslBase>(string url, params object[] parameters)
        {
            string directory = BaseDirectory ?? "";
            url = Path.Combine(directory, url);
            return Create<TDslBase>(new Uri(Path.GetFullPath(url)), parameters);
        }

        /// <summary>
        /// Create a new DSL instance
        /// </summary>
        /// <typeparam name="TDslBase">The base type of the DSL</typeparam>
        /// <param name="url">The url to read the DSL file from</param>
        /// <param name="parameters">optional ctor parameters</param>
        /// <returns>The dsl instance</returns>
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

        /// <summary>
        /// Check if there was a DSL registered for this base type.
        /// </summary>
        public bool IsRegistered<TDslBase>()
        {
            return typeToDslEngine.ContainsKey(typeof(TDslBase));
        }
    }
}
