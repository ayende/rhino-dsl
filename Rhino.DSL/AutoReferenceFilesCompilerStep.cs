#region license
// Copyright (c) 2005 - 2007 Ayende Rahien (ayende@ayende.com)
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//     this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice,
//     this list of conditions and the following disclaimer in the documentation
//     and/or other materials provided with the distribution.
//     * Neither the name of Ayende Rahien nor the names of its
//     contributors may be used to endorse or promote products derived from this
//     software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
// THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

namespace Rhino.DSL
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Boo.Lang.Compiler;
	using Boo.Lang.Compiler.Ast;
	using Boo.Lang.Compiler.IO;
	using Boo.Lang.Compiler.MetaProgramming;
	using Boo.Lang.Compiler.Steps;
	using Boo.Lang.Parser;
	using Module=Boo.Lang.Compiler.Ast.Module;

    /// <summary>
    /// This delegate is used to abstract getting the data from a url.
    /// This allows us to plug in more smarts when needed (for instance, hooking into the 
    /// Castle.Resources system)
    /// </summary>
	public delegate TextReader UrlResolverDelegate(string url, string basePath);

    /// <summary>
    /// This compiler step will auotmatically compiler and reference all files specified in import clauses such as:
    /// import file from "[file name]"
    /// 
    /// Another option is:
    /// import namespaces from "[file name]"
    /// 
    /// In which case all the namespaces defined in that file will be imported into the current file
    /// </summary>
	public class AutoReferenceFilesCompilerStep : AbstractTransformerCompilerStep
	{
		private readonly UrlResolverDelegate urlResolver;
		private readonly Dictionary<string, Assembly> assemblyCache = new Dictionary<string, Assembly>();

		private readonly string baseDirectory;

        /// <summary>
        /// Create a new instance of <seealso cref="AutoReferenceFilesCompilerStep"/>
        /// </summary>
		public AutoReferenceFilesCompilerStep()
			: this((string)null)
		{
		}

        /// <summary>
        /// Create a new instance of <seealso cref="AutoReferenceFilesCompilerStep"/>
        /// </summary>
        /// <param name="baseDirectory">The base directory to resolve files from</param>
		public AutoReferenceFilesCompilerStep(string baseDirectory)
			: this(baseDirectory, ResolveFile)
		{
		}

        /// <summary>
        /// Create a new instance of <seealso cref="AutoReferenceFilesCompilerStep"/>
        /// </summary>
        /// <param name="urlResolver">The url resolver to use</param>
		public AutoReferenceFilesCompilerStep(UrlResolverDelegate urlResolver)
			: this(null, urlResolver)
		{
		}

        /// <summary>
        /// Create a new instance of <seealso cref="AutoReferenceFilesCompilerStep"/>
        /// </summary>
        /// <param name="baseDirectory">The base directory to resolve files from</param>
        /// <param name="urlResolver">The url resolver to use</param>
		public AutoReferenceFilesCompilerStep(string baseDirectory, UrlResolverDelegate urlResolver)
		{
			if (urlResolver == null)
			{
				throw new ArgumentNullException("urlResolver");
			}

			this.baseDirectory = baseDirectory;
			this.urlResolver = urlResolver;
		}

        /// <summary>
        /// Add the desired import behavior.
        /// </summary>
		public override void OnImport(Import node)
		{
			if (node.Namespace == "file")
				AddFileReference(node);
			if (node.Namespace == "namespaces")
				AddNamespaceImports(node);
		}

		private void AddNamespaceImports(Import node)
		{
			RemoveCurrentNode();
			
			string url = GetFilePath(node);
			using(TextReader reader = urlResolver(url, baseDirectory))
			{
				BooParsingStep parser = new BooParsingStep();
				CompilerContext context = new CompilerContext();
				StringInput input = new StringInput(node.AssemblyReference.Name, reader.ReadToEnd());
				context.Parameters.Input.Add(input);
				parser.Initialize(context);
				parser.Run();
				Module current = (Module) node.GetAncestor(NodeType.Module);
				foreach (Module module in context.CompileUnit.Modules)
				{
					foreach (Import import in module.Imports)
					{
						current.Imports.Add(import);
					}
				}
			}
		}

		private void AddFileReference(Import node)
		{
			RemoveCurrentNode();

			//we may need to preserve this, since it may be used in several compiler cycles.
			//which will set them to different things
			CompilerErrorCollection errors = Errors;
			CompilerReferenceCollection references = Parameters.References;
			string url = GetFilePath(node);

			Assembly assembly;
			if (assemblyCache.TryGetValue(url, out assembly) == false)
			{
				assembly = CompileAssembly(node, url, errors);

				if (assembly == null)
				{
					throw new CompilationErrorsException(errors);
				}

				assemblyCache.Add(url, assembly);
			}

			references.Add(assembly);
		}

		private static string GetFilePath(Import node)
		{
            // assume this is located relative to the current file
            if (node.LexicalInfo != null &&
                File.Exists(node.LexicalInfo.FullPath))
            {
                string directory = Path.GetDirectoryName(node.LexicalInfo.FullPath);
                return Path.Combine(directory, node.AssemblyReference.Name);
            }

		    return node.AssemblyReference.Name
				.Replace("~", AppDomain.CurrentDomain.BaseDirectory);
		}

		private static TextReader ResolveFile(string url, string basePath)
		{
			string path = Path.Combine(basePath, url);
			return new StreamReader(path);
		}

		private Assembly CompileAssembly(Node node, string url, CompilerErrorCollection errors)
		{
		    CompilerContext oldContext = Context;
		    CompilerContext result = Compile(url);
		    _context = oldContext;
			if (result.Errors.Count > 0)
			{
				errors.Add(new CompilerError(node.LexicalInfo, "Failed to add a file reference"));
				foreach (CompilerError err in result.Errors)
				{
					errors.Add(err);
				}
				return null;
			}
			return result.GeneratedAssembly;
		}

		private CompilerContext Compile(string url)
		{
			TextReader input = urlResolver(url, baseDirectory ?? Path.GetDirectoryName(url));
			CompilerParameters parameters = SafeCloneParameters(Parameters);
			parameters.Input.Add(new ReaderInput(url, input));
			BooCompiler compiler = new BooCompiler(parameters);
			return compiler.Run();
		}

		/// <summary>
		/// This creates a copy of the passed compiler parameters, without the stuff
		/// that make a compilation unique, like input, output assembly, etc
		/// </summary>
		/// <param name="parameters"></param>
		/// <returns></returns>
		private static CompilerParameters SafeCloneParameters(CompilerParameters parameters)
		{
			CompilerParameters cloned = new CompilerParameters();
			cloned.BooAssembly = parameters.BooAssembly;
			cloned.Checked = parameters.Checked;
			cloned.Debug = parameters.Debug;
			cloned.DelaySign = parameters.DelaySign;
			cloned.Ducky = parameters.Ducky;
			cloned.GenerateInMemory = parameters.GenerateInMemory;

			// cloned.Input - we don't want to copy that
			cloned.KeyContainer = parameters.KeyContainer;
			cloned.KeyFile = parameters.KeyFile;
			cloned.LibPaths.AddRange(parameters.LibPaths);
			// cloned.OutputAssembly - we don't want that either

			// always want that, since we are compiling to add a reference
			cloned.OutputType = CompilerOutputType.Library;
			cloned.OutputWriter = parameters.OutputWriter;
			cloned.Pipeline = parameters.Pipeline;
			cloned.References = parameters.References;
			// cloned.Resources - probably won't have that, but in any case, not relevant
			cloned.StdLib = parameters.StdLib;

			return cloned;
		}

        /// <summary>
        /// Run the current compiler step
        /// </summary>
		public override void Run()
		{
			Visit(CompileUnit);
		}
	}
}