using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Wpf.Ui.Controls;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace ParserGuiWpf.Helpers;

public class CustomMessageBox
{
    private Wpf.Ui.Controls.MessageBox _mb;

    public CustomMessageBox()
    {
        _mb = new Wpf.Ui.Controls.MessageBox();
    }
    public CustomMessageBox(string message, string title = "Message", MessageBoxButton buttons = MessageBoxButton.OK)
    {
        _mb = new Wpf.Ui.Controls.MessageBox
        {
            
            AllowDrop             = false,
            Title                 = title,
            Topmost               = true,
            ShowTitle             = true,
            CloseButtonText       = "Exit",
            CloseButtonAppearance = ControlAppearance.Danger,
            SizeToContent = SizeToContent.Width,
            ResizeMode = ResizeMode.CanResize
        };

        SetButtons(buttons);
        SetContent(message);
        
    }

    private void SetContent(string message)
    {
        var scrollable = new DynamicScrollViewer()
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            Padding = new Thickness(5),
            Width = _mb.Width
        };

        var textBlock = new Wpf.Ui.Controls.TextBlock
        {
            TextWrapping = TextWrapping.WrapWithOverflow,
            Text         = message,
            Width = _mb.Width
        };
        (scrollable as IAddChild).AddChild(textBlock);

        _mb.Content = scrollable;
    }

    public async Task<MessageBoxResult> ShowDialogAsync(CancellationToken token)
    {
        var result = MessageBoxResult.None;
        try
        {
            result = await _mb.ShowDialogAsync(true, token);
        }
        catch (OperationCanceledException)
        {
            //OK
        }
        return result;
    }
    public async Task<MessageBoxResult> ShowCanceledDialog(CancellationToken token)
    {
        _mb = new Wpf.Ui.Controls.MessageBox()
        {
            ShowTitle             = true,
            Title                 = "Canceled",
            Content               = "Operation canceled",
            CloseButtonAppearance = ControlAppearance.Primary,
            CloseButtonText       = "OK"
        };
        return await ShowDialogAsync(token);
    }

    private void SetButtons(MessageBoxButton buttons)
    {
        //todo
    }
}