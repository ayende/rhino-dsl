namespace Rhino.DSL
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Boo.Lang.Compiler;
    using Boo.Lang.Compiler.IO;

    /// <summary>
    /// This class handles the storage requirements for the DSL.
    /// Where the files are located, when they are refreshed, etc
    /// </summary>
    public class DslEngineStorage : IDisposable 
    {
        private readonly Dictionary<string, FileSystemWatcher> pathToFileWatchers = new Dictionary<string, FileSystemWatcher>();

        /// <summary>
        /// Create a compiler input from the URL.
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The compiler input</returns>
        protected virtual ICompilerInput CreateInput(string url)
        {
            return new FileInput(url);
        }

        /// <summary>
        /// The file name format of this DSL
        /// </summary>
        public virtual string FileNameFormat
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
        public virtual string[] GetMatchingUrlsIn(string parentPath, string url)
        {
            url = Path.GetFullPath(Path.Combine(parentPath, url));
            if (Directory.Exists(url) == false)
                url = Path.GetDirectoryName(url);
            List<string> urls = new List<string>();
            foreach (string file in Directory.GetFiles(url, FileNameFormat))
            {
                urls.Add(file);
            }
            urls.Sort(CompareUrls);
            return urls.ToArray();
        }

        /// <summary>
        /// Compares the two urls
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        protected virtual int CompareUrls(string x, string y)
        {
            return x.CompareTo(y);
        }

        /// <summary>
        /// Will call the action delegate when any of the specified urls are changed.
        /// Note that for a single logical change several calls may be made.
        /// </summary>
        /// <param name="urls">The urls.</param>
        /// <param name="action">The action.</param>
        public virtual void NotifyOnChange(IEnumerable<string> urls, Action<string> action)
        {
            lock (pathToFileWatchers)
            {
                string[] commonPaths = GatherCommonPaths(urls);
                foreach (string path in commonPaths)
                {
                    FileSystemWatcher watcher;
                    if(pathToFileWatchers.TryGetValue(path, out watcher)==false)
                    {
                        pathToFileWatchers[path] = watcher = new FileSystemWatcher(path, FileNameFormat);
                        watcher.EnableRaisingEvents = true;
                    }
                    watcher.Changed += delegate(object sender, FileSystemEventArgs e)
                    {
                        action(e.FullPath);
                    };
                }
            }
        }

        private static string[] GatherCommonPaths(IEnumerable<string> urls)
        {
            List<string> paths = new List<string>();
            foreach (string url in urls)
            {
                string path = Path.GetDirectoryName(url);
                if(paths.Contains(path)==false)
                    paths.Add(path);
            }
            return paths.ToArray();
        }

        /// <summary>
        /// Determines whether the URL is included in the specified urls
        /// in the given parent path
        /// </summary>
        /// <param name="urls">The urls.</param>
        /// <param name="parentOath">The parent oath.</param>
        /// <param name="url">The URL.</param>
        /// <returns>
        /// 	<c>true</c> if [is URL include in] [the specified urls]; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsUrlIncludeIn(string[] urls, string parentOath, string url)
        {
            string path = Path.GetFullPath(Path.Combine(parentOath, url));
            return Array.Exists(urls, delegate(string urlInArray)
            {
                return urlInArray.Equals(path, StringComparison.InvariantCultureIgnoreCase);
            });
        }


        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        ///<filterpriority>2</filterpriority>
        public virtual void Dispose()
        {
            lock (pathToFileWatchers)
            {
                foreach (FileSystemWatcher watcher in pathToFileWatchers.Values)
                {
                    watcher.EnableRaisingEvents = false;
                    watcher.Dispose();
                }
            }
        }
    }
}