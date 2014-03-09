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

			// подготавливаем таблицу тестов [ имя/функция ]
			var tests = new Dictionary<string, Func<bool>> 
			{
				{ "Math.Pow", () => { Math.Pow(x++,1.01); return true; } },
				{ "Math.Sqrt",() => { Math.Sqrt(x++); return true; } },
				{ "Math.Log", () => { Math.Log(x++); return true; } },
				{ "Math.Log10",() => { Math.Log10(x++); return true; } },
			};

			x = Math.PI;

			// создаем сравнение тестов
			var compareTest = SpeedTest.Compare();

			// параллельно выполняем тесты
			Parallel.ForEach(tests, (test) => { var testResult = SpeedTest.Make(test.Key,null,10,test.Value); compareTest.Add(testResult); Console.WriteLine(testResult); });

			// Выводим сравнительные результаты
			Console.WriteLine();
			Console.WriteLine(" Общее сравнение производительности ");
			Console.WriteLine(compareTest);
		}
	}
}
