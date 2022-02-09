namespace myDamco.Models
{
    public class ErrorModel
    {
        public readonly string Title;
        public readonly string Description;
        public readonly string ErrorDescription;

        public ErrorModel(string Title, string Description, string ErrorDescription)
        {
            this.Title = Title;
            this.Description = Description;
            this.ErrorDescription = ErrorDescription;
        }

        public ErrorModel(string Title, string Description) : this(Title, Description, null) { }

    }
}