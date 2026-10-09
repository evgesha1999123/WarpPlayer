using Microsoft.UI.Xaml.Controls;

namespace Player.Views;


public sealed partial class TrackView : UserControl
{
    public TrackView()
    {
        InitializeComponent();
        //ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Hand);
    }
}