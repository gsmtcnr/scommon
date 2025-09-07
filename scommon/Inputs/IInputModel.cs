namespace scommon
{
    public interface IInputModel
    {
        string SearchText { get; set; }
    }

    public class InputModel : BaseInputModel
    {
        public string? SearchText { get; set; }
    }
}
