using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using static ColorTurbine.Strip;

namespace ColorTurbine
{
    public class Particle
    {
        static Random r = new Random();
        public Particle(int led_count)
        {
            intensity = r.Next(10) + 2;
            if (r.NextDouble() < 0.5)
            {
                location = led_count + intensity;
            }
            else
            {
                location = -intensity;
            }

            while (intensityVelocity == 0 && acceleration == 0 && velocity == 0)
            {
                velocity = r.NextDouble() / 8;
                if (location > 0)
                {
                    velocity = -velocity;
                }
                acceleration = (r.NextDouble() - 0.5) / 100;
                intensityVelocity = (r.NextDouble() - 0.5) / 200;
            }
        }
        public double location { get; set; }
        public double velocity { get; set; }
        public double acceleration { get; set; }
        public double intensity { get; set; }
        public double intensityVelocity { get; set; }
        public RGBWColor color { get; set; }
        public void Render(IStrip s)
        {
            velocity += acceleration;
            location += velocity;
            intensity += intensityVelocity;

            var start = location - intensity;
            var end = location + intensity;

            s.Fill(start, end, color, FillType.Factor);
        }

        public bool IsVisible(IStrip s)
        {
            if (location + intensity < 0 || location - intensity > s.led_count)
            {
                return false;
            }
            if (intensity <= 0)
            {
                return false;
            }
            return true;
        }
    }

    public class Flow : IPlugin
    {
        List<Particle> particles = new List<Particle>();
        private int maxParticles;

        public delegate void NewParticleEventHandler(object sender, Particle e);
        public event NewParticleEventHandler OnNewParticle;

        private Palette palette;

        public Flow() : this(5)
        { }

        public async Task hourly()
        {
            Console.WriteLine("Requesting new palette");
            palette = await Services.ColorMind.GetRandomPalette();

            // HACK: 'graceful' fallback if colormind is down
            if(palette == null)
            {
                Random r = new Random();
                palette = new Palette();
                palette.palette = new RGBColor[5];
                for (int i = 0; i < palette.palette.Length; i++)
                {
                    byte[] buf = new byte[3];
                    r.NextBytes(buf);
                    palette.palette[i] = new RGBColor(buf[0], buf[1], buf[2]);
                }
            }
        }
        
        public override async void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);

            Services.Sun.OnSunrise += async (_) =>
            {
                await hourly();
            };
            RecurringJob.AddOrUpdate("hourly-flow-palette", () => hourly(), Cron.Hourly);
            await hourly();
        }

        public Flow(int maxParticles)
        {
            this.maxParticles = maxParticles;
        }

        public override bool NeedsRender()
        {
            return true;
        }

        public override async Task Render()
        {
            // Clean dead particles
            for (int i = 0; i < particles.Count; i++)
            {
                if (!particles[i].IsVisible(strip))
                {
                    particles.RemoveAt(i);
                    i--;
                }
            }

            for (int i = particles.Count; i < maxParticles; i++)
            {
                var particle = new Particle(strip.led_count);
                if (Services.Sun.NightMode)
                {
                    particle.color = randomPaletteColor();
                }
                else
                {
                    particle.color = randomPaletteColor();
                    // var palette = await Services.ColorMind.GetSuggestedPalette(/* TODO OTHER PARTICLES - sorted? */ null);
                    // particle.color = palette.ElementAt(4);
                }

                if (OnNewParticle != null)
                {
                    OnNewParticle(this, particle);
                }

                particles.Add(particle);
            }
            return;
        }

        Random r = new Random();
        private RGBWColor randomPaletteColor()
        {
            return new RGBWColor(palette.palette[r.Next(palette.palette.Length)]);
        }

        public override void Paint()
        {
            foreach (Particle p in particles)
            {
                p.Render(strip);
            }
        }
    }
}
