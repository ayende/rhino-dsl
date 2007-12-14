namespace Rhino.DSL
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using antlr;
    using Boo.Lang.Compiler;
    using Boo.Lang.Compiler.IO;
    using Boo.Lang.Compiler.Pipelines;

    public abstract class DslEngine
    {
        private readonly IDictionary<Uri, Type> urlToTypeCache = new Dictionary<Uri, Type>();

        public virtual Type GetFromCache(Uri url)
        {
            Type result;
            urlToTypeCache.TryGetValue(url, out result);
            return result;
        }

        public virtual object CreateInstance(Type type, params object[] parametersForConstructor)
        {
            return Activator.CreateInstance(type, parametersForConstructor);
        }

        public virtual CompilerContext Compile(params Uri[] urls)
        {
            BooCompiler compiler = new BooCompiler();
            compiler.Parameters.OutputType = CompilerOutputType;
            compiler.Parameters.GenerateInMemory = true;
            compiler.Parameters.Pipeline = new CompileToMemory();
            CustomizeCompiler(compiler, compiler.Parameters.Pipeline, urls);
            AddInputs(compiler, urls);
            CompilerContext compilerContext = compiler.Run();
            if (compilerContext.Errors.Count != 0)
                throw CreateCompilerException(compilerContext);
            return compilerContext;
        }

        protected virtual Exception CreateCompilerException(CompilerContext context)
        {
            return context.Errors[0];
        }

        private void AddInputs(BooCompiler compiler, IEnumerable<Uri> urls)
        {
            foreach (Uri url in urls)
            {
                compiler.Parameters.Input.Add(CreateInput(url));
            }
        }

        protected virtual FileInput CreateInput(Uri url)
        {
            return new FileInput(url.AbsolutePath);
        }

        protected virtual void CustomizeCompiler(BooCompiler compiler, CompilerPipeline pipeline, Uri[] urls)
        {
        }

        public virtual CompilerOutputType CompilerOutputType
        {
            get { return CompilerOutputType.Library; }
        }

        public virtual Type GetTypeForUrl(Assembly assembly, Uri url)
        {
            string className = Path.GetFileNameWithoutExtension(url.AbsolutePath);
            return assembly.GetType(className, false, true);
        }

        public virtual void SetInCache(Uri url, Type type)
        {
            urlToTypeCache[url] = type;
        }

        public virtual string FileFormat
        {
            get
            {
                return "*.boo";
            }
        }

        /// <summary>
        /// Will retrieve all the _canonised_ urls from the given directory that
        /// this Dsl Engine can process.
        /// </summary>
        public virtual Uri[] GetMatchingUrlsIn(string directory)
        {
            List<Uri> urls = new List<Uri>();
            foreach (string url in Directory.GetFiles(directory, FileFormat))
            {
                urls.Add(new Uri(url));
            }
            return urls.ToArray();
        }
    }
}