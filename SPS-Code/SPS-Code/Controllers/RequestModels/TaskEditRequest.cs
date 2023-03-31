using SPS_Code.Helpers;

namespace SPS_Code.Controllers.RequestModels
{
    public class TaskEditRequest : TaskCreateRequest
    {
        [NotRequired]
        public int Id { get; set; }
    }
}
