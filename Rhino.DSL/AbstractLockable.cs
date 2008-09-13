namespace Rhino.DSL
{
	using System.Threading;

	///<summary>
	/// Provide local read / writer copy semantics
	///</summary>
	public abstract class AbstractLockable
	{
		private readonly ReaderWriterLock readerWriterLock = new ReaderWriterLock();

		/// <summary>
		/// Execute the action under a write lock
		/// </summary>
		/// <param name="cacheAction">The cache action.</param>
		public void WriteLock(CacheAction cacheAction)
		{
			if(readerWriterLock.IsWriterLockHeld)
			{
				cacheAction();
				return;
			}
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
			if(readerWriterLock.IsReaderLockHeld)
			{
				cacheAction();
				return;
			}
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