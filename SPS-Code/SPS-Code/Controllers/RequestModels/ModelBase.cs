using SPS_Code.Helpers;

namespace SPS_Code.Controllers.RequestModels
{
    public class ModelBase
    {
        [NotRequired]
        public string Message { get; set; }
        public ModelBase SetError(string? error = null)
        {
            Message = error;
            return this;
        }
    }
}
