namespace SPS_Code.Controllers.RequestModels
{
    public class TaskCreateRequest : RequestBase
    {
        public IFormFile Generator { get; set; }
        public IFormFile Validator { get; set; }
        public IFormFile Description { get; set; }
        public string Name { get; set; }
        public int MaxPoints { get; set; }
    }
}
