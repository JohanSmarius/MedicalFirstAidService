using Event.Domain;
using Event.DomainServices;
using MassTransit;
using StaffEvents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.EventReceivers
{
    internal class StaffCreatedEventListener : IConsumer<StafCreatedEvent>
    {
        private readonly IStaffRepository staffRepository;

        public StaffCreatedEventListener(IStaffRepository staffRepository)
        {
            this.staffRepository = staffRepository;
        }

        public Task Consume(ConsumeContext<StafCreatedEvent> context)
        {
            var newStaffMember = new Staff()
            {
                ReferenceId = context.Message.Id,
                IsActive = true,
                Role = StaffRole.FirstAider,
            };

            staffRepository.CreateStaffAsync(newStaffMember).Wait();

            return Task.CompletedTask;
        }
    }
}
