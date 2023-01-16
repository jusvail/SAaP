namespace SAaP.Contracts.Services;

public interface ILogger
{
    Task Log(string message);
}