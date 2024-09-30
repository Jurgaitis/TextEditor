using System.Diagnostics;

namespace FilesHandler;
public class FilesEditor
{
    public event EventHandler<FileEditCompletedEventArgs>? fileEditingCompleted;
    public FileHandler? fileHandlers;

    public void EditFile(string filePath)
    {
        if (fileHandlers is null)
            throw new ArgumentNullException(nameof(fileHandlers));

        foreach (var handler in fileHandlers.GetInvocationList())
        {
            string tempFilePath = Path.GetTempPath() + Guid.NewGuid().ToString();
            using (FileStream readStream = File.OpenRead(filePath))
            using (FileStream writeStream = File.Create(tempFilePath))
            using (StreamReader reader = new(readStream))
            using (StreamWriter writer = new(writeStream))
            {
                handler.DynamicInvoke(reader, writer);

                reader.Close();
                writer.Close();
                readStream.Close();
                writeStream.Close();
            };
            File.Replace(tempFilePath, filePath, null);
        }
    }

    public Task EditFiles(string[] files, string outputFile)
    {
        if (fileHandlers is null)
            throw new ArgumentNullException(nameof(fileHandlers));

        return Task.Run(() =>
        {
            var tasksId = Guid.NewGuid();
            foreach (string file in files)
            {
                try
                {
                    string fileCopyPath = Path.GetTempPath() + Guid.NewGuid().ToString();
                    File.Copy(file, fileCopyPath);
                    EditFile(fileCopyPath);
                    using (FileStream readStream = File.OpenRead(fileCopyPath))
                    using (FileStream writeStream = new(outputFile, FileMode.Append, FileAccess.Write))
                    {
                        readStream.CopyTo(writeStream);
                        readStream.Close();
                        writeStream.Close();
                    }
                    File.Delete(fileCopyPath);
                    RaiseFileEditingCompleted(tasksId, file, FileEditStatuses.Success, files.Length);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    RaiseFileEditingCompleted(tasksId, file, FileEditStatuses.Failed, files.Length);
                }
            };
        });
    }

    public Task EditFilesParallel(string[] files)
    {
        if (fileHandlers is null)
            throw new ArgumentNullException(nameof(fileHandlers));

        return Task.Run(() =>
        {
            var tasksId = Guid.NewGuid();
            Parallel.ForEach(files, (string file) =>
            {
                try
                {
                    EditFile(file);
                    RaiseFileEditingCompleted(tasksId, file, FileEditStatuses.Success, files.Length);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    RaiseFileEditingCompleted(tasksId, file, FileEditStatuses.Failed, files.Length);
                }
            });
        });
    }

    private void RaiseFileEditingCompleted(Guid taskId, string file, FileEditStatuses status, int filesNum)
    {
        if (fileEditingCompleted is null)
            return;

        fileEditingCompleted(this, new FileEditCompletedEventArgs(taskId, file, status, filesNum));
    }
}
