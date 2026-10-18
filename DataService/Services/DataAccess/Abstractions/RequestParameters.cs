using DataService.Models;

public class DeleteParameters
{
    public bool ForcedDelete{get; set;} = false;
}

public class FilterParameters<T> where T : BaseDatabaseEntity
{
    public bool IncludeDeleted{get; set;} = false;
}

public class PagedFilterParameters<T> : FilterParameters<T> where T : BaseDatabaseEntity
{
    public int Skip {get;set;} = 0;
    public int Take {get;set;} = 10;
}