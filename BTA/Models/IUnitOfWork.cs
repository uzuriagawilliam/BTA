namespace BTA.Models
{
    public interface IUnitOfWork
    {
        IBookRepository BookRepository { get; }
    }
}
