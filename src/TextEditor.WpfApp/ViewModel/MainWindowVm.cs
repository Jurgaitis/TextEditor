using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FilesHandler;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using TextEditor.WpfApp.Model;

namespace TextEditor.WpfApp.ViewModel;
public partial class MainWindowVm : ObservableObject
{
    private FilesSelector _filesSelector;

    private int _failsNum;
    private FileHandler? _truncateHandler;

    public FilesEditor filesEditor;

    #region properties
    [ObservableProperty]
    private int _executionProgress;

    [ObservableProperty]
    private string? _outputFile;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(MoveFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveFilesCommand))]
    [NotifyCanExecuteChangedFor(nameof(SelectOutputFileCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveOutputFileCommand))]
    private bool _isFilesEditing;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditFilesCommand))]
    private bool _isTruncate;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditFilesCommand))]
    private bool _isRemovePunctuations;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditFilesCommand))]
    private int _minWordLength;

    public ObservableCollection<string> Files { get; } = new();
    public ObservableCollection<string> FilesToEdit { get; } = new();
    #endregion

    public MainWindowVm()
    {
        FilesToEdit.CollectionChanged += OnFilesToEditChanged;

        _filesSelector = new();
        filesEditor = new();
        FileEditingCompletedSubscribe();
    }

    #region commands
    [RelayCommand(CanExecute = nameof(CanEditFiles))]
    public async Task EditFiles()
    {
        if (FilesToEdit.Count <= 0)
            throw new ArgumentException("List of files to edit can not be empty.", nameof(FilesToEdit));

        IsFilesEditing = true;
        if (OutputFile is null)
            await filesEditor.EditFilesParallel(FilesToEdit.ToArray());
        else
            await filesEditor.EditFiles(FilesToEdit.ToArray(), OutputFile);
        IsFilesEditing = false;
        ExecutionProgress = 0;
        _failsNum = 0;
        FilesToEdit.Clear();
        OutputFile = null;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
    public void SelectFiles()
    {
        if (_filesSelector.OpenFileDialogMultiselect())
            foreach (var file in _filesSelector.filePaths)
                if (!Files.Contains(file) && !FilesToEdit.Contains(file))
                    Files.Add(file);
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
    private void MoveFiles(List<string> selectedFiles)
    {
        if (FilesToEdit.Any(_ => selectedFiles.Contains(_)))
        {
            selectedFiles.ForEach(_ => FilesToEdit.Remove(_));
            selectedFiles.ForEach(_ => Files.Add(_));
        }
        else if (Files.Any(_ => selectedFiles.Contains(_)))
        {
            selectedFiles.ForEach(_ => Files.Remove(_));
            selectedFiles.ForEach(_ => FilesToEdit.Add(_)); ;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
    private void RemoveFiles(List<string> selectedFiles)
    {
        if (FilesToEdit.Any(_ => selectedFiles.Contains(_)))
            selectedFiles.ForEach(_ => FilesToEdit.Remove(_));
        else if (Files.Any(_ => selectedFiles.Contains(_)))
            selectedFiles.ForEach(_ => Files.Remove(_));
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
    public void SelectOutputFile()
    {
        if (_filesSelector.OpenFileDialog())
        {
            string file = _filesSelector.filePaths.First();
            if (!Files.Contains(file) && !FilesToEdit.Contains(file))
                OutputFile = file;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCommands))]
    private void RemoveOutputFile()
        => OutputFile = null;
    #endregion

    #region onPropertyChanged

    partial void OnMinWordLengthChanged(int value)
    {
        OnIsTruncateChanged(IsTruncate);
    }

    partial void OnIsTruncateChanged(bool value)
    {
        if (_truncateHandler is not null)
        {
            filesEditor.fileHandlers -= _truncateHandler;
            _truncateHandler = null;
        }
        if (value && MinWordLength > 0)
        {
            _truncateHandler += (StreamReader reader, StreamWriter writer) => FileHandlers.TruncateText(reader, writer, MinWordLength);
            filesEditor.fileHandlers += _truncateHandler;
        }
    }
       
    partial void OnIsRemovePunctuationsChanged(bool value)
    {
        if (value)
            filesEditor.fileHandlers += FileHandlers.RemovePunctuations;
        else
            filesEditor.fileHandlers -= FileHandlers.RemovePunctuations;
    }

    private void OnFilesToEditChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        EditFilesCommand.NotifyCanExecuteChanged();
    #endregion

    public void FileEditingCompletedSubscribe()
        => filesEditor.fileEditingCompleted += OnFileEditingCompleted;

    public void FileEditingCompletedUnsubscribe()
        => filesEditor.fileEditingCompleted -= OnFileEditingCompleted;

    private bool CanEditFiles() =>
        (IsTruncate && MinWordLength > 0 || IsRemovePunctuations) && FilesToEdit.Count > 0 && !IsFilesEditing;

    private bool CanExecuteCommands()
        => !IsFilesEditing;

    private void OnFileEditingCompleted(object? sender, FileEditCompletedEventArgs e)
    {
        if (e.Status == FileEditStatuses.Failed)
            _failsNum++;

        ExecutionProgress += (int)Math.Ceiling(100f / e.FilesNum);

        if (ExecutionProgress >= 100)
        {
            if (_failsNum != 0)
                MessageBoxWarning(_failsNum, FilesToEdit.Count);
            else
                MessageBoxSuccsess();
        }
    }
    
    private void MessageBoxSuccsess() =>
        MessageBox.Show("Все файлы успешно отредактированы.", "Успех!", MessageBoxButton.OK);

    private void MessageBoxWarning(int notEditedFiles, int filesNum) =>
        MessageBox.Show($"{notEditedFiles} из {filesNum} файлов не отредактированы.", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
}
