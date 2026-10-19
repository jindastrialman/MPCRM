using DataService.Models.Abstractions;

namespace DataService.Models;

public class ItemInfo : BaseLiteDbModel
{
    public int ProductCode {get;set;}
    public int ItemPrice {get;set;}
}
