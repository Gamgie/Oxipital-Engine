using System;
using System.Reflection;

namespace Oxipital
{
    public class InBuffer : Attribute
    {
        public InBuffer(int index)
        {
            this.index = index;
        }
        public int index;
    }
}
