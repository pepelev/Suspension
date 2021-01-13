using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

namespace Suspension.SourceGenerator
{
    public sealed class V : OperationVisitor<None, IEnumerable<string>>
    {
        public override IEnumerable<string> DefaultVisit(IOperation operation, None argument)
        {
            throw new Exception($"V Visit failed {operation}");
        }

        public override IEnumerable<string> VisitInvocation(IInvocationOperation operation, None argument) =>
            from child in operation.Arguments.Append(operation.Instance)
            from result in child.Accept(this, argument)
            select result;

        public override IEnumerable<string> VisitExpressionStatement(IExpressionStatementOperation operation, None argument)
        {
            return operation.Operation.Accept(this, argument);
        }

        public override IEnumerable<string> VisitArgument(IArgumentOperation operation, None argument)
        {
            return operation.Value.Accept(this, argument);
        }

        public override IEnumerable<string> VisitLiteral(ILiteralOperation operation, None argument)
        {
            return Array.Empty<string>();
        }

        public override IEnumerable<string> VisitParameterReference(IParameterReferenceOperation operation, None argument)
        {
            var parameter = operation.Parameter;
            return new[] { $"parameter: {parameter.Type} {parameter.Name}" };
        }

        public override IEnumerable<string> VisitSimpleAssignment(ISimpleAssignmentOperation operation, None argument)
        {
            return Array.Empty<string>();
        }

        public override IEnumerable<string> VisitLocalReference(ILocalReferenceOperation operation, None argument)
        {
            var local = operation.Local;
            return new[] { $"local: {local.Type} {local.Name}" };
        }
    }
}