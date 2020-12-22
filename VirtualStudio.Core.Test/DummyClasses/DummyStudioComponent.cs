using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core.Test.DummyClasses
{
    public class DummyStudioComponent : StudioComponentBase
    {
        public new void AddInput(IStudioInput input) => base.AddInput(input);

        public new void AddOutput(IStudioOutput output) => base.AddOutput(output);

        public new void RemoveInput(IStudioInput input) => base.RemoveInput(input);

        public new void RemoveOutput(IStudioOutput output) => base.RemoveOutput(output);
    }
}
