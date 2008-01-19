namespace Rhino.DSL
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Boo.Lang.Compiler.Ast;
	using Boo.Lang.Compiler.Steps;
	using Module=Boo.Lang.Compiler.Ast.Module;

    /// <summary>
    /// Takes all the code that exists in a module's global section and put it in a specificied
    /// method of a class. Allow easy handling of the Anonymous Base Class pattern 
    /// </summary>
	public class AnonymousBaseClassCompilerStep : AbstractCompilerStep
	{
		private readonly string[] namespaces;
		private readonly Type baseClass;
		private readonly string methodName;


        /// <summary>
        /// Create new instance of <seealso cref="AnonymousBaseClassCompilerStep"/>
        /// </summary>
        /// <param name="baseClass">The base class that will be used</param>
        /// <param name="methodName">The name of the method that will get all the code from globals moved to it.</param>
        /// <param name="namespaces">Namespaces that would be automatically imported into all modules</param>
		public AnonymousBaseClassCompilerStep(Type baseClass, string methodName, params string[] namespaces)
		{
			this.namespaces = namespaces;
			this.baseClass = baseClass;
			this.methodName = methodName;
		}

		///<summary>
		/// Run this compiler step
		///</summary>
		public override void Run()
		{
			if (Context.References.Contains(baseClass.Assembly) == false)
				Context.Parameters.References.Add(baseClass.Assembly);

			foreach (Module module in CompileUnit.Modules)
			{
				foreach (string ns in this.namespaces)
				{
					module.Imports.Add(new Import(module.LexicalInfo, ns));
				}

				ClassDefinition definition = new ClassDefinition();
				definition.Name = module.FullName;
				definition.BaseTypes.Add(new SimpleTypeReference(baseClass.FullName));
				Method method = new Method(methodName);
				method.Body = module.Globals;
				module.Globals = new Block();
				definition.Members.Add(method);
				module.Members.Add(definition);

				ExtendBaseClass(definition);
			}
		}

        /// <summary>
        /// Allow a derived class to perform additional operations on the newly created type definition.
        /// </summary>
		protected virtual void ExtendBaseClass(TypeDefinition definition)
		{

		}
	}
}