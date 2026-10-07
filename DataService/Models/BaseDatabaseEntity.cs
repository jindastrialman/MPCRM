using System.ComponentModel.DataAnnotations;

namespace DataService.Models;

public abstract class BaseDatabaseEntity
{
    [Key]
    public bool Id{get; set;}
    public bool IsDeleted{get; set;}
    public bool Version{get; set;}
    public bool UpdatedAt{get; set;}
    public bool CreatedAt{get; set;}

}
