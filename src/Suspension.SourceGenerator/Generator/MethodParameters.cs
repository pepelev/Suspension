using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Suspension.SourceGenerator.Generator
{
    public sealed class MethodParameters : OperationVisitor<None, IEnumerable<(string Type, string Name)>>
    {
        public override IEnumerable<(string Type, string Name)> DefaultVisit(IOperation operation, None argument)
        {
            throw new Exception($"V Visit failed {operation}");
        }

        public override IEnumerable<(string Type, string Name)> VisitInvocation(IInvocationOperation operation, None argument) =>
            from child in operation.Arguments.Append(operation.Instance)
            from result in child.Accept(this, argument)
            select result;

        public override IEnumerable<(string Type, string Name)> VisitExpressionStatement(IExpressionStatementOperation operation, None argument)
        {
            return operation.Operation.Accept(this, argument);
        }

        public override IEnumerable<(string Type, string Name)> VisitArgument(IArgumentOperation operation, None argument)
        {
            return operation.Value.Accept(this, argument);
        }

        public override IEnumerable<(string Type, string Name)> VisitLiteral(ILiteralOperation operation, None argument)
        {
            return Array.Empty<(string Type, string Name)>();
        }

        public override IEnumerable<(string Type, string Name)> VisitParameterReference(IParameterReferenceOperation operation, None argument)
        {
            var parameter = operation.Parameter;
            return new[]
            {
                (parameter.Type.ToString(), parameter.Name)
            };
        }

        public override IEnumerable<(string Type, string Name)> VisitSimpleAssignment(ISimpleAssignmentOperation operation, None argument)
        {
            return Array.Empty<(string Type, string Name)>();
        }

        public override IEnumerable<(string Type, string Name)> VisitLocalReference(ILocalReferenceOperation operation, None argument)
        {
            return Array.Empty<(string Type, string Name)>();
        }
    }
}