namespace Suspension.SourceGenerator.Generator
{
    public abstract class Instruction
    {
        public abstract string AsString();
        public override string ToString() => AsString();
    }
}