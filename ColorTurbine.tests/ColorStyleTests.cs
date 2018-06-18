using System;
using ColorTurbine;
using Xunit;

namespace ColorTurbine.tests
{
    public class ColorStyleTests
    {
        class colorstyle_test : ColorStyle
        {
            public override RGBWColor BestColor => new RGBWColor(0, 0, 0, 100);

            public override RGBWColor GoodColor => new RGBWColor(0, 0, 100, 0);

            public override RGBWColor MidColor => new RGBWColor(0, 100, 0, 0);

            public override RGBWColor BadColor => new RGBWColor(100, 0, 0, 0);
        }

        [Fact]
        public void Color_style_points_are_correct()
        {
            var c = new colorstyle_test();
            Assert.Equal(c.BadColor, c.MapGoodnessToColor(0));
            Assert.Equal(c.MidColor, c.MapGoodnessToColor(1));
            Assert.Equal(c.GoodColor, c.MapGoodnessToColor(2));
            Assert.Equal(c.BestColor, c.MapGoodnessToColor(3));
        }

        [Fact]
        public void Color_style_fades_should_work()
        {
            var c = new colorstyle_test();
            Assert.Equal(c.BadColor * 0.75 + c.MidColor * 0.25, c.MapGoodnessToColor(0.25));
            Assert.Equal(c.BadColor * 0.5 + c.MidColor * 0.5, c.MapGoodnessToColor(0.5));
            Assert.Equal(c.MidColor * 0.75 + c.GoodColor * 0.25, c.MapGoodnessToColor(1.25));
            Assert.Equal(c.MidColor * 0.5 + c.GoodColor * 0.5, c.MapGoodnessToColor(1.5));
            Assert.Equal(c.GoodColor * 0.75 + c.BestColor * 0.25, c.MapGoodnessToColor(2.25));
            Assert.Equal(c.GoodColor * 0.5 + c.BestColor * 0.5, c.MapGoodnessToColor(2.5));
            Assert.Equal(c.BestColor, c.MapGoodnessToColor(3.5));
        }
    }
}
