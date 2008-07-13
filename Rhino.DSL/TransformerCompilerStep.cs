using System;
using System.Collections.Generic;
using System.Text;
using Boo.Lang.Compiler.Ast;
using Boo.Lang.Compiler.Steps;

namespace Rhino.DSL
{
	/// <summary>
	/// Compiler step to run <see cref="DepthFirstTransformer"/>s.
	/// </summary>
	/// <remarks>
	/// This is a useful way to apply one or more transformers as a compiler step.
	/// </remarks>
	public class TransformerCompilerStep : AbstractVisitorCompilerStep
	{
		/// <summary>
		/// Creates an instance of TransformerCompilerStep.
		/// </summary>
		/// <param name="transformers">Transformers for this compiler step to run.</param>
		/// <exception cref="ArgumentNullException"><paramref name="transformers"/> should not be null.</exception>
		/// <exception cref="ArgumentException"><paramref name="transformers"/> should have at least one element.</exception>
		public TransformerCompilerStep(params DepthFirstTransformer[] transformers)
		{
			if (transformers == null)
				throw new ArgumentNullException("transformers");
			if (transformers.Length == 0)
				throw new ArgumentException("Expected at least one element.", "transformers");

			this.transformers = transformers;
		}

		/// <summary>
		/// Run the <see cref="TransformerCompilerStep"/>.
		/// </summary>
		public override void Run()
		{
			foreach (DepthFirstTransformer transformer in transformers)
				transformer.Visit(CompileUnit);
		}

		private readonly DepthFirstTransformer[] transformers;
	}
}
