//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Text;
//using Moq;
//using VirtualStudio.Core.Abstractions;
//using System.Reflection;
//using VirtualStudio.Core.UpdateEvents;
//using System.Linq;

//namespace VirtualStudio.Core.Test
//{
//    [TestClass]
//    public class VirtualStudioEventManagerTest
//    {
//        [TestMethod]
//        public void Fires_events_when_the_VirtualStudio_changes()
//        {
//            var connectionFactoryMock = new Mock<IStudioConnectionFactory>();
//            var connectionMock = new Mock<StudioConnection>();
//            connectionFactoryMock.Setup(c => c.CanCreateStudioConnection(It.IsAny<StudioComponentOutput>(), It.IsAny<StudioComponentInput>())).Returns(true);
//            connectionFactoryMock.Setup(c => c.CreateStudioConnection(It.IsAny<StudioComponentOutput>(), It.IsAny<StudioComponentInput>())).Returns(connectionMock.Object);
//            var virtualStudio = new VirtualStudio(connectionFactoryMock.Object);
//            var clientComponentMock = new Mock<StudioComponent>();
//            clientComponentMock.SetupGet(c => c.Outputs).Returns(new List<StudioComponentOutput>() { new Mock<StudioComponentOutput>().Object });
//            clientComponentMock.SetupGet(c => c.Id).Returns(1);
//            var placeholderComponentMock = new Mock<PlaceholderStudioComponent>();
//            placeholderComponentMock.SetupGet(c => c.Inputs).Returns(new List<StudioComponentInput>() { new Mock<StudioComponentInput>().Object });
//            placeholderComponentMock.SetupGet(c => c.Id).Returns(2);
//            var inputMock = new Mock<StudioComponentInput>();
//            inputMock.SetupGet(i => i.Component).Returns(clientComponentMock.Object);
//            var outputMock = new Mock<StudioComponentOutput>();
//            outputMock.SetupGet(i => i.Component).Returns(clientComponentMock.Object);

//            var virtualStudioEventManager = new VirtualStudioEventManager(virtualStudio, "test");

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.RepositoryClientAdded),
//                () => virtualStudio.ComponentRepository.AddClient(clientComponentMock.Object)));           

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.RepositoryPlaceholderAdded),
//                () => virtualStudio.ComponentRepository.AddPlaceholder(placeholderComponentMock.Object)));         

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ComponentAdded),
//                () => virtualStudio.AddComponent(clientComponentMock.Object)));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ComponentRemoved),
//                () => virtualStudio.RemoveComponent(clientComponentMock.Object)));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ConnectionAdded),
//                () =>
//                {
//                    virtualStudio.AddComponent(clientComponentMock.Object);
//                    virtualStudio.AddComponent(placeholderComponentMock.Object);
//                    virtualStudio.CreateConnection(clientComponentMock.Object.Outputs.First(), placeholderComponentMock.Object.Inputs.First());
//                }));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ConnectionStateChanged),
//                () => connectionMock.Raise(c => c.StateChanged += null, connectionMock.Object, ConnectionState.Connected)));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ConnectionRemoved),
//                () => virtualStudio.RemoveConnection(virtualStudio.Connections.First())));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ComponentPropertyChanged),
//                () => clientComponentMock.Raise(c => c.PropertyChanged += null, clientComponentMock.Object, "Name")));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ComponentInputAdded),
//                () => clientComponentMock.Raise(c => c.InputAdded += null, clientComponentMock.Object, inputMock.Object)));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ComponentInputRemoved),
//                () => clientComponentMock.Raise(c => c.InputRemoved += null, clientComponentMock.Object, inputMock.Object)));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ComponentOutputAdded),
//                () => clientComponentMock.Raise(c => c.OutputAdded += null, clientComponentMock.Object, outputMock.Object)));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.ComponentOutputRemoved),
//                () => clientComponentMock.Raise(c => c.OutputRemoved += null, clientComponentMock.Object, outputMock.Object)));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//               nameof(virtualStudioEventManager.RepositoryClientRemoved),
//               () => virtualStudio.ComponentRepository.RemoveClient(clientComponentMock.Object)));

//            Assert.IsTrue(EventWatcher.FiresEvent(virtualStudioEventManager,
//                nameof(virtualStudioEventManager.RepositoryPlaceholderRemoved),
//                () => virtualStudio.ComponentRepository.RemovePlaceholder(placeholderComponentMock.Object)));

//        }
//    }
//}
