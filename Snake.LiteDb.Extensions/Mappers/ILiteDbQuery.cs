using Snake.LiteDb.Extensions.Models;

namespace Snake.LiteDb.Extensions.Mappers
{
    public interface ILiteDbQuery<T, TResult>
    {
        TResult ExecuteQuery();
        TResult UseServerSide<TResult>(LiteDbSet<T>.UseQueryFunc<T, TResult> query);
    }
}