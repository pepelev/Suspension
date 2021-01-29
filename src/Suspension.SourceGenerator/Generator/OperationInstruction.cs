using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Generator
{
    public sealed class OperationInstruction : Instruction
    {
        private readonly IOperation operation;

        public OperationInstruction(IOperation operation)
        {
            this.operation = operation;
        }

        public override string AsString()
        {
            return operation.Accept(new V2(), new None());
        }
    }
}