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

	public delegate TextReader UrlResolverDelegate(string url, string basePath);

	[CLSCompliant(false)]
	public class AutoReferenceFilesCompilerStep : AbstractTransformerCompilerStep
	{
		private readonly UrlResolverDelegate urlResolver;
		private readonly Dictionary<string, Assembly> assemblyCache = new Dictionary<string, Assembly>();

		private readonly string baseDirectory;

		public AutoReferenceFilesCompilerStep()
			: this((string)null)
		{	
		}

		public AutoReferenceFilesCompilerStep(string baseDirectory)
			: this(baseDirectory, ResolveFile)
		{
		}

		public AutoReferenceFilesCompilerStep(UrlResolverDelegate urlResolver)
			: this(null, urlResolver)
		{
		}

		public AutoReferenceFilesCompilerStep(string baseDirectory, UrlResolverDelegate urlResolver)
		{
			if (urlResolver == null)
			{
				throw new ArgumentNullException("urlResolver");	
			}

			this.baseDirectory = baseDirectory;
			this.urlResolver = urlResolver;
		}

		public override void OnImport(Import node)
		{
			if (node.Namespace != "file")
				return;

			RemoveCurrentNode();

			//we may need to preserve this, since it may be used in several compiler cycles.
			//which will set them to different things
			CompilerErrorCollection errors = Errors;
			AssemblyCollection references = Parameters.References;
			string url = node.AssemblyReference.Name
				.Replace("~", AppDomain.CurrentDomain.BaseDirectory);

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

		private static TextReader ResolveFile(string url, string basePath)
		{
			string path = Path.Combine(basePath, url);
			return new StreamReader(path);
		}

		private Assembly CompileAssembly(Node node, string url, CompilerErrorCollection errors)
		{
			CompilerContext result = Compile(url);
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
			TextReader input = urlResolver(url, baseDirectory);
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
			cloned.MaxAttributeSteps = parameters.MaxAttributeSteps;
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

		public override void Run()
		{
			Visit(CompileUnit);
		}
	}
}