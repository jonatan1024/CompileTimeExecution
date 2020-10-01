using Microsoft.VisualStudio.TestTools.UnitTesting;
#if CompileTimeExecution
using CompileTimeExecution;
#endif

namespace CompileTimeExecutionTests
{
    [TestClass]
    public partial class SideEffectsTest
    {
#if CompileTimeExecution
        static int counter = 0;

        [CompileTimeExecution]
        static int Id1 => ++counter;

        [CompileTimeExecution]
        static int Id2 => ++counter;
#endif
        [TestMethod]
        public void Test()
        {
            Assert.IsTrue((Id1 == 1 && Id2 == 2) || (Id1 == 2 && Id2 == 1));
        }
    }
}
