using System;
using System.Net;

namespace ColorTurbine
{
    public class RGBStrip : Strip
    {   
        public RGBStrip(string name, IPAddress address, int led_count, bool enable_rle = true)
            : base(address, led_count, enable_rle)
        {
            bytes_per_pixel = 3;
        }

        internal override int _packetLength(int ledcount)
        {
            return ledcount * 3;
        }

        public override RGBWColor this[int index]
        {
            get
            {
                return new RGBWColor(buffer[header_len + index * 3],
                                    buffer[header_len + index * 3 + 1],
                                    buffer[header_len + index * 3 + 2], 0);
            }
            set
            {
                dirty = true;
                var color = value.toRGB();
                buffer[header_len + index * 3] = color.r;
                buffer[header_len + index * 3 + 1] = color.g;
                buffer[header_len + index * 3 + 2] = color.b;
            }
        }
    }
}
