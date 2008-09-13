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

	/// <summary>
	/// Provide cache support for a DSL
	/// </summary>
	public class DefaultDslEngineCache : AbstractLockable, IDslEngineCache
	{
		private readonly IDictionary<string, Type> urlToTypeCache = new Dictionary<string, Type>();

		#region IDslEngineCache Members

		/// <summary>
		/// Try to get a cached type for this URL.
		/// </summary>
		/// <param name="url">The url to use as a key for the cache</param>
		/// <returns>The compiled DSL or null if not found</returns>
		public virtual Type Get(string url)
		{
			Type result = null;
			ReadLock(delegate
			{
				urlToTypeCache.TryGetValue(url, out result);				
			});
			return result;
		}

		/// <summary>
		/// Put the type in the cache, with the url as the key
		/// </summary>
		public virtual void Set(string url, Type type)
		{
			WriteLock(delegate
			{
				urlToTypeCache[url] = type;				
			});
		}

		/// <summary>
		/// Removes the url for the from cache.
		/// </summary>
		/// <param name="url">The URL.</param>
		public virtual void Remove(string url)
		{
			WriteLock(delegate
			{
				urlToTypeCache.Remove(url);
			});
		}

		#endregion
	}
}