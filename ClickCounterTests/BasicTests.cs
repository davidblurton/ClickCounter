using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace ClickCounterTests
{
    public class BasicTests
    {
        [Test]
        public void ThisTestShouldPass()
        {
            Assert.Pass();
        }

        [Test]
        public void ThisTestShouldFail()
        {
            Assert.Fail("This was meant to fail :)");
        }
    }
}
