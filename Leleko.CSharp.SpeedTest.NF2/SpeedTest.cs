using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using System.Text;

namespace Leleko.CSharp
{
	/// <summary>
	/// Тесты скорости выполнения методов
	/// </summary>
	public partial class SpeedTest: IComparable<SpeedTest>
	{
		#region [ Properties ]

		#region [ Staticstics ]

		/// <summary>
		/// get имя теста
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; private set; }

		/// <summary>
		/// Выполняемое действие result:{true:продолжить,false:прервать}
		/// </summary>
		/// <value>The func.</value>
		public Func<bool> Function { get; private set; }

		/// <summary>
		/// get кол-во выполненных итераций
		/// </summary>
		/// <value>The repeats.</value>
		public double Repeats { get; private set; }

		/// <summary>
		/// get производительность: кол-во итераций в секунду
		/// </summary>
		/// <value>The repeats per second.</value>
		public double Perfomance { get { return (this.Repeats/this.Ticks)*TimeSpan.TicksPerSecond; } }

		/// <summary>
		/// get кол-во прошедших тиков
		/// </summary>
		/// <value>The ticks.</value>
		public long Ticks { get; private set; }

		/// <summary>
		/// get кол-во прошедших секунд
		/// </summary>
		/// <value>The seconds.</value>
		public long Seconds { get { return (long)(this.Ticks/TimeSpan.TicksPerSecond); } }

		/// <summary>
		/// get прошедшее время
		/// </summary>
		/// <value>The time.</value>
		public TimeSpan Time { get { return new TimeSpan(this.Ticks); } }

		#endregion

		#region [ Constraints ]

		/// <summary>
		/// get лимит на кол-во итераций
		/// </summary>
		/// <value>The max repeats.</value>
		public long? MaxRepeats { get; private set; }

		/// <summary>
		/// get лими на кол-во тиков
		/// </summary>
		/// <value>The max ticks.</value>
		public long? MaxTicks { get; private set; }

		#endregion

		#endregion

		#region [ Internal Static Methods ]

		/// <summary>
		/// Тест ограниченный только функцией {do while(func())}
		/// </summary>
		/// <returns>The only func.</returns>
		/// <param name="func">Func.</param>
		static SpeedTest MakeOnlyFunc(Func<bool> func)
		{
			double repeats = 0;

			Stopwatch sw = Stopwatch.StartNew();
			for(;func();repeats++);
			sw.Stop();

			return new SpeedTest
			{
				Repeats = repeats,
				Ticks = sw.ElapsedTicks
			};
		}

		/// <summary>
		/// Тест ограниченный функцией и лимитом итераций
		/// </summary>
		/// <returns>The repeats limit.</returns>
		/// <param name="maxRepeats">Max repeats.</param>
		/// <param name="func">Func.</param>
		static SpeedTest MakeRepeatsLimit(long maxRepeats, Func<bool> func)
		{
			double repeats = 0, limitRepeats = maxRepeats;
			
			Stopwatch sw = Stopwatch.StartNew();
			for(;func() && (repeats < limitRepeats) ;repeats++);
			sw.Stop();
			
			return new SpeedTest
			{
				Repeats = repeats,
				Ticks = sw.ElapsedTicks,
			};
		}

		/// <summary>
		/// Тест ограниченный функцией и лимитом времени
		/// </summary>
		/// <returns>The ticks limit.</returns>
		/// <param name="maxTicks">Max ticks.</param>
		/// <param name="func">Func.</param>
		static SpeedTest MakeTicksLimit(long maxTicks, Func<bool> func)
		{
			double repeats = 0;
			
			Stopwatch sw = Stopwatch.StartNew();
			for(;func() && (sw.ElapsedTicks < maxTicks);repeats++);
			sw.Stop();
			
			return new SpeedTest
			{
				Repeats = repeats,
				Ticks = sw.ElapsedTicks,
			};
		}

		/// <summary>
		/// Тест ограниченный функцией, лимитами времени и итераций
		/// </summary>
		/// <returns>The all.</returns>
		/// <param name="maxRepeats">Max repeats.</param>
		/// <param name="maxTicks">Max ticks.</param>
		/// <param name="func">Func.</param>
		static SpeedTest MakeAll(long maxRepeats, long maxTicks, Func<bool> func)
		{
			double repeats = 0, limitRepeats = maxRepeats;
			
			Stopwatch sw = Stopwatch.StartNew();
			for(;func() && (repeats < limitRepeats) && (sw.ElapsedTicks < maxTicks);repeats++);
			sw.Stop();
			
			return new SpeedTest
			{
				Repeats = repeats,
				Ticks = sw.ElapsedTicks,
			};
		}

