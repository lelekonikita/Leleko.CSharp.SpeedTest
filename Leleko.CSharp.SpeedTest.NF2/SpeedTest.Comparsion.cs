using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Leleko.CSharp
{
	/// <summary>
	/// Тесты скорости выполнения методов
	/// </summary>
	public partial class SpeedTest
	{
		/// <summary>
		/// Сравнить результаты тестов скорости
		/// </summary>
		/// <param name="speedTests">Speed tests.</param>
		public static Comparison Compare(params SpeedTest[] speedTests)
		{
			return new Comparison(speedTests);
		}

		/// <summary>
		/// Сравнить результаты тестов скорости
		/// </summary>
		/// <param name="speedTests">Speed tests.</param>
		public static Comparison Compare(IEnumerable<SpeedTest> speedTests)
		{
			return new Comparison(speedTests);
		}

		/// <summary>
		/// Сравнение результатов по пулу тестов
		/// </summary>
		public sealed class Comparison: IDictionary<string, SpeedTest>
		{
			/// <summary>
			/// The speed tests dictionary {Name,SpeedTest}
			/// </summary>
			readonly Dictionary<string, SpeedTest> speedTests;

			/// <summary>
			/// Initializes a new instance of the <see cref="Leleko.CSharp.SpeedTest+Comparison"/> class.
			/// </summary>
			/// <param name="speedTests">Speed tests.</param>
			internal Comparison(IEnumerable<SpeedTest> speedTests)
			{
				if (speedTests == null)
					throw new ArgumentNullException("speedTests");
				
				this.speedTests = new Dictionary<string, SpeedTest>();
				try
				{
					foreach (var e in speedTests)
						this.speedTests.Add(e.Name, e);
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException("Name of test not unique in dictionary", "speedTests", ex);
				}
				this.NeedRecalc = true;
			}

			/// <summary>
			/// Gets or sets a value indicating whether this <see cref="Leleko.CSharp.SpeedTest+Comparison"/> need recalc.
			/// </summary>
			/// <value><c>true</c> if need recalc; otherwise, <c>false</c>.</value>
			bool NeedRecalc { get; set; }

			/// <summary>
			/// The comparsion result (cache)
			/// </summary>
			string comparsionResult;

			/// <summary>
			/// Gets the result.
			/// </summary>
			/// <value>The result.</value>
			public string Result 
			{ 
				get 
				{ 
					if (this.NeedRecalc)
						this.RecalcResult();
					return this.comparsionResult; 
				} 
			}

			/// <summary>
			/// Пересчитываем результаты
			/// </summary>
			void RecalcResult()
			{
				lock ((this.speedTests as ICollection).SyncRoot)
				{
					SpeedTest[] speedTestsArr = new SpeedTest[this.speedTests.Count];
					this.speedTests.Values.CopyTo(speedTestsArr, 0);

					Array.Sort(speedTestsArr, (stA,stB) => -stA.Perfomance.CompareTo(stB.Perfomance));
					
					StringBuilder sb = new StringBuilder(128);
					sb.Append('{').Append(' ');
					var enumerator = speedTestsArr.GetEnumerator();
					if (enumerator.MoveNext())
					{

						var maxPerfomance = (enumerator.Current as SpeedTest).Perfomance;
						int i = 0;
						if (double.IsPositiveInfinity(maxPerfomance))
							maxPerfomance = double.MaxValue;
						if (maxPerfomance != 0)
						{
							do
							{
								var speedTest = enumerator.Current as SpeedTest;
								sb.Append(++i).Append('.').Append(speedTest.Name).AppendFormat("({0:N2}%) ",speedTest.Perfomance*100/maxPerfomance);
							} while (enumerator.MoveNext());
						}
					}
					sb.Append('}');
					
					this.comparsionResult = sb.ToString();
					this.NeedRecalc = false;
				}
			}

			/// <summary>
			/// Returns a <see cref="System.String"/> that represents the current <see cref="Leleko.CSharp.SpeedTest+Comparison"/>.
			/// </summary>
			/// <returns>A <see cref="System.String"/> that represents the current <see cref="Leleko.CSharp.SpeedTest+Comparison"/>.</returns>
			public override string ToString()
			{
				return string.Format("[Comparison({0}): Result={1}]", this.speedTests.Count, Result);
			}

			/// <summary>
			/// Add the specified value.
			/// </summary>
			/// <param name="value">Value.</param>
			public void Add(SpeedTest value)
			{
				if (value == null)
					throw new ArgumentNullException("value");
				(this as IDictionary<string, SpeedTest>).Add(value.Name, value);
			}

			/// <summary>
			/// Remove the specified speedTest.
			/// </summary>
			/// <param name="speedTest">Speed test.</param>
			public bool Remove(SpeedTest speedTest)
			{
				return (this as ICollection<KeyValuePair<string, SpeedTest>>).Remove(new KeyValuePair<string, SpeedTest>(speedTest.Name, speedTest));
			}

			/// <summary>
			/// Remove the specified key.
			/// </summary>
			/// <param name="key">Key.</param>
			public bool Remove(string key)
			{
				lock ((this.speedTests as IDictionary).SyncRoot)
				{
					var result = this.speedTests.Remove(key);
					this.NeedRecalc = result;
					return result;
				}
			}

			/// <summary>
			/// Gets the count.
			/// </summary>
			/// <value>The count.</value>
			public int Count { get { return this.speedTests.Count; } }

			/// <summary>
			/// Containses the key.
			/// </summary>
			/// <returns><c>true</c>, if key was containsed, <c>false</c> otherwise.</returns>
			/// <param name="key">Key.</param>
			public bool ContainsKey(string key) { return this.speedTests.ContainsKey(key); }

			/// <summary>
			/// Gets or sets the <see cref="Leleko.CSharp.SpeedTest+Comparison"/> with the specified key.
			/// </summary>
			/// <param name="key">Key.</param>
			public SpeedTest this[string key]
			{
				get { return this.speedTests[key]; }
				set { (this as IDictionary<string, SpeedTest>).Add(key, value); }
			}

			/// <summary>
			/// Gets the keys.
			/// </summary>
			/// <value>The keys.</value>
			public ICollection<string> Keys { get { return this.speedTests.Keys; } }

			/// <summary>
			/// Gets the values.
			/// </summary>
			/// <value>The values.</value>
			public ICollection<SpeedTest> Values { get { return this.speedTests.Values; } }

			/// <summary>
			/// Clear this instance.
			/// </summary>
			public void Clear()
			{
				lock ((this.speedTests as IDictionary).SyncRoot)
				{
					this.speedTests.Clear();
					this.NeedRecalc = true;
				}
			}

			/// <summary>
			/// Contains the specified item.
			/// </summary>
			/// <param name="item">Item.</param>
			public bool Contains(KeyValuePair<string, SpeedTest> item) { SpeedTest speedTest; return this.speedTests.TryGetValue(item.Key, out speedTest) && EqualityComparer<SpeedTest>.Default.Equals(speedTest, item.Value); }

			#region IDictionary implementation
			void IDictionary<string, SpeedTest>.Add(string key, SpeedTest value)
			{
				if (key == null)
					throw new ArgumentNullException("key");
				if (value == null)
					throw new ArgumentNullException("value");
				try
				{
					lock ((this.speedTests as IDictionary).SyncRoot)
						this.speedTests.Add(key, value);
					this.NeedRecalc = true;
				}
				catch (ArgumentException ex)
				{
					throw new ArgumentException("Name of test not unique in dictionary", "key", ex);
				}

			}
			bool IDictionary<string, SpeedTest>.TryGetValue(string key, out SpeedTest value) { return this.speedTests.TryGetValue(key, out value); }
			#endregion
			#region ICollection implementation
			void ICollection<KeyValuePair<string, SpeedTest>>.Add(KeyValuePair<string, SpeedTest> item) { (this as IDictionary<string, SpeedTest>).Add(item.Key, item.Value); }
			void ICollection<KeyValuePair<string, SpeedTest>>.CopyTo(KeyValuePair<string, SpeedTest>[] array, int arrayIndex) { foreach (var e in this.speedTests) array[arrayIndex++] = e; }
			bool ICollection<KeyValuePair<string, SpeedTest>>.Remove(KeyValuePair<string, SpeedTest> item) { return (this as ICollection<KeyValuePair<string, SpeedTest>>).Contains(item) && this.Remove(item.Key); }
			bool ICollection<KeyValuePair<string, SpeedTest>>.IsReadOnly { get { return false; }}
			#endregion
			#region IEnumerable implementation
			IEnumerator<KeyValuePair<string, SpeedTest>> IEnumerable<KeyValuePair<string, SpeedTest>>.GetEnumerator() { throw new NotImplementedException(); }
			#endregion
			#region IEnumerable implementation
			IEnumerator IEnumerable.GetEnumerator() { throw new NotImplementedException(); }
			#endregion
		}
	}
}

