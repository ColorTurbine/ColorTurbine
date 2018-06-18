using System;
using System.Threading.Tasks;

namespace ColorTurbine
{

    public class Clock : IPlugin
    {
        public int lastSecond = 0;
        
        public override bool NeedsRender()
        {
            // if (DateTime.UtcNow.Second == lastSecond)
            // {
            //     return false;
            // }
            // lastSecond = DateTime.UtcNow.Second;
            return true;
        }

        public override Task Render()
        {
            return Task.CompletedTask;
        }

        public override void Paint()
        {
            var time = DateTime.UtcNow;

            var minuteColor = new RGBWColor(255, 0, 0, 0);
            var hourColor = new RGBWColor(0, 0, 255, 0);
            var secondColor = new RGBWColor(255, 255, 255, 255);
            if (Services.Sun.NightMode)
            {
                minuteColor = new RGBWColor(1, 0, 0, 0);
                hourColor = new RGBWColor(1, 0, 0, 0);
                secondColor = new RGBWColor(2, 0, 0, 0);
            }

            for (int i = 0; i < strip.led_count; i++)
            {
                strip[i] = new RGBWColor(0, 0, 0, 0);
            }
            for (int i = 0; i < time.Minute; i += 10)
            {
                strip[i] += minuteColor;
            }
            for (int i = 0; i <= time.Minute; i++)
            {
                strip[i] += minuteColor;
            }

            for (int i = 0; i <= time.Hour; i++)
            {
                strip[i] += hourColor;
            }

            for (int i = 0; i < strip.led_count; i++)
            {
                strip[i] *= Services.Sun.CalculateSunPosition().brtscl;
            }

            strip[time.Second] = secondColor; // * (time.Millisecond / 1000.0);
        }
    }
}
