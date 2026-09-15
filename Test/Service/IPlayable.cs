using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace Test.Service
{
    public interface IPlayable
    {
        public void Play();
        public void Pause();

        public void Rewind(double delta);
        public void Stop();

        public void Seek(double seconds);
    }
}
