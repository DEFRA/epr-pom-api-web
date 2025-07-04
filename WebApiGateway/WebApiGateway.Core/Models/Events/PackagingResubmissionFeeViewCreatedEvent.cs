﻿using System.Diagnostics.CodeAnalysis;
using WebApiGateway.Core.Enumeration;

namespace WebApiGateway.Core.Models.Events;

[ExcludeFromCodeCoverage]
public class PackagingResubmissionFeeViewCreatedEvent : AbstractEvent
{
    public override EventType Type => EventType.PackagingResubmissionFeeViewed;

    public Guid? FileId { get; set; }

    public bool? IsPackagingResubmissionFeeViewed { get; set; }
}
