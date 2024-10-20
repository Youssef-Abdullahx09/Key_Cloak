namespace KeyCloakSolution.Controllers;

public class Response
{
    public dynamic? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string[]? errors { get; set; }
}