using AdonisUI.Controls;

namespace FileSorter;

public partial class MainWindow : AdonisWindow
{
    MainViewModel GetMain() => (MainViewModel)DataContext;
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
    }

    private void GoToPrevious(object sender, System.Windows.RoutedEventArgs e)
    {
        GetMain()._main.GoToPreviousFile(GetMain());
    }

    private void GoToNext(object sender, System.Windows.RoutedEventArgs e)
    {
        GetMain()._main.GoToNextFile(GetMain());
    }
}
