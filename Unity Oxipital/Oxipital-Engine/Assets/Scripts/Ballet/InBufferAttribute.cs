using System;
using System.Reflection;
using UnityEngine;

namespace Oxipital
{
    public class InBuffer : Attribute
    {
        public int index;
        public bool useCustomFunc;

        public InBuffer(int index, bool useCustomFunc = false)
        {
            this.index = index;
            this.useCustomFunc = useCustomFunc;
        }
    }
}
