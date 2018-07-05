using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace ColorTurbine
{

    public enum FillType
    {
        Set,
        Add,
        Factor
    }
    public interface IStrip
    {
        void Clear();

        void Fill(int start, int end, RGBWColor c, FillType mode = FillType.Set);
        void Fill(double start, double end, RGBWColor c, FillType mode = FillType.Set);
        string Name { get; set; }
        void Send(bool force);
        void DumpBuffer();

        RGBWColor this[int index] { get; set; }

        bool dirty { get; set; }
        int led_count { get; }
    }

    public abstract class Strip : IStrip
    {
        internal const int header_len = 6;
        internal int bytes_per_pixel;
        public bool use_rle;

        public Strip(IPAddress address, int led_count, bool enable_rle = true)
        : this(new IPEndPoint(address, 7890), led_count, enable_rle)
        { } 

        public Strip(IPEndPoint address, int led_count, bool enable_rle = true)
        {
            client = new UdpClient();

            this.use_rle = enable_rle;
            this.address = address;
            this.led_count = led_count;
            this.packet_len = _packetLength(led_count) + header_len;

            buffer = new byte[packet_len];
            buffer.Initialize();
            rleBuf = new byte[packet_len * 2];
            rleBuf.Initialize();

            var header = new byte[] { 0, 0, 0, 0, 0, 0 };
            header.CopyTo(buffer, 0);
            dirty = true;
        }

        public void Clear()
        {
            dirty = true;
            Array.Clear(buffer, header_len, buffer.Length - header_len);
        }

        public void Fill(int start, int end, RGBWColor c, FillType mode = FillType.Set)
        {
            switch (mode)
            {
                case FillType.Set:
                    for (int i = start; i <= end; i++)
                    {
                        this[i] = c;
                    }
                    break;
                case FillType.Add:

                    for (int i = start; i <= end; i++)
                    {
                        this[i] += c;
                    }
                    break;
                case FillType.Factor:
                    var width = end - start;
                    var half = width / 2;
                    for (int i = 0; i < width; i++)
                    {
                        this[start + i] += c * Math.Abs(i / half);
                    }
                    break;
            }
        }

        private RGBWColor BlendStart(double pixel, RGBWColor c)
        {
            return c * (1 - Math.Abs(pixel - Math.Floor(pixel)));
        }

        private RGBWColor BlendEnd(double pixel, RGBWColor c)
        {
            return c * Math.Abs(pixel - Math.Floor(pixel));
        }

        // Note: this doesn't handle the sub-1px case
        public void Fill(double start, double end, RGBWColor c, FillType mode = FillType.Set)
        {
            if (start == end)
            {
                return;
            }

            var width = (int)Math.Ceiling(end - start);

            switch (mode)
            {
                case FillType.Set:
                    Fill((int)Math.Floor(start), (int)Math.Ceiling(end), new RGBWColor(0, 0, 0, 0), FillType.Set);
                    Fill(start, end, c, FillType.Add);
                    break;

                case FillType.Add:
                    for (int i = -1; i < width + 1; i++)
                    {
                        var a = (int)Math.Floor(start + i);
                        var b = (int)Math.Ceiling(start + i);
                        if (a >= 0 && a < led_count && i >= 0)
                        {
                            this[a] += BlendStart(start + i, c);
                        }
                        if (b >= 0 && b < led_count && i < width)
                        {
                            this[b] += BlendEnd(start + i, c);
                        }
                    }
                    break;

                case FillType.Factor:
                    var half = width / 2.0;
                    for (int i = -1; i < width + 1; i++)
                    {
                        var newc = c * (1 - Math.Abs((i - half) / half));

                        var a = (int)Math.Floor(start + i);
                        var b = (int)Math.Ceiling(start + i);
                        if (a >= 0 && a < led_count && i >= 0)
                        {
                            this[a] += BlendStart(start + i, newc);
                        }
                        if (b >= 0 && b < led_count && i < width)
                        {
                            this[b] += BlendEnd(start + i, newc);
                        }
                    }
                    break;
            }
        }

        public string Name { get; set; }

        internal UdpClient client;
        internal IPEndPoint address;
        public int led_count { get; private set; }
        private int packet_len;
        internal byte[] buffer;
        public bool dirty { get; set; }
        internal abstract int _packetLength(int ledcount);
        byte[] rleBuf;

        public virtual void Send(bool force)
        {
            if (!dirty && !force)
            {
                return;
            }
            dirty = false;

            var max_packet_size = 1464;
            var compressed_frame = rleBuf.AsSpan();
            var raw_frame = buffer.AsSpan().Slice(header_len);
            var compressed_segments = RlePixelEncoder.RleEncode((byte)bytes_per_pixel, raw_frame, compressed_frame, max_packet_size);

            if (use_rle) // TODO: dynamically swap send functions
            {
                foreach(var seg in compressed_segments)
                {
                    var slice = compressed_frame.Slice(seg.start, seg.length);
                    client.SendAsync(slice.ToArray(), slice.Length, address).Wait();
                    var debugSlice = compressed_frame.Slice(header_len, seg.length - header_len);
                }
            }
            else
            {
                if (packet_len <= max_packet_size + header_len) // Don't use RLE - first frame
                {
                    buffer[0] = 0; // Command: new frame
                    buffer[1] = 0; // Format: raw
                    buffer[2] = (byte)(packet_len >> 8); // Packet len
                    buffer[3] = (byte)(packet_len & 0xFF);
                    buffer[4] = (byte)(led_count >> 8); // Pixel count
                    buffer[5] = (byte)(led_count & 0xFF);
                    client.SendAsync(buffer, packet_len, address);
                }
                else
                {
                    // split into multiple packets
                    var buf = new byte[max_packet_size + header_len]; // TODO: Do this with Span<T>?
                    var firstPacket = true;
                    var offset = 0;
                    var datalen = packet_len - header_len;

                    while (offset < datalen)
                    {
                        var count = Math.Min(datalen - offset, max_packet_size);
                        if (firstPacket)
                        {
                            buf[0] = 0; // Command: new frame
                            buf[1] = 0; // Format: raw
                            buf[4] = (byte)((led_count >> 8) & 0xFF); // Pixel count
                            buf[5] = (byte)(led_count & 0xFF);
                            firstPacket = false;
                        }
                        else
                        {
                            buf[0] = 1; // Command: existing frame
                            buf[1] = 0; // Format: raw
                            buf[4] = (byte)(((offset / bytes_per_pixel) >> 8) & 0xFF); // Starting pixel index
                            buf[5] = (byte)((offset / bytes_per_pixel) & 0xFF);
                        }

                        Array.Copy(buffer, offset + header_len, buf, header_len, count);
                        client.SendAsync(buf, count + header_len, address);
                        offset += count;
                    }
                }
            }
        }

        public virtual void DumpBuffer()
        {
            dirty = true;

            buffer[0] = 2; // Command: drop buffer
            buffer[1] = 0; 
            buffer[2] = 0; // Packet len
            buffer[3] = 6;
            buffer[4] = 0;
            buffer[5] = 0;
            client.SendAsync(buffer, 6, address);
        }

        public abstract RGBWColor this[int index] { get; set; }
    }
}
