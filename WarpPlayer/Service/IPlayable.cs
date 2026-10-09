namespace Player.Service
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
