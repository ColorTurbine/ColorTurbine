using System;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;

namespace ColorTurbine
{
    public class GameOfLife2D : IPlugin
    {
        RGBWStrip2D s2d;
        public override void Initialize(IStrip s, PluginConfig config)
        {
            base.Initialize(s, config);
            
            s2d = s as RGBWStrip2D;
            if (s2d == null)
            {
                throw new ArgumentException("Strip must be of type RGBWStrip2D");
            }
            width = s2d.width;
            height = s2d.height;
            state = new bool[width, height];

            // Generate random gamestate
            Random random = new Random();
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    state[x,y] = random.Next(3) == 0;
                }
            }
        }

        bool[,] state;
        int width;
        int height;

        private int CountAdjacent(bool[,] xstate, int x, int y) {
            int count = 0;
            for(int ix = x-1; ix <= x+1; ix++) {
                for(int iy = y-1; iy <= y+1; iy++) {
                    int testx = (width + ix)%width;
                    int testy = (height + iy)%height; 
                    if(testx == x && testy == y) {
                        continue;
                    }
                    if(xstate[testx, testy]) {
                        count++;
                    }
                }
            }
            return count;
        }

        private bool GolRules(bool[,] xstate, int x, int y) {
            var adj = CountAdjacent(state, x, y);

            // Live cell rules
            if(xstate[x,y]) {
                if(adj < 2) {
                    // Death by underpopulation
                    return false;
                } else if (adj < 4) {
                    // Lives
                    return true;
                } else {
                    // Death by overpopulation
                    return false;
                }
            } else {
                // Dead cell rules
                if(adj == 3) {
                    // Life by reproduction
                    return true;
                } else {
                    // Stays dead
                    return false;
                }
            }
        }

        int every;

        public override bool NeedsRender() {
            if((every++ % 8) == 1) {
                return true;
            }
            return false;
        }
        
        public override Task Render()
        {
            // Save gamestate
            bool[,] newstate = new bool[width, height];

            // Generate new gamestate
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newstate[x, y] = GolRules(state, x, y);
                }
            }

            // Copy gamestate
            state = newstate;

            return Task.CompletedTask;
        }

        public override void Paint()
        {
            // Pick color
            var color = new RGBWColor(25, 0, 0, 0);

            // Render
            for(int x = 0; x < width; x++) {
                for(int y = 0; y < height; y++) {
                    if(state[x,y]) {
                        s2d[x,y] = color;
                    } else {
                        s2d[x,y] = new RGBWColor(0,0,0,0);
                    }
                }
            }
        }
    }
}
