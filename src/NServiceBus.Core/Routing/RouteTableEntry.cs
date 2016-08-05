namespace NServiceBus.Routing
{
    using System;

    /// <summary>
    /// Represents an entry in a routing table.
    /// </summary>
    public class RouteTableEntry
    {
        /// <summary>
        /// Type of message.
        /// </summary>
        public Type MessageType { get; }
        /// <summary>
        /// Route for the message type.
        /// </summary>
        public IUnicastRoute Route { get; }
        /// <summary>
        /// Relative priority of this entry.
        /// </summary>
        public RoutePriority Priority { get; }

        /// <summary>
        /// Creates a new entry.
        /// </summary>
        public RouteTableEntry(Type messageType, IUnicastRoute route, RoutePriority priority)
        {
            MessageType = messageType;
            Route = route;
            Priority = priority;
        }
    }
}