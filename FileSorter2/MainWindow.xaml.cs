using AdonisUI.Controls;
namespace FileSorter;

public partial class MainWindow : AdonisWindow
{
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
    }
}
