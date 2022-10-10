namespace SAaP.Models;

public class TaskStartEventArgs : EventArgs
{
    public string TitleBarMessage { get; set; }

    public CancellationToken CancellationToken { get; set; }
}