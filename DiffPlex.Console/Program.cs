using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiffPlex.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Enabled = false;
            Differ d = new Differ();
            var diffresult = d.CreateLineDiffs(A, B,false);
            var formater = new UnidiffFormater();
            var output = formater.Generate(diffresult);
            foreach (var line in output)
                Console.WriteLine(line);
           
        }

        static string A =
@"We the people
of the united states of america
establish justice
ensure domestic tranquility
provide for the common defence
secure the blessing of liberty
to ourselves and our posterity
";

        static string B =
@"We the people
in order to form a more perfect union
establish justice
ensure domestic tranquility
promote the general welfare and
secure the blessing of liberty
to ourselves and our posterity
do ordain and establish this constitution
for the United States of America




";
    }
}
