using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Arrangement;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.Core.Operations
{
    public class GetVirtualStudioWithArrangementQuery : IVirtualStudioQuery<VirtualStudioWithArrangementDto>
    {
        public OperationError Error { get; private set; }

        public Task<VirtualStudioWithArrangementDto> Process(VirtualStudio virtualStudio)
        {
            if (virtualStudio is VirtualStudioWithArrangement virtualStudioWithArrangement)
                return Task.FromResult(virtualStudioWithArrangement.ToDto());
            else
            {
                Error = new OperationError(ErrorType.NotFound, "A VirtualStudioWithArrangement was not found.");
                return Task.FromResult<VirtualStudioWithArrangementDto>(null);
            }
        }
    }
}
