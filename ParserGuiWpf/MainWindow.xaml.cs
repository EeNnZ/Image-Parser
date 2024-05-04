using Wpf.Ui.Controls;

namespace ParserGuiWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);
            };
        }
    }
}