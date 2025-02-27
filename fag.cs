using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Karakterbog_GUI
{
    public class Fag
    {
        public string Navn { get; set; }
        public float Karakter { get; set; }

        public Fag(string navn, float karakter)
        {
            Navn = navn;
            Karakter = karakter;
        }

        public override string ToString()
        {
            return $"{Navn}: {Karakter}";
        }
    }
}