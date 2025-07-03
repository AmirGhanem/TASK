namespace Domain.Contractors
{
    public interface IModelBase<T>
    {
        T Id { get; set; }
    }
}