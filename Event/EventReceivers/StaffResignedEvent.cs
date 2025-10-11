using MassTransit;
using StaffEvents;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.EventReceivers
{
    public class StaffResignedEvent : IConsumer<StafCreatedEvent>
    {
        public StaffResignedEvent()
        {
             
        }

        public Task Consume(ConsumeContext<StafCreatedEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}
