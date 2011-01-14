using System.Linq;
using Hammock.Framework.Specifications;

namespace Hammock.Framework.DataAccess
{
    public interface IRepository<T> : IQueryable<T>
        where T : class
    {
        IQueryable<T> Satisfying<K>() where K : class, ISpecification<T>;
    }

    public interface IRepository : IQueryable
    {
        IQueryable<T> Satisfying<T, K>()
            where T : class
            where K : class, ISpecification<T>;
    }
}