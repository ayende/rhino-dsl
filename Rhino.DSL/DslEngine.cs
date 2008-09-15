namespace Rhino.DSL
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Boo.Lang.Compiler;
    using Boo.Lang.Compiler.Pipelines;
    using System.IO;

    /// <summary>
    /// Base class for DSL engines, handles most of the routine tasks that a DSL
    /// engine needs to do. Compilation, caching, creation, etc.
    /// </summary>
    public abstract class DslEngine
    {
        private IDslEngineStorage storage;
        private IDslEngineCache cache;
		private DslCompilerContextCache compilerContextCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DslEngine"/> class.
        /// </summary>
        public DslEngine()
        {
            Storage = new FileSystemDslEngineStorage();
            Cache = new DefaultDslEngineCache();
        }


        /// <summary>
        /// Gets or sets the cache.
        /// </summary>
        /// <value>The cache.</value>
        public IDslEngineCache Cache
        {
            get { return cache; }
            set { cache = value; }
        }

        /// <summary>
        /// Gets or sets the storage for this DSL
        /// </summary>
        /// <value>The storage.</value>
        public IDslEngineStorage Storage
        {
            get { return storage; }
            set 
			{
                storage = value;
                CompilerContextCache = new DslCompilerContextCache(storage);
            }
        }

        /// <summary>
        /// Create a new instance of this DSL type.
        /// </summary>
        /// <param name="type">The type to create</param>
        /// <param name="parametersForConstructor">optional ctor paraemters</param>
        /// <returns>The newly created instance</returns>
        public virtual object CreateInstance(Type type, params object[] parametersForConstructor)
        {
            return Activator.CreateInstance(type, parametersForConstructor);
        }

        /// <summary>
        /// Compile the DSL and return the resulting context
        /// </summary>
        /// <param name="urls">The files to compile</param>
        /// <returns>The resulting compiler context</returns>
        public virtual CompilerContext Compile(params string[] urls)
        {
            return CompilerContextCache.GetCached(this, urls);
        }

        /// <summary>
        /// Compiler context cache
        /// </summary>
        public DslCompilerContextCache CompilerContextCache 
		{ 
			get { return compilerContextCache; }
			set { compilerContextCache = value; }
		}

		/// <summary>
		/// Force a compile with no caching
		/// </summary>
		/// <param name="urls">The urls.</param>
		/// <param name="cacheFileName">Name of the cache file.</param>
		/// <returns></returns>
        public virtual CompilerContext ForceCompile(string[] urls, string cacheFileName)
        {
            BooCompiler compiler = new BooCompiler();
            compiler.Parameters.OutputType = CompilerOutputType;
            compiler.Parameters.GenerateInMemory = true;
            compiler.Parameters.Pipeline = new CompileToFile();
        	compiler.Parameters.OutputAssembly = cacheFileName;
			CustomizeCompiler(compiler, compiler.Parameters.Pipeline, urls);
            AddInputs(compiler, urls);
            CompilerContext compilerContext = compiler.Run();
            if (compilerContext.Errors.Count != 0)
                throw CreateCompilerException(compilerContext);
            HandleWarnings(compilerContext.Warnings);

            return compilerContext;
        }

        /// <summary>
        /// Allow a derived class to get access to the warnings that occured during 
        /// compilation
        /// </summary>
        /// <param name="warnings">The warnings.</param>
        protected virtual void HandleWarnings(CompilerWarningCollection warnings)
        {
        }

        /// <summary>
        /// Create an exception that would be raised on compilation errors.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual Exception CreateCompilerException(CompilerContext context)
        {
            return new CompilerError(context.Errors.ToString(true));
        }


        private void AddInputs(BooCompiler compiler, IEnumerable<string> urls)
        {
            foreach (string url in urls)
            {
                ICompilerInput input = Storage.CreateInput(url);
                if (input == null)
                    throw new InvalidOperationException("Got a null input for url: " + url);
                compiler.Parameters.Input.Add(input);
            }
        }

        /// <summary>
        /// Customise the compiler to fit this DSL engine.
        /// This is the most commonly overriden method.
        /// </summary>
        protected virtual void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, string[] urls)
        {
        }

        ///<summary>
        /// The type of assembly compilation should produce
        ///</summary>
        public virtual CompilerOutputType CompilerOutputType
        {
            get { return CompilerOutputType.Library; }
        }


        /// <summary>
        /// Get a type from the assembly according to the URL.
        /// This is used to match a class with its originating file
        /// </summary>
        public virtual Type GetTypeForUrl(Assembly assembly, string url)
        {
            string className = Storage.GetTypeNameFromUrl(url);
            return assembly.GetType(className, false, true);
        }

		/// <summary>
		/// Canonizes the URL to a consistent form.
		/// </summary>
    	public virtual string CanonizeUrl(string url)
    	{
    		return Path.GetFullPath(url);
    	}
    }
}
