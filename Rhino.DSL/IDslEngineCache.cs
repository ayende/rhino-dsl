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

	/// <summary>
	/// An action delegate to be executed under a cache lock
	/// </summary>
	public delegate void CacheAction();

	/// <summary>
	/// Cahce interface for the DslEngine
	/// </summary>
	public interface IDslEngineCache
	{
		/// <summary>
		/// Try to get a cached type for this URL.
		/// </summary>
		/// <param name="url">The url to use as a key for the cache</param>
		/// <returns>The compiled DSL or null if not found</returns>
		Type Get(string url);

		/// <summary>
		/// Put the type in the cache, with the url as the key
		/// </summary>
		void Set(string url, Type type);

		/// <summary>
		/// Removes the url for the from cache.
		/// </summary>
		/// <param name="url">The URL.</param>
		void Remove(string url);

		/// <summary>
		/// Execute the action under a write lock
		/// </summary>
		/// <param name="cacheAction">The cache action.</param>
		void WriteLock(CacheAction cacheAction);

		/// <summary>
		/// Execute the action under a read lock
		/// </summary>
		/// <param name="cacheAction">The cache action.</param>
		void ReadLock(CacheAction cacheAction);
	}
}