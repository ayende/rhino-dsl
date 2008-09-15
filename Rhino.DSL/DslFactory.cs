namespace Rhino.DSL
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Boo.Lang.Compiler;

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
		private readonly List<string> standAloneCompilation = new List<string>();

		/// <summary>
		/// The base directory to read all the relative url from.
		/// </summary>
		public string BaseDirectory
		{
			get { return baseDirectory ?? ""; }
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
			return CreateInternal<TDslBase>(ScriptNotFoundBehavior.Throw,
				url, parameters);
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
			return CreateInternal<TDslBase>(ScriptNotFoundBehavior.ReturnNull,
				url, parameters);
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
			DslEngine engine = GetEngine<TDslBase>();
			
			List<TDslBase> instances = new List<TDslBase>();
			engine.Cache.ReadLock(delegate
			{
				foreach (string dsl in GetUrlsFromDslEngine(engine, ref parentUrl))
				{
					instances.Add(Create<TDslBase>(dsl, parameters));
				}
			});
			return instances.ToArray();
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

		private TDslBase CreateInternal<TDslBase>(ScriptNotFoundBehavior notFoundBehavior, string url, object[] parameters)
		{
			DslEngine engine = GetEngine<TDslBase>();
			Type type = null;
			engine.Cache.ReadLock(delegate
			{
				type = engine.Cache.Get(engine.CanonizeUrl(url));
				if (type == null)
				{
					bool recompilation;
					string[] urls = GetUrls(engine, ref url, out recompilation);
					bool existsInArray = engine.Storage.IsUrlIncludeIn(urls, BaseDirectory, url);
					if (existsInArray == false)
					{
						if (notFoundBehavior == ScriptNotFoundBehavior.Throw)
						{
							throw new InvalidOperationException("Could not find DSL script: " + url);
						}
						return;
					}
					CompilerContext compilerContext;
					try
					{
						compilerContext = engine.Compile(urls);
					}
					catch (Exception)
					{
						// if we fail to compile with batch, we will try just the current url
						urls = new string[] { url };
						compilerContext = engine.Compile(urls);
					}
					Assembly assembly = compilerContext.GeneratedAssembly;
					RegisterBatchInCache(engine, urls, assembly);
					//find the type that we searched for
					//we may have a race condition with the cache, so we force
					//it to go through the assemly instead of the cache
					type = engine.GetTypeForUrl(assembly, url);
					RaiseCompilationEvent(recompilation);
				}
			});
			if (type == null)
				return default(TDslBase);
			return (TDslBase)engine.CreateInstance(type, parameters);
		}

		private DslEngine GetEngine<TDslBase>()
		{
			DslEngine engine;
			if (typeToDslEngine.TryGetValue(typeof(TDslBase), out engine) == false)
				throw new InvalidOperationException("Could not find an engine to process type: " + typeof(TDslBase));
			return engine;
		}

		private void RaiseCompilationEvent(bool recompilation)
		{
			Compilation(this, EventArgs.Empty);
			if (recompilation)
			{
				Recompilation(this, EventArgs.Empty);
			}
		}

		private string[] GetUrls(DslEngine engine, ref string url, out bool recompilation)
		{
			string[] urls;
			// we need to compile this separatedly, instead of
			// in a batch. This is usually happening when a script
			// has changed
			recompilation = false;
			if (standAloneCompilation.Contains(url))
			{
				standAloneCompilation.Remove(url);
				urls = new string[] { url };
				recompilation = true;
			}
			else
			{
				urls = GetUrlsFromDslEngine(engine, ref url);
			}
			return urls;
		}

		private void RegisterBatchInCache(DslEngine engine, IEnumerable<string> urls, Assembly compiledAssembly)
		{
			engine.Cache.WriteLock(delegate
			{
				foreach (string batchUrl in urls)
				{
					Type type = engine.GetTypeForUrl(compiledAssembly, batchUrl);
					if (type == null)
						throw new InvalidOperationException("Could not find the generated type for: " + batchUrl);
					engine.Cache.Set(batchUrl, type);
				}
				engine.Storage.NotifyOnChange(urls, delegate(string invalidatedUrl)
				{
					engine.Cache.Remove(invalidatedUrl);
					standAloneCompilation.Add(invalidatedUrl);
				});
			});
		}

		private string[] GetUrlsFromDslEngine(DslEngine engine, ref string path)
		{
			string[] matchingUrls = engine.Storage.GetMatchingUrlsIn(BaseDirectory, ref path);
			List<string> urls = new List<string>(matchingUrls ?? new string[0]);
			// even if the path is in the cache, we still return the it
			// so we will get a new version
			if (urls.Exists(GetMatchPathPredicate(path)) == false &&
				engine.Storage.IsValidScriptUrl(path))
				urls.Add(path);
			return urls.ToArray();
		}

		private static Predicate<string> GetMatchPathPredicate(string path)
		{
			return delegate(string url)
			{
				return path.Equals(url, StringComparison.InvariantCultureIgnoreCase);
			};
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
