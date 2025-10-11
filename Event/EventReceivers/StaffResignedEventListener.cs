using Event.DomainServices;
using MassTransit;
using StaffEvents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.EventReceivers
{
    internal class StaffResignedEventListener : IConsumer<StaffResignedEvent>
    {
        private readonly IStaffRepository staffRepository;
        private readonly IStaffService staffService;

        public StaffResignedEventListener(IStaffRepository staffRepository, IStaffService staffService)
        {
            this.staffRepository = staffRepository;
            this.staffService = staffService;
        }

        public Task Consume(ConsumeContext<StaffResignedEvent> context)
        {
            var staff = staffRepository.GetStaffByReferenceId(context.Message.Id).Result;
            if (staff != null)
            {
                staffService.ResignStaffAsync(staff).Wait();
            }
            
            return Task.CompletedTask;
        }
    }
}
