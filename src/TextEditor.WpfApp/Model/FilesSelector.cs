using Microsoft.Win32;
using System.ComponentModel;

namespace TextEditor.WpfApp.Model;
public class FilesSelector : IDisposable
{
    private OpenFileDialog _fileDialog;
    public string[] filePaths;

    public FilesSelector()
    {
        _fileDialog = new OpenFileDialog();
        _fileDialog.Filter = "(*.txt)|*.txt|(*.*)|*.*";
        _fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        _fileDialog.FileOk += OnFileOk;
    }

    public bool OpenFileDialogMultiselect()
    {
        _fileDialog.Multiselect = true;
        return (bool)_fileDialog.ShowDialog();
    }

    public bool OpenFileDialog()
    {
        _fileDialog.Multiselect = false;
        return (bool)_fileDialog.ShowDialog();
    }

    private void OnFileOk(object? sender, CancelEventArgs e) => filePaths = _fileDialog.FileNames;

    public void Dispose() => _fileDialog.FileOk -= OnFileOk;
}
