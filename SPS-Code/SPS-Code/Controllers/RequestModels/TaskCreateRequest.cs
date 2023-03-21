namespace SPS_Code.Controllers.RequestModels
{
    public class TaskCreateRequest : RequestBase
    {
        public IFormFile Generator { get; set; }
        public IFormFile Validator { get; set; }
        public string Description { get; set; }
        public string Inputs { get; set; }
        public string Outputs { get; set; }
        public string Name { get; set; }
        public int MaxPoints { get; set; }
        public int MaxSubmitTimeMinutes { get; set; }
        public int TestCount { get; set; }
    }
}
