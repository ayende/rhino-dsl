using System;
using System.Collections.Generic;
using System.Reflection;
using Boo.Lang.Compiler.Ast;
using Module = Boo.Lang.Compiler.Ast.Module;


namespace Rhino.DSL
{
	/// <summary>
	/// Takes all macro statements that exist in the modules global section and puts the bodies of those methods 
	/// into corresponding overridable methods on the base class.
	/// </summary>
	public class MethodSubstitutionBaseClassCompilerStep : BaseClassCompilerStep
	{
		private readonly DepthFirstTransformer[] transformers;

		/// <summary>
		/// Create an instance of MethodSubstitutionBaseClassCompilerStep
		/// </summary>
		public MethodSubstitutionBaseClassCompilerStep(Type baseClassType, params string[] namespaces)
			: base(baseClassType, namespaces)
		{
		}

		/// <summary>
		/// Create an instance of MethodSubstitutionBaseClassCompilerStep
		/// </summary>
		public MethodSubstitutionBaseClassCompilerStep(Type baseClass, DepthFirstTransformer[] transformers, params string[] namespaces)
			: base(baseClass, namespaces)
		{
			this.transformers = transformers;
		}

		/// <summary>
		/// Extends the base class by placing the blocks of macros into methods on the base class
		/// of the same name.
		/// </summary>
		/// <example>
		/// MyMethod:
		///		PerformActions
		/// 
		/// If an overridable method called MyMethod exists on <see cref="BaseClassCompilerStep.BaseClass"/>, then 
		/// it is overridden as follows:
		/// <code>
		/// public override void MyMethod() { PerformActions(); }
		/// </code>
		/// </example>
		protected override void ExtendBaseClass(Module module, ClassDefinition definition)
		{
			List<MethodInfo> methodsThatAreOverridable = new List<MethodInfo>();

			MethodInfo[] baseClassMethods =
				BaseClass.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public |
									 BindingFlags.InvokeMethod);

			foreach (MethodInfo method in baseClassMethods)
			{
				if(method.DeclaringType==typeof(object))
					continue;
				if (method.IsVirtual || method.IsAbstract)
					methodsThatAreOverridable.Add(method);
			}

			MethodSubstitutionTransformer mst = new MethodSubstitutionTransformer(methodsThatAreOverridable.ToArray(), definition);
			mst.Visit(module);

			foreach (Statement statement in module.Globals.Statements)
			{
				ExpressionStatement es = statement as ExpressionStatement;
				if (es != null)
				{
					BinaryExpression be = es.Expression as BinaryExpression;
					if (be != null)
					{
						if (be.Left.NodeType == NodeType.ReferenceExpression && be.Operator == BinaryOperatorType.Assign)
						{
							ReferenceExpression refExp = be.Left as ReferenceExpression;

							Field field = new Field(refExp.LexicalInfo);
							field.Name = refExp.Name;
							field.Initializer = be.Right;
							definition.Members.Add(field);
						}
					}
				}
			}

			if (transformers != null)
			{
				foreach (DepthFirstTransformer transformer in transformers)
					transformer.Visit(module);
			}
		}

		private class MethodSubstitutionTransformer : DepthFirstTransformer
		{
			private readonly ClassDefinition classDefinition;
			private readonly MethodInfo[] methods;

			public MethodSubstitutionTransformer(MethodInfo[] methods, ClassDefinition classDefinition)
			{
				this.classDefinition = classDefinition;
				this.methods = methods;
			}

			public override void OnMacroStatement(MacroStatement node)
			{
				MethodInfo methodToOverride = Array.Find(methods, delegate(MethodInfo mi) { return mi.Name.Equals(node.Name); });

				if (methodToOverride != null)
				{
					Method method = new Method(node.LexicalInfo);
					method.Name = node.Name;
					method.Body = node.Body.CloneNode();

					classDefinition.Members.Add(method);

					RemoveCurrentNode();
				}
				else
				{
					base.OnMacroStatement(node);
				}
			}
		}
	}
}
