namespace DefaultNamespace;

public interface IRepository
{
    public T GetById(int Id)

    public void Create(T entity)

    public void Update(int Id)
    
    public void Delete(int Id)
}