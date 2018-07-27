using System;

namespace ColorTurbine
{
/*
Palettes:
    - may be based on a set of colors (fixed - night, vibrant, pastels, etc)
    - may be randomly generated
    - may change over time (time of day)

Palettes should follow & extend the ColorStyle API

 */

    public class PaletteGenerator
    {
        Random r;
        public PaletteGenerator()
        {
            r = new Random();
        }
        public RGBWColor Next()
        {
            // Take into account:
            //  - Night mode
            //  - Recently given colors (temporally)

            var bytes = new byte[4];
            r.NextBytes(bytes);
            return new RGBWColor(bytes[0], bytes[1], bytes[2], bytes[3]);
        }
    };
}
