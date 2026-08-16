namespace Jots.Api;

using Models;

public interface IRepository
{
    Jot[] GetAll();
    void Add(Jot jot);
    Jot? GetById(Guid id);
    bool Delete(Guid id);
}
