using NAudio.Wave;
using System;
using System.Diagnostics;
using System.DirectoryServices;
using System.Runtime.CompilerServices;


namespace Test.Service
{
    public class SpeedSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly int channels;

        public float Speed { get; set; } = 1.0f;

        public WaveFormat WaveFormat => source.WaveFormat;

        public SpeedSampleProvider(ISampleProvider source)
        {
            this.source = source;
            channels = source.WaveFormat.Channels;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int sourceCount = (int)(count * Speed) + channels;

            float[] temp = new float[sourceCount];

            int samplesRead = source.Read(temp, 0, sourceCount);

            if (samplesRead == 0)
                return 0;

            int outputSamples = Math.Min(
                count,
                (int)(samplesRead / Speed)
            );

            for (int i = 0; i < outputSamples; i++)
            {
                float position = i * Speed;

                int index = (int)position;
                float fraction = position - index;

                if (index + 1 >= samplesRead)
                    break;

                buffer[offset + i] =
                    temp[index] * (1 - fraction) +
                    temp[index + 1] * fraction;
            }

            return outputSamples;
        }
    }
}
