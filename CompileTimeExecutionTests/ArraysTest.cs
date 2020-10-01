using Microsoft.VisualStudio.TestTools.UnitTesting;
#if CompileTimeExecution
using CompileTimeExecution;
#endif

namespace CompileTimeExecutionTests
{
    [TestClass]
    public partial class ArraysTest
    {
        static int Factorial(int x) => x <= 1 ? 1 : Factorial(x - 1) * x;

#if CompileTimeExecution
        [CompileTimeExecution]
        static int[] Fac10Array
        {
            get
            {
                var facs = new int[10];
                for(int i = 0; i < 10; i++)
                {
                    facs[i] = Factorial(i + 1);
                }
                return facs;
            }
        }
        
        [CompileTimeExecution]
        static int[][] Triangle
        {
            get
            {
                const int triangleSize = 10;
                var triangle = new int[triangleSize][];
                int k = 0;
                for(int i = 0; i < triangleSize; i++)
                {
                    triangle[i] = new int[i + 1];
                    for(int j = 0; j <= i; j++)
                    {
                        triangle[i][j] = k++;
                    }
                }
                return triangle;
            }
        }
        
        [CompileTimeExecution]
        static int[,] Matrix
        {
            get
            {
                var matrix = new int[3, 4];
                for (int i = 0; i < 3; i++)
                    matrix[i, i] = 1;
                return matrix;
            }
        }
#endif

        [TestMethod]
        public void Test()
        {
            for(int x = 1; x <= 10; x++)
            {
                Assert.AreEqual(Factorial(x), Fac10Array[x - 1]);
            }

            var triangularNumbers = new int[] {0, 1, 3, 6, 10, 15, 21, 28, 36, 45};
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(triangularNumbers[i], Triangle[i][0]);
            }

            Assert.AreEqual(1, Matrix[0, 0]);
            Assert.AreEqual(1, Matrix[1, 1]);
            Assert.AreEqual(1, Matrix[2, 2]);
            Assert.AreEqual(0, Matrix[2, 3]);
        }
    }
}
