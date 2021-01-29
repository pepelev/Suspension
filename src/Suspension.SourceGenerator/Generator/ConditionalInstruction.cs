using System.Collections.Generic;

namespace Suspension.SourceGenerator.Generator
{
    public sealed class ConditionalInstruction : Instruction
    {
        private readonly Instruction condition;
        private readonly IReadOnlyList<Instruction> @false;
        private readonly IReadOnlyList<Instruction> @true;

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
}