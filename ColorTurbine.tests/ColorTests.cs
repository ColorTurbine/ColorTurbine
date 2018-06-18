using System;
using ColorTurbine;
using Xunit;

namespace ColorTurbine.tests
{
    public class ColorTests
    {
        [Fact]
        public void RGBWColors_should_add()
        {
            var a = new RGBWColor(1, 2, 3, 4);
            var b = new RGBWColor(10, 12, 14, 16);
            Assert.Equal(new RGBWColor(11, 14, 17, 20), a + b);
        }

        [Fact]
        public void RGBWColors_should_multiply()
        {
            var negative = new RGBWColor(1, 2, 3, 4) * -1;
            Assert.Equal(new RGBWColor(0, 0, 0, 0), negative);

            var dbl = new RGBWColor(1, 2, 3, 4) * 2;
            Assert.Equal(new RGBWColor(2, 4, 6, 8), dbl);

            var max = new RGBWColor(1, 2, 3, 4) * 1000;
            Assert.Equal(new RGBWColor(255, 255, 255, 255), max);
        }
    }
}
