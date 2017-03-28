using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Setting
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public override string ToString()
        {
            return Key + ": " + Value;
        }
    }
}