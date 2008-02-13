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
        /// This is used to mark urls that should be compiled on their own
        /// usually this means scripts that has been changed after they were 
        /// compiled
        /// </summary>
        private readonly List<Uri> standAloneCompilation = new List<Uri>();

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
        public TDslBase TryCreate<TDslBase>(string url, params object[] parameters)
        {
            string directory = BaseDirectory ?? "";
            url = Path.Combine(directory, url);
            return TryCreate<TDslBase>(new Uri(Path.GetFullPath(url)), parameters);
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
            return CreateInternal<TDslBase>(ScriptNotFoundBehavior.Throw, url, parameters);
        }

        /// <summary>
        /// Tries to create a new DSL instance, if it exists.
        /// If it doesn't, return null.
        /// </summary>
        /// <typeparam name="TDslBase">The base type of the DSL</typeparam>
        /// <param name="url">The url to read the DSL file from</param>
        /// <param name="parameters">optional ctor parameters</param>
        /// <returns>The dsl instance</returns>
        public TDslBase TryCreate<TDslBase>(Uri url, params object[] parameters)
        {
            return CreateInternal<TDslBase>(ScriptNotFoundBehavior.ReturnNull, url, parameters);
        }

        /// <summary>
        /// Occurs when a compilation is completed
        /// Useful to track how many assemblies has been loaded
        /// by the DslFactory
        /// </summary>
        public event EventHandler Compilation = delegate { };
        /// <summary>
        /// Occurs when recompilation of a script is completed.
        /// Useful to track how many assemblies has been loaded by
        /// the DslFactory
        /// </summary>
        public event EventHandler Recompilation = delegate { };

        private enum ScriptNotFoundBehavior
        {
            Throw,
            ReturnNull
        }

        private TDslBase CreateInternal<TDslBase>(ScriptNotFoundBehavior notFoundBehavior, Uri url, object[] parameters)
        {
            DslEngine engine;
            if (typeToDslEngine.TryGetValue(typeof(TDslBase), out engine) == false)
                throw new InvalidOperationException("Could not find an engine to process type: " + typeof(TDslBase));
            Type type = engine.GetFromCache(url);
            if (type == null)
            {
                bool recompilation;
                Uri[] urls = GetUrls(engine, url, out recompilation);
                if (Array.IndexOf(urls, url) == -1)
                {
                    if (notFoundBehavior == ScriptNotFoundBehavior.Throw)
                    {
                        throw new InvalidOperationException("Could not find DSL script: " + url);
                    }
                    else
                    {
                        return default(TDslBase);
                    }
                }
                Assembly assembly = engine.Compile(urls).GeneratedAssembly;
                RegisterBatchInCache(engine, urls, assembly);
                //find the type that we searched for
                //we may have a race condition with the cache, so we force
                //it to go through the assemly instead of the cache
                type = engine.GetTypeForUrl(assembly, url);
                RaiseCompilationEvent(recompilation);
            }
            return (TDslBase)engine.CreateInstance(type, parameters);
        }

        private void RaiseCompilationEvent(bool recompilation)
        {
            Compilation(this, EventArgs.Empty);
            if (recompilation)
            {
                Recompilation(this, EventArgs.Empty);
            }
        }

        private Uri[] GetUrls(DslEngine engine, Uri url, out bool recompilation)
        {
            Uri[] urls;
            // we need to compile this separatedly, instead of
            // in a batch. This is usually happening when a script
            // has changed
            recompilation = false;
            if (standAloneCompilation.Contains(url))
            {
                standAloneCompilation.Remove(url);
                urls = new Uri[] { url };
                recompilation = true;
            }
            else
            {
                urls = GetUrls(engine, Path.GetDirectoryName(url.AbsolutePath));
            }
            return urls;
        }

        private void RegisterBatchInCache(DslEngine engine, IEnumerable<Uri> urls, Assembly compiledAssembly)
        {
            foreach (Uri batchUrl in urls)
            {
                Type type = engine.GetTypeForUrl(compiledAssembly, batchUrl);
                if (type == null)
                    throw new InvalidOperationException("Could not find the generated type for: " + batchUrl);
                engine.SetInCache(batchUrl, type);
                engine.NotifyOnChange(urls, delegate(Uri invalidatedUrl)
                {
                    engine.RemoveFromCache(invalidatedUrl);
                    standAloneCompilation.Add(invalidatedUrl);
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