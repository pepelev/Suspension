using System;

namespace Suspension.Tests
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SuspendAttribute : Attribute
    {
    }
}