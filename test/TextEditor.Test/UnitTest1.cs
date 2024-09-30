using FilesHandler;
using TextEditor.WpfApp.ViewModel;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace TextEditor.Test
{
    public class UnitTest1
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public UnitTest1(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async void Test_NoFilesToEdit_ThrowsArgumentException()
        {
            MainWindowVm ViewModel = new();
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => ViewModel.EditFiles());

            var expected = ex.ParamName;
            var actual = nameof(ViewModel.FilesToEdit);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void Test_HasNotFileHandlers_ThrowsArgumentNullException()
        {
            MainWindowVm ViewModel = new();
            ViewModel.FilesToEdit.Add(@"..\..\..\Resources\exist.txt");

            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => ViewModel.EditFiles());

            var expected = ex.ParamName;
            var actual = nameof(ViewModel.filesEditor.fileHandlers);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void Test_NotExistingFile_RaiseEvent_StatusFailed()
        {
            MainWindowVm ViewModel = new()
            {
                IsRemovePunctuations = true
            };
            ViewModel.FileEditingCompletedUnsubscribe();
            ViewModel.FilesToEdit.Add(@"..\..\..\Resources\notExist.txt");

            var e = await Assert.RaisesAsync<FileEditCompletedEventArgs>(
                a => ViewModel.filesEditor.fileEditingCompleted += a,
                a => ViewModel.filesEditor.fileEditingCompleted -= a,
                () => ViewModel.EditFiles());
            Assert.NotNull(e);

            var expected = FileEditStatuses.Failed;
            var actual = e.Arguments.Status;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void Test_RemovePunctuations_RaiseEvent_StatusSuccess()
        {
            MainWindowVm ViewModel = new()
            {
                IsRemovePunctuations = true
            };
            ViewModel.FileEditingCompletedUnsubscribe();
            ViewModel.FilesToEdit.Add(@"..\..\..\Resources\exist.txt");


            var e = await Assert.RaisesAsync<FileEditCompletedEventArgs>(
                a => ViewModel.filesEditor.fileEditingCompleted += a,
                a => ViewModel.filesEditor.fileEditingCompleted -= a,
                () => ViewModel.EditFiles());
            Assert.NotNull(e);

            var expected = FileEditStatuses.Success;
            var actual = e.Arguments.Status;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void Test_RemovePunctuationsTruncateText5_RaiseEvent_StatusSuccess()
        {
            MainWindowVm ViewModel = new()
            { 
                IsRemovePunctuations = true,
                IsTruncate = true,
                MinWordLength = 5
            };
            ViewModel.FileEditingCompletedUnsubscribe();
            ViewModel.FilesToEdit.Add(@"..\..\..\Resources\exist.txt");

            var e = await Assert.RaisesAsync<FileEditCompletedEventArgs>(
                a => ViewModel.filesEditor.fileEditingCompleted += a,
                a => ViewModel.filesEditor.fileEditingCompleted -= a,
                () => ViewModel.EditFiles());
            Assert.NotNull(e);

            var expected = FileEditStatuses.Success;
            var actual = e.Arguments.Status;

            Assert.Equal(expected, actual);
        }
    }
}