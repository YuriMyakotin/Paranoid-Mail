using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Paranoid
{
    public class NamedValue
    {
        public int Value { get; set; }
        public string ValueName { get; set; }

        public NamedValue(int V, string N)
        {
            Value = V;
            ValueName = N;
        }
    }

}
