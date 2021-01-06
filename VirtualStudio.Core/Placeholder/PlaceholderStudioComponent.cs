using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class PlaceholderStudioComponent : StudioComponent
    {
        new public void AddInput(StudioComponentInput input)
        {
            base.AddInput(input);
        }

        new public void AddOutput(StudioComponentOutput output)
        {
            base.AddOutput(output);
        }

        new public void RemoveInput(StudioComponentInput input)
        {
            base.RemoveInput(input);
        }

        new public void RemoveOutput(StudioComponentOutput output)
        {
            base.RemoveOutput(output);
        }
    }
}
