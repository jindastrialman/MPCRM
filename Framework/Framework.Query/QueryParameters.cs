using System.Linq.Expressions;

namespace Framework.Query;

public class QueryParameters<T>
{
    public AuthParameters? AuthParameters{get;init;}
    public PagingParameters? PagingParameters{get; init;}
    public FilterParameters<T>? FilterParameters{get; init;}
    public DeleteParameters? DeleteParameters{get; init;}
    public CollectionParameters<T>? CollectionParameters{get;init;}
}

public class AuthParameters
{
    public IEnumerable<string> Permissions {get;set;}
}

public class PagingParameters
{
    public int Skip {get;set;} = 0;
    public int Take {get;set;} = 10;
}

public class FilterParameters<T>
{
    public Expression<Func<T, bool>> FilterExpression {get; set;} = x => true;
}
public class CollectionParameters<T>
{
    public IEnumerable<T> Collection{get;set;} = new List<T>();
}
public class DeleteParameters
{
    public bool ForcedDelete{get; set;} = false;
}