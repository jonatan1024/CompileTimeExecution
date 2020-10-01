using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
#if CompileTimeExecution
using CompileTimeExecution;
#endif

namespace CompileTimeExecutionTests
{
    [TestClass]
    public partial class CollectionsTest
    {
        static int Factorial(int x) => x <= 1 ? 1 : Factorial(x - 1) * x;

#if CompileTimeExecution
        
        [CompileTimeExecution]
        static IReadOnlyList<int> Fac10List
        {
            get
            {
                var facs = new List<int>();
                for(int x = 1; x <= 10; x++)
                {
                    facs.Add(Factorial(x));
                }
                return facs;
            }
        }
        
        [CompileTimeExecution]
        static Dictionary<int, string> Numbers
        {
            get
            {
                var numbers = new Dictionary<int, string>();
                for(int x = 1; x <= 10; x++)
                {
                    numbers.Add(x, string.Concat(Enumerable.Repeat(x, x)));
                }
                return numbers;
            }
        }
        
        [CompileTimeExecution(deserialize: true)]
        static List<KeyValuePair<string, string>> Pairs {
            get
            {
                var pairs = new List<KeyValuePair<string, string>>();
                pairs.Add(new KeyValuePair<string, string>("key", "value"));
                return pairs;
            }
        }
#endif

        [TestMethod]
        public void Test()
        {
            for(int x = 1; x <= 10; x++)
            {
                Assert.AreEqual(Factorial(x), Fac10List[x - 1]);
            }

            Assert.AreEqual("1", Numbers[1]);
            Assert.AreEqual(9, Numbers[9].Length);
            Assert.AreEqual(20, Numbers[10].Length);

            Assert.AreEqual("key", Pairs[0].Key);
            Assert.AreEqual("value", Pairs[0].Value);
        }
    }
}
