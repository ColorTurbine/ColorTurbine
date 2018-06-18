using System;

namespace ColorTurbine
{
    public abstract class ColorStyle
    {
        public abstract RGBWColor BestColor { get; }
        public abstract RGBWColor GoodColor { get; }
        public abstract RGBWColor MidColor { get; }
        public abstract RGBWColor BadColor { get; }

        public RGBWColor MapGoodnessToColor(double goodness)
        {
            if (goodness <= 0)
            {
                return BadColor;
            }

            if (goodness > 2)
            {
                // Cap goodness at best
                if (goodness >= 3)
                {
                    return BestColor;
                }

                // Fade between good & best
                return (RGBWColor)(GoodColor * (3 - goodness) + BestColor * (goodness - 2));
            }

            // Fade between mid & good
            if (goodness <= 2 && goodness > 1)
            {
                return (RGBWColor)(MidColor * (2 - goodness) + GoodColor * (goodness - 1));
            }

            // Fade between bad & mid
            if (goodness <= 1 && goodness > 0)
            {
                return (RGBWColor)(BadColor * (1 - goodness) + MidColor * goodness);
            }

            throw new System.Exception($"Unhandled goodness value: {goodness}");
        }
    }

    // TODO: Get color styles into config
    class NightColors : ColorStyle
    {
        public override RGBWColor BestColor { get; } = new RGBWColor(0, 0, 0, 0);

        public override RGBWColor GoodColor { get; } = new RGBWColor(0, 0, 0, 0);

        public override RGBWColor MidColor { get; } = new RGBWColor(20, 20, 0, 0);

        public override RGBWColor BadColor { get; } = new RGBWColor(20, 0, 0, 0);
    }

    class VisibleNightColors : ColorStyle
    {
        public override RGBWColor BestColor { get; } = new RGBWColor(25, 0, 0, 0);

        public override RGBWColor GoodColor { get; } = new RGBWColor(20, 0, 0, 0);

        public override RGBWColor MidColor { get; } = new RGBWColor(15, 15, 0, 0);

        public override RGBWColor BadColor { get; } = new RGBWColor(10, 0, 0, 0);
    }

    class DayColors : ColorStyle
    {
        public override RGBWColor BestColor { get; } = new RGBWColor(0, 0, 50, 0);

        public override RGBWColor GoodColor { get; } = new RGBWColor(0, 50, 0, 0);

        public override RGBWColor MidColor { get; } = new RGBWColor(25, 25, 0, 0);

        public override RGBWColor BadColor { get; } = new RGBWColor(25, 0, 0, 0);
    }

    public class ThemeService
    {
        NightColors nc = new NightColors();
        VisibleNightColors vnc = new VisibleNightColors();
        DayColors dc = new DayColors();
        public ColorStyle GetColorStyle(bool forceVisible = false)
        {
            if (Services.Sun.NightMode)
            {
                if (forceVisible)
                {
                    return vnc;
                }
                return nc;
            }
            else
            {
                return dc;
            }
        }

        Random r = new Random();
        public RGBWColor GetRandomAccent()
        {
            if(Services.Sun.NightMode)
            {
                var bytes = new byte[1];
                r.NextBytes(bytes);
                return new RGBWColor(bytes[0], 0, 0, 0);
            }

            var other = new byte[4];
            r.NextBytes(other);
            return new RGBWColor(other[0], other[1], other[2], other[3]); // TODO: Use a little color theory
        }
    }
}