﻿namespace WebApiGateway.Api.Clients.Interfaces;

using Core.Models.Antivirus;

public interface IAntivirusClient
{
    Task SendFileAsync(FileDetails fileDetails, string fileName, Stream fileStream);

    Task<HttpResponseMessage> VirusScanFileAsync(FileDetails fileDetails, string fileName, MemoryStream fileStream);
}