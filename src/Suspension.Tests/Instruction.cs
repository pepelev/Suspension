using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Suspension.Tests
{
    public abstract class Instruction
    {
        public abstract string AsString();
        public override string ToString() => AsString();
    }

    public sealed class ConditionalInstruction : Instruction
    {
        private readonly Instruction condition;
        private readonly IReadOnlyList<Instruction> @true;
        private readonly IReadOnlyList<Instruction> @false;

        public ConditionalInstruction(Instruction condition, IReadOnlyList<Instruction> @true, IReadOnlyList<Instruction> @false)
        {
            this.condition = condition;
            this.@true = @true;
            this.@false = @false;
        }

        public override string AsString()
        {
            return
                @$"if ({condition.AsString()})
{{
{string.Join("\n", @true)}
}}
else
{{
{string.Join("\n", @false)}
}}";
        }
    }

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
