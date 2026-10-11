using System.ComponentModel.DataAnnotations;

namespace DataService.Models;

public abstract class BaseDatabaseEntity
{
    [Key]
    public int Id{get; set;}
    public bool IsDeleted{get; set;}
    public int Version{get; set;}
    public DateTime UpdatedAt{get; set;}
    public DateTime CreatedAt{get; set;}

}
