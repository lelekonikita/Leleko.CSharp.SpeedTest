using System;
using System.Linq;
using Leleko.CSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleRun
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			double x = default(double);


			var tests = new Dictionary<string, Func<bool>> 
			{
				{ "Math.Pow", () => { Math.Pow(x++,1.01); return true; } },
				{ "Math.Sqrt",() => { Math.Sqrt(x++); return true; } },
				{ "Math.Pow2", () => { Math.Pow(x++,1.01); return true; } },
				{ "Math.Sqrt2",() => { Math.Sqrt(x++); return true; } },
			};
			//x = Math.PI;
			//foreach(var e in SpeedTest.Make(null,10,tests).AsParallel())
			//	Console.WriteLine(e);
			//Console.WriteLine(" ------ ");

			x = Math.PI;
			Parallel.ForEach(tests, (test) => Console.WriteLine(SpeedTest.Make(test.Key,null,10,test.Value)));
		}
	}
}
