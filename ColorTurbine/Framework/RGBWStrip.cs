using System;
using System.Net;

namespace ColorTurbine
{
    public class RGBWStrip : Strip
    {
        public RGBWStrip(string name, IPAddress address, int led_count, bool enable_rle = true)
            : base(address, led_count, enable_rle)
        {
            bytes_per_pixel = 4;
        }

        internal override int _packetLength(int ledcount)
        {
            return ledcount * 4;
        }

        public override RGBWColor this[int index]
        {
            get
            {
                return new RGBWColor(buffer[header_len + index * 4],
                                    buffer[header_len + index * 4 + 1],
                                    buffer[header_len + index * 4 + 2],
                                    buffer[header_len + index * 4 + 3]);
            }
            set
            {
                // TODO: Use the memory slicing coming out in .Net Core 2.1
                dirty = true;
                buffer[header_len + index * 4] = value.r;
                buffer[header_len + index * 4 + 1] = value.g;
                buffer[header_len + index * 4 + 2] = value.b;
                buffer[header_len + index * 4 + 3] = value.w;
            }
        }
    }
}
