namespace Suspension.Tests.Samples
{
    public sealed partial class DiscardAssignment
    {
        [Suspendable]
        public static void Variable()
        {
            _ = "asd";
        }

        [Suspendable]
        public static void OutParameter()
        {
            int.TryParse("42", out _);
        }
    }
}