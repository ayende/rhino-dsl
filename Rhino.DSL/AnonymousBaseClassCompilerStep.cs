namespace Rhino.DSL
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using Boo.Lang.Compiler.Ast;
	using Boo.Lang.Compiler.Steps;
	using Module=Boo.Lang.Compiler.Ast.Module;

	public class AnonymousBaseClassCompilerStep : AbstractCompilerStep
	{
		private readonly string[] namespaces;
		private readonly Type baseClass;
		private readonly string methodName;


		public AnonymousBaseClassCompilerStep(Type baseClass, string methodName, params string[] namespaces)
		{
			this.namespaces = namespaces;
			this.baseClass = baseClass;
			this.methodName = methodName;
		}

		public override void Run()
		{
			if (Context.References.Contains(baseClass.Assembly) == false)
				Context.Parameters.References.Add(baseClass.Assembly);

			AddAssembliesRecursive(new List<Assembly>(), AppDomain.CurrentDomain.GetAssemblies());

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

		private void AddAssembliesRecursive(ICollection<Assembly> visited, IEnumerable<Assembly> assemblies)
		{
			foreach (Assembly assembly in assemblies)
			{
				if (visited.Contains(assembly))
					continue;
				visited.Add(assembly);
				if (Context.References.Contains(assembly) == false)
					Context.Parameters.References.Add(assembly);
				List<Assembly> referenced = new List<Assembly>();
				foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
				{
					referenced.Add(Assembly.Load(assemblyName.FullName));
				}
				AddAssembliesRecursive(visited, referenced);
			}
		}

		protected virtual void ExtendBaseClass(TypeDefinition definition)
		{

		}
	}
}