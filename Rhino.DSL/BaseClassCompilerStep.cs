using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;
using Boo.Lang.Compiler.Util;
using Module=Boo.Lang.Compiler.Ast.Module;

namespace Rhino.DSL
{
	/// <summary>
	/// Provides a base class for classes that provide DSL via
	/// substituting code into one or more members of a class
	/// created at runtime inheriting from a BaseClassCompilerStep.
	/// </summary>
	public abstract class BaseClassCompilerStep: AbstractCompilerStep
	{
		private readonly Type baseClass; 
		private readonly string[] namespaces;

		/// <summary>
		/// Create new instance of <seealso cref="BaseClassCompilerStep"/>
		/// </summary>
		/// <param name="baseClass">The base class that will be used</param>
		/// <param name="namespaces">Namespaces that will be automatically imported into all modules</param>
		protected BaseClassCompilerStep(Type baseClass, params string[] namespaces)
		{
			this.baseClass = baseClass;
			this.namespaces = namespaces;
		}

		/// <summary>
		/// Run this compiler step
		/// </summary>
		public override void Run()
		{
			if (Context.References.Contains(baseClass.Assembly) == false)
                Context.Parameters.References.Add(baseClass.Assembly);

			foreach (Module module in CompileUnit.Modules)
			{
				foreach (string ns in namespaces)
				{
					module.Imports.Add(new Import(module.LexicalInfo, ns));
				}

				ClassDefinition definition = new ClassDefinition();
				definition.Name = module.FullName;
				definition.BaseTypes.Add(new SimpleTypeReference(baseClass.FullName));

				GenerateConstructors(definition);

				// This is called before the module.Globals is set to a new block so that derived classes may retrieve the
				// block from the module.
				ExtendBaseClass(module, definition);

				module.Globals = new Block();
				module.Members.Add(definition);
			}
		}

		/// <summary>
		/// Base class that this BaseClassCompilerStep builds a derived instance of.
		/// </summary>
		protected Type BaseClass
		{
			get { return baseClass; }
		}

		private void GenerateConstructors(TypeDefinition definition)
		{
			ConstructorInfo[] ctors =
				baseClass.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			foreach (ConstructorInfo ctor in ctors)
			{
				if (ctor.IsPrivate)
					continue;
				Constructor constructor = new Constructor(definition.LexicalInfo);
				definition.Members.Add(constructor);
				MethodInvocationExpression super = new MethodInvocationExpression(new SuperLiteralExpression());
				constructor.Body.Add(super);
				foreach (ParameterInfo info in ctor.GetParameters())
				{
					SimpleTypeReference typeReference =
						new SimpleTypeReference(TypeUtilities.GetFullName(info.ParameterType));
					constructor.Parameters.Add(new ParameterDeclaration(info.Name,
																		typeReference)
						);
					super.Arguments.Add(new ReferenceExpression(info.Name));
				}
			}
		}

		/// <summary>
		/// Allow a derived class to perform additional operations on the newly created type definition.
		/// </summary>
		protected virtual void ExtendBaseClass(Module module, ClassDefinition definition)
		{
		}
	}
}
