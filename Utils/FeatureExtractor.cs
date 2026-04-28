namespace AudioClassification.Utils
{
    public class FeatureExtractor
    {
        public static float[] ExtractFeatures(string filePath)
        {
            var samples = ReadWavSamples(filePath);

            if (samples.Count == 0)
            {
                throw new InvalidDataException($"No audio samples could be read from: {filePath}");
            }

            float avg = samples.Average();
            float max = samples.Max();
            float min = samples.Min();
            float energy = samples.Select(x => x * x).Average();

            return new[] { avg, max, min, energy };
        }

        private static List<float> ReadWavSamples(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            using var reader = new BinaryReader(stream);

            string riffId = new string(reader.ReadChars(4));
            if (riffId != "RIFF")
            {
                throw new InvalidDataException($"Unsupported WAV container in '{Path.GetFileName(filePath)}'.");
            }

            reader.ReadUInt32();

            string waveId = new string(reader.ReadChars(4));
            if (waveId != "WAVE")
            {
                throw new InvalidDataException($"File '{Path.GetFileName(filePath)}' is not a valid WAV file.");
            }

            ushort audioFormat = 0;
            ushort channels = 0;
            ushort bitsPerSample = 0;
            byte[]? data = null;

            while (reader.BaseStream.Position + 8 <= reader.BaseStream.Length)
            {
                string chunkId = new string(reader.ReadChars(4));
                int chunkSize = reader.ReadInt32();

                if (chunkSize < 0 || reader.BaseStream.Position + chunkSize > reader.BaseStream.Length)
                {
                    throw new InvalidDataException($"Corrupt WAV chunk in '{Path.GetFileName(filePath)}'.");
                }

                if (chunkId == "fmt ")
                {
                    audioFormat = reader.ReadUInt16();
                    channels = reader.ReadUInt16();
                    reader.ReadUInt32();
                    reader.ReadUInt32();
                    reader.ReadUInt16();
                    bitsPerSample = reader.ReadUInt16();

                    int remaining = chunkSize - 16;
                    if (remaining > 0)
                    {
                        reader.ReadBytes(remaining);
                    }
                }
                else if (chunkId == "data")
                {
                    data = reader.ReadBytes(chunkSize);
                }
                else
                {
                    reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
                }

                if ((chunkSize & 1) == 1 && reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    reader.BaseStream.Seek(1, SeekOrigin.Current);
                }
            }

            if (channels == 0 || data == null)
            {
                throw new InvalidDataException($"Missing WAV metadata in '{Path.GetFileName(filePath)}'.");
            }

            return DecodeSamples(data, audioFormat, channels, bitsPerSample, filePath);
        }

        private static List<float> DecodeSamples(byte[] data, ushort audioFormat, ushort channels, ushort bitsPerSample, string filePath)
        {
            int bytesPerSample = bitsPerSample / 8;
            int frameSize = bytesPerSample * channels;

            if (frameSize <= 0 || data.Length < frameSize)
            {
                throw new InvalidDataException($"Invalid sample layout in '{Path.GetFileName(filePath)}'.");
            }

            var samples = new List<float>(data.Length / frameSize);

            switch (audioFormat)
            {
                case 1:
                case 65534:
                    DecodePcmSamples(data, channels, bitsPerSample, frameSize, samples, filePath);
                    break;

                case 3:
                    DecodeFloatSamples(data, channels, bitsPerSample, frameSize, samples, filePath);
                    break;

                default:
                    throw new InvalidDataException(
                        $"Unsupported WAV format code {audioFormat} in '{Path.GetFileName(filePath)}'.");
            }

            return samples;
        }

        private static void DecodePcmSamples(byte[] data, ushort channels, ushort bitsPerSample, int frameSize, List<float> samples, string filePath)
        {
            for (int offset = 0; offset + frameSize <= data.Length; offset += frameSize)
            {
                float mixedSample = 0f;

                for (int channel = 0; channel < channels; channel++)
                {
                    int sampleOffset = offset + (channel * bitsPerSample / 8);
                    mixedSample += bitsPerSample switch
                    {
                        8 => (data[sampleOffset] - 128) / 128f,
                        16 => BitConverter.ToInt16(data, sampleOffset) / 32768f,
                        24 => ReadInt24(data, sampleOffset) / 8388608f,
                        32 => BitConverter.ToInt32(data, sampleOffset) / 2147483648f,
                        _ => throw new InvalidDataException(
                            $"Unsupported PCM bit depth {bitsPerSample} in '{Path.GetFileName(filePath)}'.")
                    };
                }

                samples.Add(mixedSample / channels);
            }
        }

        private static void DecodeFloatSamples(byte[] data, ushort channels, ushort bitsPerSample, int frameSize, List<float> samples, string filePath)
        {
            if (bitsPerSample != 32)
            {
                throw new InvalidDataException(
                    $"Unsupported float WAV bit depth {bitsPerSample} in '{Path.GetFileName(filePath)}'.");
            }

            for (int offset = 0; offset + frameSize <= data.Length; offset += frameSize)
            {
                float mixedSample = 0f;

                for (int channel = 0; channel < channels; channel++)
                {
                    int sampleOffset = offset + (channel * 4);
                    mixedSample += BitConverter.ToSingle(data, sampleOffset);
                }

                samples.Add(mixedSample / channels);
            }
        }

        private static int ReadInt24(byte[] data, int offset)
        {
            int value = data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);

            if ((value & 0x800000) != 0)
            {
                value |= unchecked((int)0xFF000000);
            }

            return value;
        }
    }
}
