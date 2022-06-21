using UnityEngine;

namespace Unidice.Simulator.Utilities
{
    public class DestroyOn : MonoBehaviour
    {
        public enum On { Awake, Start }

        public On on;

        public Object[] objects;

        public void Awake()
        {
            if (on == On.Awake)
            {foreach (var o in objects)
                {
                    Destroy(o);
                }
            }
            Destroy(this);
        }

        public void Start()
        {
            if (on == On.Start)
                foreach (var o in objects)
                {
                    Destroy(o);
                }
            Destroy(this);
        }
    }
}