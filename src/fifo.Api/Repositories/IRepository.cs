namespace fifo.Api.Repositories;

public interface IRepository<T> where T : class
{
    IQueryable<T> Query();
    Task<T?> FindAsync(int id, CancellationToken cancellationToken = default);
    Task<List<T>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Remove(T entity);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
