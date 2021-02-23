using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualStudio.Shared.DTOs
{
    public class StudioConnectionDto
    {
        public int Id { get; set; }
        public ConnectionState State { get; set; }
        public int OutputComponentId { get; set; }
        public int OutputId { get; set; }
        public int InputComponentId { get; set; }
        public int InputId { get; set; }

        public override string ToString()
        {
            return $"StudioConnectionDto: OutputComponentId: {OutputComponentId}, OutputId: {OutputId}, InputComponentId: {InputComponentId}, InputId: {InputId}, State = {State.ToString()}";
        }
    }
}
