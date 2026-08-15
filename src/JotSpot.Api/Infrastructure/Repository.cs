using JotSpot.Api.Models;

namespace JotSpot.Api.Infrastructure;

// TODO: TBD - implement later when introducing real storage
public class Repository : IRepository
{
    public Jot[] GetAll()
    {
        throw new NotImplementedException();
    }

    public void Add(Jot jot)
    {
        throw new NotImplementedException();
    }

    public Jot? GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public bool Delete(Guid id)
    {
        throw new NotImplementedException();
    }
}
