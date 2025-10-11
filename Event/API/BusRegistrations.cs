using Event.EventReceivers;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Event.API
{
    public static class BusRegistrations
    {
        public static IBusRegistrationConfigurator AddEventListeners(this IBusRegistrationConfigurator config)
        {
            config.AddConsumer<StaffCreatedEventListener>();
            config.AddConsumer<StaffResignedEventListener>();
            return config;
        }
    }
}
