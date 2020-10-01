using System;

namespace CompileTimeExecution
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = false)]
    public sealed class CompileTimeExecutionAttribute : Attribute
    {
        readonly bool deserialize;
        public bool Deserialize => deserialize;
        public CompileTimeExecutionAttribute(bool deserialize = false) {
            this.deserialize = deserialize;
        }
    }
}
