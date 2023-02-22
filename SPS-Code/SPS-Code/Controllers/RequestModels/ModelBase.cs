using SPS_Code.Helpers;

namespace SPS_Code.Controllers.RequestModels
{
    public class RequestBase
    {
        [NotRequired]
        public string Message { get; set; }
        public RequestBase SetError(string? error = null)
        {
            Message = error;
            return this;
        }
    }
}
