using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Test.ViewModel
{
    public class PlaylistViewModel
    {
        public string Title { get; } = "Hello from PlaylistViewModel!";
        public Visibility Visibility;

        public PlaylistViewModel(Visibility visibility)
        {
            Visibility = visibility;
        }
    }
}
