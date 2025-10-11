using Event.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.DomainServices
{
    internal interface IStaffService
    {
        Task ResignStaffAsync(Staff staff);
    }
}
