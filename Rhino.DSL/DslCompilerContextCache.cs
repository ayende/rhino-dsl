// Copyright (c) 2005 - 2008 Ayende Rahien (ayende@ayende.com)
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
namespace Rhino.DSL
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Boo.Lang.Compiler;

	/// <summary>
	/// Cache for a CompilerContext instance
	/// </summary>
	public class DslCompilerContextCache : AbstractLockable
	{
		private readonly IDictionary<string, Assembly> assemblyCache = new Dictionary<string, Assembly>();

		private readonly IDslEngineStorage storage;


		/// <summary>
		/// Create a cache based on the script storage method
		/// </summary>
		/// <param name="storage"></param>
		public DslCompilerContextCache(IDslEngineStorage storage)
		{
			this.storage = storage;
		}

		/// <summary>
		/// Returns cached instance if any, or null if none
		/// </summary>
		/// <param name="engine"></param>
		/// <param name="urls"></param>
		/// <returns></returns>
		public CompilerContext GetCached(DslEngine engine, string[] urls)
		{
			if (urls == null || urls.Length == 0) throw new ArgumentNullException("urls");

			string cacheFileName = GetCacheFileName(engine, urls);
			CompilerContext context = LoadCompilerContext(cacheFileName);

			if (context == null)
			{
				context = engine.ForceCompile(urls,cacheFileName);
				WriteLock(delegate
				{
					assemblyCache[cacheFileName] = context.GeneratedAssembly;
					AssemblyLoaded(cacheFileName, context.GeneratedAssembly, false);
				});
			}

			return context;
		}

		private string GetCacheFileName(DslEngine engine, string[] urls)
		{
			string fileName = storage.GetChecksumForUrls(engine.GetType(), urls) + ".boocache";
			return Path.Combine(Path.GetTempPath(), fileName);
		}

		private CompilerContext LoadCompilerContext(string file)
		{
			if (!File.Exists(file)) 
				return null;

			Assembly assembly = null;
			ReadLock(delegate
			{
				if (assemblyCache.TryGetValue(file, out assembly) == false)
				{
					WriteLock(delegate
					{
						byte[] bytes = File.ReadAllBytes(file);
						assembly = Assembly.Load(bytes);
						assemblyCache[file] = assembly;
						AssemblyLoaded(file, assembly, true);
					});
				}
			});
			
			CompilerContext context = new CompilerContext();
			context.GeneratedAssembly = assembly;
			return context;
		}

		/// <summary>
		/// Occurs when an assembly is loaded from disk.
		/// </summary>
		public event AssemblyLoadEvent AssemblyLoaded = delegate {};
	}

	/// <summary>
	/// The event type for loading assembly from disk
	/// </summary>
	public delegate void AssemblyLoadEvent(string filename, Assembly assembly, bool fromCache);
}