using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Domain
{
    internal abstract class Output
    {
        public abstract SyntaxTree Document { get; }
    }
}