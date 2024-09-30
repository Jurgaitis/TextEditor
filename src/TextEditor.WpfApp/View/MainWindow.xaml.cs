using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TextEditor.WpfApp.ViewModel;

namespace TextEditor.WpfApp.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetRequiredService<MainWindowVm>();
        }
        public MainWindowVm ViewModel => (MainWindowVm)DataContext;

        private void MoveFiles(object sender, EventArgs arg)
        {
            if (ViewModel.MoveFilesCommand.CanExecute(null))
                ViewModel.MoveFilesCommand.Execute(GetSelectedFiles());
        }

        private void RemoveFiles(object sender, EventArgs arg)
        {
            if (ViewModel.RemoveFilesCommand.CanExecute(null))
                ViewModel.RemoveFilesCommand.Execute(GetSelectedFiles());
        }

        private List<string> GetSelectedFiles()
        {
            List<string> selectedFiles = new();

            if (filesLv.SelectedItems.Count != 0)
            {
                foreach (string file in filesLv.SelectedItems)
                    selectedFiles.Add(file);
            }
            else if (filesToEditLv.SelectedItems.Count != 0)
            {
                foreach (string file in filesToEditLv.SelectedItems)
                    selectedFiles.Add(file);
            }
            return selectedFiles;
        }

        private void LvLostFocus(object sender, RoutedEventArgs e)
        {
            var lv = (ListView)sender;
            lv.UnselectAll();
        }

        private void MinWordLengthTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out int _);
            if (!e.Handled)
            {
                var textBox = sender as TextBox;
                if (textBox.Text.Length != 0 && textBox.Text.First() == '0')
                    textBox.Text.Reverse();
            }
        }
            
    }
}