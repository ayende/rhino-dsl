namespace Rhino.DSL
{
    using Boo.Lang.Compiler.Ast;
    using Boo.Lang.Compiler.Steps;

    /// <summary>
    /// Automatically imports all the specified namespaces to all the modules
    /// in the compilation unit
    /// </summary>
    /// <remarks>
    /// This probably belongs early in the compilation process
    /// </remarks>
    public class AutoImportCompilerStep : AbstractTransformerCompilerStep
    {
        private readonly string[] namespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoImportCompilerStep"/> class.
        /// </summary>
        /// <param name="namespaces">The namespaces.</param>
        public AutoImportCompilerStep(params string[] namespaces)
        {
            this.namespaces = namespaces;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public override void Run()
        {
            Visit(CompileUnit);
        }

        /// <summary>
        /// Add the namespaces to the module
        /// </summary>
        public override void OnModule(Module node)
        {
            foreach (string ns in namespaces)
            {
                Import import = new Import(node.LexicalInfo, ns);
                node.Imports.Add(import);
            }
        }
    }
}