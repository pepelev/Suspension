using Microsoft.CodeAnalysis;

namespace Suspension.SourceGenerator.Domain
{
    internal abstract class Coroutine
    {
        public abstract SyntaxTree Document { get; }
    }
}