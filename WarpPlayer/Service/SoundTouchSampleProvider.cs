using NAudio.Wave;
using SoundTouch;
using System;

namespace Player.Service
{
    public class SoundTouchSampleProvider : ISampleProvider
    {
        private readonly SoundTouchProcessor _soundTouch;
        private readonly ISampleProvider _source;
        private readonly int _channels;
        private readonly float[] _inputBuffer;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public float Speed
        {
            get => (float)_soundTouch.Rate;
            set => _soundTouch.Rate = value;
        }

        public SoundTouchSampleProvider(SoundTouchProcessor processor, ISampleProvider source, int channels, float speed)
        {
            _soundTouch = processor;
            _source = source;
            _channels = channels;

            _inputBuffer = new float[4096 * channels];

            _soundTouch.Channels = channels;
            _soundTouch.SampleRate = source.WaveFormat.SampleRate;
            _soundTouch.Rate = speed;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int totalWritten = 0;

            while (totalWritten < count)
            {
                int availableFrames = _soundTouch.AvailableSamples;

                if (availableFrames > 0)
                {
                    int requestedFrames = Math.Min(
                        availableFrames,
                        (count - totalWritten) / _channels);

                    if (requestedFrames <= 0)
                        break;

                    Span<float> output = buffer.AsSpan(
                        offset + totalWritten,
                        requestedFrames * _channels);

                    int receivedFrames =
                        _soundTouch.ReceiveSamples(
                            in output,
                            requestedFrames);

                    totalWritten += receivedFrames * _channels;

                    continue;
                }

                int samplesRead = _source.Read(
                    _inputBuffer,
                    0,
                    _inputBuffer.Length);

                if (samplesRead == 0)
                {
                    _soundTouch.Flush();

                    int requestedFrames =
                        (count - totalWritten) / _channels;

                    if (requestedFrames > 0)
                    {
                        Span<float> output = buffer.AsSpan(
                            offset + totalWritten,
                            requestedFrames * _channels);

                        int flushedFrames =
                            _soundTouch.ReceiveSamples(
                                in output,
                                requestedFrames);

                        totalWritten += flushedFrames * _channels;
                    }

                    break;
                }

                int inputFrames = samplesRead / _channels;

                _soundTouch.PutSamples(
                    _inputBuffer.AsSpan(0, samplesRead),
                    inputFrames);
            }

            return totalWritten;
        }

        public void Clear()
        {
            _soundTouch.Clear();
        }
    }
}