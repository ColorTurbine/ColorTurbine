using System;
using System.Threading.Tasks;

namespace ColorTurbine
{
    public class PartyMode : IPlugin
    {
        public override bool NeedsRender()
        {
            return true;
        }

        double count = 0;
        public override Task Render()
        {
            count += 0.1; // Speed
            return Task.CompletedTask;
        }

        public override void Paint()
        {
            for (int i = 0; i < strip.led_count; i++)
            {
                strip[i] = RGBWColor.FromHSV(i + count, 1, 1);
            }
        }
    }
}
