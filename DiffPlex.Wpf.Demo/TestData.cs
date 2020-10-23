using System;
using System.Collections.Generic;
using System.Text;

namespace DiffPlex.Wpf.Demo
{
    static class TestData
    {
        internal static string DuplicateText(string text, int repeat = 2)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < repeat; i++)
            {
                sb.Append(text);
            }

            return sb.ToString();
        }


        internal const string OldText =
            @"ABCDEFG hijklmn 01234567 _!# 98?

We the people
Lorem ipsum dolor sit amet, 
consectetur adipiscing elit, 
sed do eiusmod tempor incididunt
ut labore et dolore magna aliqua. 
Ut enim ad minim veniam, quis nostrud exercitation 
ullamco laboris nisi ut aliquip ex ea commodo consequat. 
Duis aute irure dolor in reprehenderit in voluptate velit esse c
illum dolore eu fugiat nulla pariatur. 
Excepteur sint occaecat cupidatat non proident, 
sunt in culpa qui officia deserunt mollit anim id est laborum

=======
";

        internal const string NewText =
            @"ABCDEFG hijklmn 01234567 _!# 98?

We 1the people
Lorem ipsum dolor sit amet, 
consectetur adipiscing elit, 
sed do eiusmod tempor incididunt
ut labore et dolore magna aliqua. 
Ut enim ad minim veniam, quis nostrud exercitation 
ullamco laboris nisi ut aliquip ex ea commodo consequat. 
asasa aute irure dolor in reprehenderit in voluptate velit esse c
illum dolore eu fugiat nulla pariatur. 
Excepteur sint occaecat cupidatat non proident, 
sunt in culpa qui officia deserunt mollit anim id est laborum




=======
";
//        internal const string OldText =
//            @"ABCDEFG hijklmn 01234567 _!# 98?

//We the people
//of the united states of america
//establish justice
//ensure domestic tranquility
//provide for the common defence
//secure the blessing of liberty
//to ourselves and our posterity

//=======
//";

//        internal const string NewText =
//            @"ABCDEFG opq rst uvw xyz 01234567 _&^ 98.

//We the people
//in order to form a more perfect union
//establish justice
//ensure domestic tranquility
//promote the general welfare and
//secure the blessing of liberty
//to ourselves and our posterity
//do ordain and establish this constitution
//for the United States of America




//=======
//";
    }
}
