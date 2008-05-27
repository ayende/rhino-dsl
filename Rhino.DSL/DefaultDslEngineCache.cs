
namespace Rhino.DSL
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	/// <summary>
	/// Provide cache support for a DSL
	/// </summary>
	public class DefaultDslEngineCache : IDslEngineCache
	{
		private readonly ReaderWriterLock readerWriterLock = new ReaderWriterLock();
		private readonly IDictionary<string, Type> urlToTypeCache = new Dictionary<string, Type>();

		/// <summary>
		/// Try to get a cached type for this URL.
		/// </summary>
		/// <param name="url">The url to use as a key for the cache</param>
		/// <returns>The compiled DSL or null if not found</returns>
		public virtual Type Get(string url)
		{
			Type result;
			urlToTypeCache.TryGetValue(url, out result);
			return result;
		}

		/// <summary>
		/// Put the type in the cache, with the url as the key
		/// </summary>
		public virtual void Set(string url, Type type)
		{
			urlToTypeCache[url] = type;
		}

		/// <summary>
		/// Removes the url for the from cache.
		/// </summary>
		/// <param name="url">The URL.</param>
		public virtual void Remove(string url)
		{
			urlToTypeCache.Remove(url);
		}

		/// <summary>
		/// Execute the action under a write lock
		/// </summary>
		/// <param name="cacheAction">The cache action.</param>
		public void WriteLock(CacheAction cacheAction)
		{
			bool readerLockHeld = readerWriterLock.IsReaderLockHeld;
			LockCookie writerLock = new LockCookie();
			if (readerLockHeld)
			{
				writerLock = readerWriterLock.UpgradeToWriterLock(Timeout.Infinite);
			}
			else
				readerWriterLock.AcquireWriterLock(Timeout.Infinite);
			try
			{
				cacheAction();
			}
			finally
			{
				if (readerLockHeld)
					readerWriterLock.DowngradeFromWriterLock(ref writerLock);
				else
					readerWriterLock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Execute the action under a read lock
		/// </summary>
		/// <param name="cacheAction">The cache action.</param>
		public void ReadLock(CacheAction cacheAction)
		{
			readerWriterLock.AcquireReaderLock(Timeout.Infinite);
			try
			{
				cacheAction();
			}
			finally
			{
				readerWriterLock.ReleaseReaderLock();
			}
		}
	}
}