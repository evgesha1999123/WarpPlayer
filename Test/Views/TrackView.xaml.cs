using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace Test.Views;

public sealed partial class TrackView : UserControl
{
    public TrackView()
    {
        InitializeComponent();
        //ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }
}