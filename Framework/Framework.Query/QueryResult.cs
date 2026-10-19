namespace Framework.Query;

public class QueryResult<T>
{
    public int ChangedCount{get;init;}
    public int TotalCount {get;init;}
    public IEnumerable<T>? OutputCollection {get; init;} // will be used for debug logging probabaly because it is too heavy for liteDB // PS oh and for get requests
}
