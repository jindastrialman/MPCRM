namespace Framework.DataAccess.Models;

public interface IBaseEntity
{
    public int Id{get; set;}
    public bool IsDeleted{get; set;}
    public int Version{get; set;}
    public DateTime UpdatedAt{get; set;}
    public DateTime CreatedAt{get; set;}
}
