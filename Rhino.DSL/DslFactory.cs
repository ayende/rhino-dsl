namespace Rhino.DSL
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

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
        /// Creates instances of all the DSL that are located directly under the parent URL.
        /// </summary>
        /// <typeparam name="TDslBase">The type of the DSL base.</typeparam>
        /// <param name="parentUrl">The parent URL.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public TDslBase[] CreateAll<TDslBase>(string parentUrl, params object[] parameters)
        {
            string directory = BaseDirectory ?? "";
            parentUrl = Path.Combine(directory, parentUrl);
            DslEngine engine;
            if (typeToDslEngine.TryGetValue(typeof(TDslBase), out engine) == false)
                throw new InvalidOperationException("Could not find an engine to process type: " + typeof(TDslBase));
            List<TDslBase> instances = new List<TDslBase>();
            foreach (Uri dsl in GetUrls(engine, Path.GetFullPath(parentUrl)))
            {
                instances.Add(Create<TDslBase>(dsl, parameters));
            }
            return instances.ToArray();
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
                Uri[] urls = GetUrls(engine, Path.GetDirectoryName(url.AbsolutePath));
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
                engine.NotifyOnChange(urls, delegate(Uri invalidatedUrl)
                {
                    engine.RemoveFromCache(invalidatedUrl);
                });
            }
        }

        private static Uri[] GetUrls(DslEngine engine, string path)
        {
            Uri[] matchingUrls = engine.GetMatchingUrlsIn(path);
            List<Uri> urls = new List<Uri>();
            if (matchingUrls != null)
            {
                urls.AddRange(matchingUrls);
            }
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