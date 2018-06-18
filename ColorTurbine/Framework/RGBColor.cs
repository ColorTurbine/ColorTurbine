using System;

namespace ColorTurbine
{
    public struct RGBColor
    {
        public readonly byte r;
        public readonly byte g;
        public readonly byte b;

        public RGBColor(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public RGBColor(RGBWColor c)
        {
            var rgb = c.toRGB();
            this.r = rgb.r;
            this.g = rgb.g;
            this.b = rgb.b;
        }

        public override bool Equals(object obj)
        {
            RGBColor cobj;
            if (obj is RGBColor)
            {
                cobj = (RGBColor)obj;
                return cobj.r == r &&
                        cobj.g == g &&
                        cobj.b == b;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return $"{r} {g} {b}".GetHashCode();
        }
    }
}
