using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new dvrData())
            {
                // Create and save a new Blog 
                //Console.Write("Enter a name for a new Blog: ");
                db.dvrs.Add(new dvrInfo { dvrId="aa"});
                db.SaveChanges();  
                
            } 
        }
    }
}
