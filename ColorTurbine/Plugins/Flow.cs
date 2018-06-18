using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public Flow() : this(5)
        { }

        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);

            // TODO: Configure
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
                if(Services.Sun.NightMode)
                {
                    particle.color = Services.Theme.GetRandomAccent();
                }
                else
                {
                    particle.color = Services.Theme.GetRandomAccent();
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

        public override void Paint()
        {
            foreach (Particle p in particles)
            {
                p.Render(strip);
            }
        }
    }
}
