using System.ComponentModel.DataAnnotations;
using Framework.DataAccess.Models;

namespace DataService.Models.Abstractions;

public abstract class BaseLiteDbModel : IBaseEntity
{
    [Key]
    public int Id{get; set;}
    public bool IsDeleted{get; set;}
    public int? Version{get; set;}
    public DateTime? UpdatedAt{get; set;}
    public DateTime? CreatedAt{get; set;}

}
