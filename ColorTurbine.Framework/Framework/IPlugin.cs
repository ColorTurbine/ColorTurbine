using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorTurbine
{
    public abstract class IPlugin
    {
        private bool _enabled;
        public string Name { get; set; }
        public bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                if (_enabled)
                {
                    OnEnable();
                }
                else
                {
                    OnDisable();
                }
            }
        }

        public List<string> tags { get; set; } = new List<string>();

        public virtual void Initialize(IStrip s, PluginConfig config)
        {
            this.strip = s;
        }

        public virtual IStrip strip { get; set; }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        public abstract void Paint();
        public abstract bool NeedsRender();
        public abstract Task Render();
    }
}