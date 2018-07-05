using System;

namespace ColorTurbine
{
    public struct RGBWColor
    {
        public readonly byte r;
        public readonly byte g;
        public readonly byte b;
        public readonly byte w;

        public RGBWColor(byte r, byte g, byte b, byte w)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.w = w;
        }

        public RGBWColor(RGBColor rgb)
        {
            this.r = rgb.r;
            this.g = rgb.g;
            this.b = rgb.b;
            this.w = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is RGBWColor)
            {
                var cobj = (RGBWColor)obj;
                return cobj.r == r &&
                        cobj.g == g &&
                        cobj.b == b &&
                        cobj.w == w;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return $"{r} {g} {b} {w}".GetHashCode();
        }

        public RGBColor toRGB()
        {
            return new RGBColor(r, g, b); // TODO: Blend W into RGB
        }

        internal static byte between(int value, byte min, byte max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return (byte)value;
        }

        public static RGBWColor operator +(RGBWColor c1, RGBWColor c2)
        {
            return new RGBWColor(between(c1.r + c2.r, 0, 255),
                                between(c1.g + c2.g, 0, 255),
                                between(c1.b + c2.b, 0, 255),
                                between(c1.w + c2.w, 0, 255));
        }

        public static RGBWColor operator *(RGBWColor c1, double s)
        {
            return new RGBWColor(between((int)Math.Round(c1.r * s), 0, 255),
                                between((int)Math.Round(c1.g * s), 0, 255),
                                between((int)Math.Round(c1.b * s), 0, 255),
                                between((int)Math.Round(c1.w * s), 0, 255));
        }


        public static RGBWColor Parse(string color)
        {
            if (color == null)
            {
                throw new ArgumentNullException("no string given for color");
            }
            var parts = color.Replace("(", "").Replace(")", "").Split(",");
            return new RGBWColor(byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]), byte.Parse(parts[3]));
        }

        public static RGBWColor FromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = (byte)(Convert.ToInt32(value) & 0xFF000000 >> 24);
            byte p = (byte)(Convert.ToInt32(value * (1 - saturation)) & 0xFF000000 >> 24);
            byte q = (byte)(Convert.ToInt32(value * (1 - f * saturation)) & 0xFF000000 >> 24);
            byte t = (byte)(Convert.ToInt32(value * (1 - (1 - f) * saturation)) & 0xFF000000 >> 24);

            if (hi == 0)
                return new RGBWColor(v, t, p, 0);
            else if (hi == 1)
                return new RGBWColor(q, v, p, 0);
            else if (hi == 2)
                return new RGBWColor(p, v, t, 0);
            else if (hi == 3)
                return new RGBWColor(p, q, v, 0);
            else if (hi == 4)
                return new RGBWColor(t, p, v, 0);
            else
                return new RGBWColor(v, p, q, 0);
        }
        
        public static RGBWColor WarmWhite = new RGBWColor(255, 230, 200, 200);
    }
}
