namespace FakeExtractor
{
    /// <summary>
    /// Options to run Extractor
    /// </summary>
    public class ExtractorOptions
    {
        /// <summary>
        /// The target that the extractor have to process.
        /// Can be:
        ///         - Folder
        ///         - Solution
        ///         - Project
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Path to target
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Skip folders list like bin, obj, etc.
        /// List with ';' separator
        /// </summary>
        public string SkipFolders { get; set; }

        public string OutputFolder { get; set; }
    }
}