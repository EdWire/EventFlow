﻿// The MIT License (MIT)
//
// Copyright (c) 2015 Rasmus Mikkelsen
// https://github.com/rasmus/EventFlow
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using EventFlow.Aggregates;

namespace EventFlow.EventStores
{
    public class DomainEventFactory : IDomainEventFactory
    {
        private readonly Dictionary<Type, Type> _aggregateEventToDomainEventTypeMap = new Dictionary<Type, Type>(); 

        public IDomainEvent<TAggregate, TIdentity> Create<TAggregate, TIdentity>(
            IAggregateEvent aggregateEvent,
            IMetadata metadata,
            long globalSequenceNumber,
            TIdentity id,
            int aggregateSequenceNumber,
            Guid batchId)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
        {
            var aggregateType = typeof (TAggregate);
            var aggregateEventType = aggregateEvent.GetType();
            Type domainEventType;
            if (!_aggregateEventToDomainEventTypeMap.TryGetValue(aggregateEventType, out domainEventType))
            {
                domainEventType = typeof(DomainEvent<,,>).MakeGenericType(aggregateType, id.GetType(), aggregateEventType);
                _aggregateEventToDomainEventTypeMap[aggregateEventType] = domainEventType;
            }

            var domainEvent = (IDomainEvent<TAggregate, TIdentity>)Activator.CreateInstance(
                domainEventType,
                aggregateEvent,
                metadata,
                metadata.Timestamp,
                globalSequenceNumber,
                id,
                aggregateSequenceNumber,
                batchId);

            return domainEvent;
        }

        public IDomainEvent<TAggregate, TIdentity> Upgrade<TAggregate, TIdentity>(
            IDomainEvent domainEvent,
            IAggregateEvent aggregateEvent)
            where TAggregate : IAggregateRoot<TIdentity>
            where TIdentity : IIdentity
        {
            return Create<TAggregate, TIdentity>(
                aggregateEvent,
                domainEvent.Metadata,
                domainEvent.GlobalSequenceNumber,
                (TIdentity) domainEvent.GetIdentity(),
                domainEvent.AggregateSequenceNumber,
                domainEvent.BatchId);
        }
    }
}