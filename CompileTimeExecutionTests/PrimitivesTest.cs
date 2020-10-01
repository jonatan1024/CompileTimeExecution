using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
#if CompileTimeExecution
using CompileTimeExecution;
#endif

namespace CompileTimeExecutionTests
{
    [TestClass]
    public partial class PrimitivesTest
    {
#if CompileTimeExecution
        [CompileTimeExecution]
        static string CompiledAt => DateTime.Now.ToString();

        [CompileTimeExecution]
        static DayOfWeek CompiledAtDay => DateTime.Now.DayOfWeek;

        [CompileTimeExecution]
        static int CompiledAtYear => DateTime.Now.Year;

        [CompileTimeExecution]
        static double CompiledAtTimeOfDay => DateTime.Now.TimeOfDay.TotalDays;

        [CompileTimeExecution]
        static bool CompiledAtMonday => DateTime.Now.DayOfWeek == DayOfWeek.Monday;

        [CompileTimeExecution]
        static uint MaxUInt => uint.MaxValue;

        [CompileTimeExecution]
        static int MinInt => int.MinValue;

        [CompileTimeExecution]
        static byte MaxByte => byte.MaxValue;

        [CompileTimeExecution]
        static sbyte MinSByte => sbyte.MinValue;

        [CompileTimeExecution]
        static ulong MaxULong => ulong.MaxValue;

        [CompileTimeExecution]
        static long MinLong => long.MinValue;

        [CompileTimeExecution]
        static double DoubleEpsilon => double.Epsilon;

        [CompileTimeExecution]
        static float FloatEpsilon => float.Epsilon;

        [CompileTimeExecution]
        static char NewLine => '\n';
        
        [CompileTimeExecution]
        static BindingFlags BitFlags => BindingFlags.Public | BindingFlags.Static;

        [CompileTimeExecution(deserialize: true)]
        static (string A, string B) Tuple => ("A", "B");

        [CompileTimeExecution(deserialize: true)]
        static (string A, string B) GetTuple() => ("A", "B");
#endif

        [TestMethod]
        public void Test()
        {
            Assert.IsTrue(DateTime.Parse(CompiledAt) < DateTime.Now);

            Assert.AreEqual(int.MinValue, MinInt);
            Assert.AreEqual(uint.MaxValue, MaxUInt);
            Assert.AreEqual(sbyte.MinValue, MinSByte);
            Assert.AreEqual(byte.MaxValue, MaxByte);
            Assert.AreEqual(long.MinValue, MinLong);
            Assert.AreEqual(ulong.MaxValue, MaxULong);

            Assert.AreEqual(float.Epsilon, FloatEpsilon);
            Assert.AreEqual(double.Epsilon, DoubleEpsilon);

            Assert.AreEqual('\n', NewLine);

            Assert.AreEqual(BindingFlags.Public | BindingFlags.Static, BitFlags);

            Assert.AreEqual("A", Tuple.A);
            Assert.AreEqual("B", Tuple.B);
            Assert.AreEqual("A", GetTuple().A);
            Assert.AreEqual("B", GetTuple().B);
        }
    }
}
