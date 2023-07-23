namespace BTA.Models
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BookContext _context;
        public UnitOfWork(BookContext context)
        {
            _context = context;
        }
        public IBookRepository BookRepository => new BookRepository(_context);
    }
}
