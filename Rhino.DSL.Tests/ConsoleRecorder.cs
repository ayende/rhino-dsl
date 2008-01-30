namespace Rhino.DSL.Tests
{
    using System;
    using System.IO;
    using MbUnit.Framework;

    public class ConsoleRecorder : IDisposable
    {
        private readonly TextWriter old;
        private readonly StringWriter writer;

        public ConsoleRecorder()
        {
            old = Console.Out;
            writer = new StringWriter();
            Console.SetOut(this.writer);
        }

        public string Result
        {
            get { return writer.GetStringBuilder().ToString(); }
        }

        public void AssertEquals(string expected)
        {
            Assert.AreEqual(expected, Result);
        }

        public void Dispose()
        {
            Console.SetOut(old);
        }
    }
}