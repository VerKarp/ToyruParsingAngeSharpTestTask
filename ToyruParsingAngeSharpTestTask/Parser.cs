using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToyruParsingAngeSharpTestTask
{
    class Parser
    {
        const string address = "https://www.toy.ru/catalog/boy_transport/";
        readonly IConfiguration config = Configuration.Default.WithDefaultLoader();

        public List<ToyInfo> Toys { get; set; } = new();
    }
}