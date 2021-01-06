using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Test
{
    [TestClass]
    public class VirtualStudioManagerTest
    {
        [TestMethod]
        public void Creates_a_VirtualStudio_instance()
        {
            var virtualStudioManager = new VirtualStudioRepository();
            string name = "testname";

            VirtualStudio virtualStudio = virtualStudioManager.GetVirtualStudio(name);

            Assert.IsNotNull(virtualStudio);
            Assert.IsTrue(virtualStudioManager.Count == 1);
        }

        [TestMethod]
        public void Returns_existing_VirtualStudio_instance()
        {
            var virtualStudioManager = new VirtualStudioRepository();
            string name = "testname";
            VirtualStudio virtualStudio = virtualStudioManager.GetVirtualStudio(name);

            VirtualStudio existingVirtualStudio = virtualStudioManager.GetVirtualStudio(name);

            Assert.IsNotNull(existingVirtualStudio);
            Assert.AreEqual(virtualStudio, existingVirtualStudio);
        }

        [TestMethod]
        public void Gets_all_VirtualStudio_IDs()
        {
            var virtualStudioManager = new VirtualStudioRepository();
            string name = "testname";
            string name2 = "testname2";
            virtualStudioManager.GetVirtualStudio(name);
            virtualStudioManager.GetVirtualStudio(name2);

            var virtualStudioIds = virtualStudioManager.GetVirtualStudioIds();
            var virtualStudioIdsEnumerator = virtualStudioIds.GetEnumerator();

            Assert.IsTrue(virtualStudioIds.Count() == 2);
            virtualStudioIdsEnumerator.MoveNext();
            Assert.AreEqual(name, virtualStudioIdsEnumerator.Current);
            virtualStudioIdsEnumerator.MoveNext();
            Assert.AreEqual(name2, virtualStudioIdsEnumerator.Current);
        }
    }
}
