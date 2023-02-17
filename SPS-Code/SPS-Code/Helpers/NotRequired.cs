namespace SPS_Code.Helpers
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class NotRequired : Attribute
    {
        public NotRequired() { }
    }
}
