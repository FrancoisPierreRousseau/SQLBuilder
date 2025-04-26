namespace SQLBuilder.Repositories;
public interface IRepository<T> where T : class, new()
{
    int Insert(T entity);
    void Update(T entity);
    void Delete(object id);
    T GetById(object id);
    List<T> GetAll();
}