		#endregion

		/// <summary>
		/// Выполнить тест
		/// </summary>
		/// <param name="name">имя теста</param>
		/// <param name="maxRepeats">максимальное число итераций</param>
		/// <param name="maxTicks">максимальное число тиков</param>
		/// <param name="func">выполняемое действие result:{true:продолжить,false:прервать}</param>
		public static SpeedTest Make(string name, long? maxRepeats, TimeSpan? maxTime, Func<bool> func)
		{
			if (func == null)
				throw new ArgumentNullException("func");

			var result = 
				(maxRepeats != null) 
					? (maxTime != null) ? MakeAll((long)maxRepeats,(long)maxTime.Value.Ticks,func) : MakeRepeatsLimit((long)maxRepeats, func)
					: (maxTime != null) ? MakeTicksLimit((long)maxTime.Value.Ticks, func) : MakeOnlyFunc(func)
				;
			result.Name = name;
			result.Function = func;
			result.MaxRepeats = maxRepeats;

			if (maxTime != null)
				result.MaxTicks = maxTime.Value.Ticks;

			return result;
		}

		/// <summary>
		/// Выполнить тест
		/// </summary>
		/// <param name="name">имя теста</param>
		/// <param name="maxRepeats">максимальное число итераций</param>
		/// <param name="maxSeconds">максимальное число секунд</param>
		/// <param name="func">выполняемое действие result:{true:продолжить,false:прервать}</param>
		public static SpeedTest Make(string name, long? maxRepeats, int maxSeconds, Func<bool> func)
		{
			return Make(name, maxRepeats, new TimeSpan(0,0,maxSeconds), func);
		}

		/// <summary>
		/// Выполнить группу тестов
		/// </summary>
		/// <param name="name">имя теста</param>
		/// <param name="maxRepeats">максимальное число итераций</param>
		/// <param name="maxTicks">максимальное число тиков</param>
		/// <param name="funcs">выполняемые действия result:{true:продолжить,false:прервать}</param>
		public static IEnumerable<SpeedTest> Make(long? maxRepeats, TimeSpan? maxTime, IEnumerable<KeyValuePair<string,Func<bool>>> funcs)
		{
			if (funcs == null)
				throw new ArgumentNullException("funcs");
			
			foreach(var func in funcs)
				yield return Make(func.Key, maxRepeats, maxTime, func.Value);
		}

		/// <summary>
		/// Выполнить группу тестов
		/// </summary>
		/// <param name="name">имя теста</param>
		/// <param name="maxRepeats">максимальное число итераций</param>
		/// <param name="maxSeconds">максимальное число тиков</param>
		/// <param name="funcs">выполняемые действия result:{true:продолжить,false:прервать}</param>
		public static IEnumerable<SpeedTest> Make(long? maxRepeats, int maxSeconds, IEnumerable<KeyValuePair<string,Func<bool>>> funcs)
		{
			return Make(maxRepeats, new TimeSpan(0,0,maxSeconds), funcs);
		}

		#region IComparable implementation
		/// <Docs>To be added.</Docs>
		/// <para>Returns the sort order of the current instance compared to the specified object.</para>
		/// <summary>
		/// Compares to.
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="other">Other.</param>
		public int CompareTo(SpeedTest other)
		{
			if (other == null)
				throw new ArgumentNullException("other");

			return this.Perfomance.CompareTo(other.Perfomance);
		}
		#endregion

		/// <summary>
		/// Строковое представление
		/// </summary>
		/// <returns>строковое представление</returns>
		public override string ToString()
		{
			return 
				string.Format(
					"SpeedTest:{{ Name:{0}, Time:{1}, Repeats:{2}, Perfomance:{3:E3}, Limits:{{ Ticks:{4}, Repeats:{5} }} }}", 
					this.Name ?? this.Function.Method.Name, 
					this.Time,
					this.Repeats,
					this.Perfomance,
					this.MaxTicks,
					this.MaxRepeats
				);
		}
	}


}

