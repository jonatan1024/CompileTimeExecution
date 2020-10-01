using Microsoft.VisualStudio.TestTools.UnitTesting;
#if CompileTimeExecution
using CompileTimeExecution;
#endif

#if CompileTimeExecution
class Naked {
    [CompileTimeExecution]
    public static bool OK => true;
}
#endif

namespace CompileTimeExecutionTests.Nested1
{
    namespace Nested2.Nested3 {
        [TestClass]
        public partial class NestedTest
        {
#if CompileTimeExecution
            class Nested1 {
                [CompileTimeExecution]
                public static bool OK => true;

                public class Nested2 {
                    [CompileTimeExecution]
                    public static bool OK => true;
                }
            }
#endif
            [TestMethod]
            public void Test()
            {
                Assert.IsTrue(Naked.OK);
                Assert.IsTrue(Nested1.OK);
                Assert.IsTrue(Nested1.Nested2.OK);
                Assert.IsTrue(Nested4.Nested.OK);
            }
        }
#if CompileTimeExecution
        namespace Nested4
        {
            class Nested {
                [CompileTimeExecution]
                public static bool OK => true;
            }
        }
#endif
    }
}
