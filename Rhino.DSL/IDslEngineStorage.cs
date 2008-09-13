namespace Rhino.DSL
{
    using System;
    using System.Collections.Generic;
    using Boo.Lang.Compiler;

    /// <summary>
    /// Implementors of this class will 
    /// handle all the storage requirements for the DSL.
    /// Where the scripts are located, when they are refreshed, etc
    /// </summary>
    public interface IDslEngineStorage : IDisposable
    {
        /// <summary>
        /// Will retrieve all the _canonised_ urls from the given directory that
        /// this Dsl Engine can process.
        /// </summary>
        string[] GetMatchingUrlsIn(string parentPath, ref string url);

        /// <summary>
        /// Will call the action delegate when any of the specified urls are changed.
        /// Note that for a single logical change several calls may be made.
        /// </summary>
        /// <param name="urls">The urls.</param>
        /// <param name="action">The action.</param>
        void NotifyOnChange(IEnumerable<string> urls, Action<string> action);


        /// <summary>
        /// Create a compiler input from the URL.
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns>The compiler input</returns>
        ICompilerInput CreateInput(string url);

		/// <summary>
		/// Determines whether the URL is included in the specified urls
		/// in the given parent path
		/// </summary>
		/// <param name="urls">The urls.</param>
		/// <param name="parentPath">The parent oath.</param>
		/// <param name="url">The URL.</param>
		/// <returns>
		/// 	<c>true</c> if [is URL include in] [the specified urls]; otherwise, <c>false</c>.
		/// </returns>
        bool IsUrlIncludeIn(string[] urls, string parentPath, string url);

        /// <summary>
        /// Gets the type name from URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        string GetTypeNameFromUrl(string url);
        
        /// <summary>
        /// Determains whatever the given url is a valid script url.
        /// </summary>
        bool IsValidScriptUrl(string url);

		/// <summary>
		/// Given a set of script URLs return a checksum for them taking into account their location and their contents
		/// </summary>
		/// <param name="dslEngineType">Type of the DSL base.</param>
		/// <param name="urls">The urls.</param>
		/// <returns></returns>
        string GetChecksumForUrls(Type dslEngineType, IEnumerable<string> urls);
    }
}
