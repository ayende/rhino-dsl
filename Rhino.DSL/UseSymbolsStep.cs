namespace Rhino.DSL
{
    using Boo.Lang.Compiler.Ast;
    using Boo.Lang.Compiler.Steps;

    /// <summary>
    /// Translate all @foo reference expression to "foo" string literals.
    /// In essense, add symbols to the language
    /// </summary>
    public class UseSymbolsStep : AbstractTransformerCompilerStep
    {
        /// <summary>
        /// Runs this instance.
        /// </summary>
        public override void Run()
        {
            Visit(CompileUnit);
        }

        /// <summary>
        /// Called when visting a reference expression.
        /// Will turn reference expressions with initial @ to string literals
        /// </summary>
        /// <param name="node">The node.</param>
        public override void OnReferenceExpression(Boo.Lang.Compiler.Ast.ReferenceExpression node)
        {
            if(node.Name.StartsWith("@")==false)
                return;

            ReplaceCurrentNode(new StringLiteralExpression(node.Name.Substring(1)));
        }
    }
}