namespace FilesHandler;
public class FileEditCompletedEventArgs : EventArgs
{
    private readonly Guid _taskId;
    public Guid TaskId { get { return _taskId; } }

    private readonly string _file;
    public string File { get { return _file; } }

    private readonly FileEditStatuses _status;
    public FileEditStatuses Status { get { return _status; } }

    private readonly int _filesNum;
    public int FilesNum { get { return _filesNum; } }

    public FileEditCompletedEventArgs(Guid taskId, string file, FileEditStatuses status, int filesNum)
    {
        _taskId = taskId;
        _file = file;
        _status = status;
        _filesNum = filesNum;
    }

    public override string ToString()
    {
        return $"Id: {TaskId}, File: {File}, Status: {Status}, Number of files: {FilesNum}";
    }
}
