using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using BunBundle.Model;

namespace UnitTests {
    class UtilsTest {

        [Test]
        public void ToFloatString() {

            (string expected, float number)[] values = new[] {
                ("5", 5),
                ("5.5f", 5.5f),
                ("0.1f", .1f)
            };

            foreach (var value in values) {
                Assert.AreEqual(value.expected, Utils.ToFloatString(value.number));
            }


        }

    }
}
