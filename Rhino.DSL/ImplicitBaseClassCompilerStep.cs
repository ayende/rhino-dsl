namespace Rhino.DSL
{
    using System;
    using Boo.Lang.Compiler.Ast;
    using Module=Boo.Lang.Compiler.Ast.Module;

    /// <summary>
    /// Takes all the code that exists in a module's global section and put it in a specificied
    /// method of a class. Allow easy handling of the Anonymous Base Class pattern 
    /// </summary>
    public class ImplicitBaseClassCompilerStep : BaseClassCompilerStep
    {
        private readonly string methodName;
        private readonly ParameterDeclarationCollection parameters;

        /// <summary>
        /// Create new instance of <seealso cref="ImplicitBaseClassCompilerStep"/>
        /// </summary>
        /// <param name="baseClass">The base class that will be used</param>
        /// <param name="methodName">The name of the method that will get all the code from globals moved to it.</param>
        /// <param name="namespaces">Namespaces that would be automatically imported into all modules</param>
        public ImplicitBaseClassCompilerStep(Type baseClass, string methodName, params string[] namespaces)
            : this(baseClass, methodName, null, namespaces)
        {            
        }

        /// <summary>
        /// Create new instance of <seealso cref="ImplicitBaseClassCompilerStep"/>
        /// </summary>
        /// <param name="baseClass">The base class that will be used</param>
        /// <param name="methodName">The name of the method that will get all the code from globals moved to it.</param>
        /// <param name="parameters">The parameters of this method</param>        
        /// <param name="namespaces">Namespaces that would be automatically imported into all modules</param>               
        public ImplicitBaseClassCompilerStep(Type baseClass, string methodName, ParameterDeclarationCollection parameters, params string[] namespaces)
			: base(baseClass, namespaces)
        {
            this.methodName = methodName;
            this.parameters = parameters;
        }

        /// <summary>
        /// Allow a derived class to perform additional operations on the newly created type definition.
        /// </summary>
        protected override void ExtendBaseClass(Module module, ClassDefinition definition)
        {
			Method method = new Method(methodName);


			if (parameters != null)
			{
				foreach (ParameterDeclaration parameter in parameters)
				{
					method.Parameters.Add(parameter);
				}
			}

			method.Body = module.Globals;
			definition.Members.Add(method);

			ExtendBaseClass(definition);
        }

		/// <summary>
		/// Allow to extend the base class in additional ways
		/// </summary>
		/// <param name="definition">The definition.</param>
		protected virtual void ExtendBaseClass(TypeDefinition definition)
		{
			
		}
    }
}
