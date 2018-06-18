using System;
using System.Threading.Tasks;

namespace ColorTurbine
{
    public class FloodFill : IPlugin
    {
        private RGBWColor _color;
        public RGBWColor Color
        {
            get { return _color; }
            set
            {
                _color = value;
                smudged = true;
            }
        }

        public FloodFill() { }

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            this.Color = RGBWColor.Parse((string)config["color"]);
        }

        public override void OnEnable()
        {
            smudged = true;
        }

        bool smudged = true;
        public override bool NeedsRender()
        {
            return smudged;
        }

        public override Task Render()
        {
            smudged = false;
            return Task.CompletedTask;
        }

        public override void Paint()
        {
            strip.Fill(0, strip.led_count - 1, Color);
        }
    }
}
