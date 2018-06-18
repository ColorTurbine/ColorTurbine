using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorTurbine
{
    public class ColorMindService
    {
        private class ColorMindAPI
        {

        }
        
        public Task<IEnumerable<string>> GetModelList()
        {
            return null;
        }

        public async Task<IEnumerable<RGBWColor>> GetRandomPalette(string model = "default")
        {
            return null;
        }

        public async Task<IEnumerable<RGBWColor>> GetSuggestedPalette(IEnumerable<RGBWColor> input)
        {
            return null;
        }
    }
}
