using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopWeb.Model
{
    public record CommandProperty
    {
        public bool IsNew { get; set; }
        public bool IsEditing { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
