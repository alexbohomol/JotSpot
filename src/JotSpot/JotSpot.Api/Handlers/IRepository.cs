using JotSpot.Api.Models;

namespace JotSpot.Api.Handlers;

public interface IRepository
{
    Jot[] GetAll();
    void Add(Jot jot);
    Jot? GetById(Guid id);
    bool Delete(Guid id);
}