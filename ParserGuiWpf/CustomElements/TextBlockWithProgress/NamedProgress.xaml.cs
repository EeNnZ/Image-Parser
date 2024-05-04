using System.Diagnostics;
using System.Windows;

namespace ParserGuiWpf.CustomElements.TextBlockWithProgress
{
    /// <summary>
    /// Interaction logic for NamedProgress.xaml
    /// </summary>
    public partial class NamedProgress
    {
        public string ProgressName
        {
            get => (string)GetValue(ProgressNameProperty);
            set => SetValue(ProgressNameProperty, value);
        }
        public double ProgressValue
        {
            get => (double)GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        public string ProgressText
        {
            get => (string)GetValue(ProgressTextProperty);
            set => SetValue(ProgressTextProperty, value);
        }

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty ProgressNameProperty =
            DependencyProperty.Register(nameof(ProgressName),
                                        typeof(string),
                                        typeof(NamedProgress),
                                        new FrameworkPropertyMetadata("progressNameText"));


        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register(nameof(ProgressValue),
                                        typeof(double),
                                        typeof(NamedProgress),
                                        new FrameworkPropertyMetadata(0d,
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                      OnProgressValueChanged));

        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register(nameof(ProgressText),
                                        typeof(string),
                                        typeof(NamedProgress),
                                        new FrameworkPropertyMetadata("status",
                                            OnProgressTextChanged));

        private static void OnProgressTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var thisControl = (NamedProgress)d;
            thisControl.ProgressStatusTextBlock.Text = e.NewValue as string;
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected),
                                        typeof(bool),
                                        typeof(NamedProgress),
                                        new FrameworkPropertyMetadata(false,
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                      OnSelectionChanged));

        public NamedProgress()
        {
            InitializeComponent();
        }
        private static void OnProgressValueChanged(DependencyObject dObj, DependencyPropertyChangedEventArgs e)
        {
            var thisControl = (NamedProgress)dObj;
            thisControl.ProgressBarElement.Value = (double)e.NewValue;
        }

        private static void OnSelectionChanged(DependencyObject dObj, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("new value: " + e.NewValue);
            var thisControl = (NamedProgress)dObj;
            thisControl.SelectionCheckBox.IsChecked = (bool)e.NewValue;
        }
    }
}
