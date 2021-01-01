using System;

namespace Suspension
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class SuspendableAttribute : Attribute
    {
    }
}