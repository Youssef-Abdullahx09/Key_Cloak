namespace KeyCloakSolution.Domain;

public class FilterDto
{
    public string? username { get; set; }
    public string? firstName { get; set; }
    public string? lastName { get; set; }
    public string? email { get; set; }
    public string? search { get; set; }
    public bool enabled { get; set; }
    public Direction direction { get; set; }
    public string? sortBy { get; set; }
}
public enum Direction
{
    Asc,
    Desc
}