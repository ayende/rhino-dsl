namespace Rhino.DSL
{
    using Boo.Lang.Compiler;
    using Boo.Lang.Compiler.Ast;

    /// <summary>
    /// This class allows to easily define property generation macros
    /// </summary>
    public abstract class GeneratePropertyMacro : AbstractAstMacro
    {
        private readonly string propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneratePropertyMacro"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public GeneratePropertyMacro(string propertyName)
        {
            this.propertyName = propertyName;
        }

        /// <summary>
        /// Expands the specified macro.
        /// </summary>
        /// <param name="macro">The macro.</param>
        /// <returns></returns>
        public override Statement Expand(MacroStatement macro)
        {
            Property property = new Property(propertyName);
            property.LexicalInfo = macro.LexicalInfo;
            property.Getter = new Method();
            if(macro.Arguments.Count==1)
            {
                property.Getter.Body.Add(
                    new ReturnStatement(macro.Arguments[0])
                    );
            }
            else if(
                macro.Arguments.Count == 0 &&
                macro.Block != null && 
                macro.Block.HasStatements)//use the macro block
            {
                property.Getter.Body = macro.Block;
            }
            else
            {
                CompilerErrorFactory.CustomError(macro.LexicalInfo,
                                                 macro.Name + " must have a single expression argument or a block");
                return null;
            }

            ClassDefinition clazz = (ClassDefinition) macro.GetAncestor(NodeType.ClassDefinition);
            clazz.Members.Add(property);

            return null;
        }
    }
}