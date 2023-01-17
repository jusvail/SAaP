namespace SAaP.Contracts.Services;

public interface ILogger
{
    Task Log(string message);

    Task Log(List<string> message);
}