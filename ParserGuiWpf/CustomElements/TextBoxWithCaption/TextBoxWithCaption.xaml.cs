using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ParserGuiWpf.CustomElements.TextBoxWithCaption
{
    /// <summary>
    /// Interaction logic for TextBoxWithCaption.xaml
    /// </summary>
    public partial class TextBoxWithCaption : UserControl
    {
        public TextBoxWithCaption()
        {
            InitializeComponent();
        }
        

        public static readonly DependencyProperty TextValueProperty =
            DependencyProperty.Register(nameof(TextValue),
                                        typeof(string),
                                        typeof(TextBoxWithCaption),
                                        new FrameworkPropertyMetadata(string.Empty, 
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                                                                      OnTextChanged));

        public static readonly DependencyProperty CaptionValueProperty =
            DependencyProperty.Register(nameof(CaptionValue),
                                        typeof(string),
                                        typeof(TextBoxWithCaption),
                                        new FrameworkPropertyMetadata(string.Empty, 
                                                                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                                                                      OnCaptionChanged));

        public string TextValue
        {
            get => (string)GetValue(TextValueProperty);
            set => SetValue(TextValueProperty, value);
        }

        public string CaptionValue
        {
            get => (string)GetValue(CaptionValueProperty);
            set => SetValue(CaptionValueProperty, value);
        }
        private static void OnTextChanged(DependencyObject dObj, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("new value: " + e.NewValue);
            var thisControl = (TextBoxWithCaption)dObj;
            thisControl.TextBoxText.Text = e.NewValue.ToString();
        }
        private static void OnCaptionChanged(DependencyObject dObj, DependencyPropertyChangedEventArgs e)
        {
            Debug.WriteLine("new value: " + e.NewValue);
            var thisControl = (TextBoxWithCaption)dObj;
            thisControl.CaptionTextBlock.Text = e.NewValue as string;
        }
    }
}
