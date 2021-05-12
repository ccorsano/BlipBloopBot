using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlipBloopWeb.Model
{
    public class CommandItemModel
    {
        public Guid? Id { get; set; }
        public string Name { get; set; }
        public List<string> Aliases { get; set; }
        public string Type { get; set; }
        public List<CommandProperty> Parameters { get; set; }
    }
}
