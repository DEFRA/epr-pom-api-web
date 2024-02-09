﻿namespace WebApiGateway.Core.Models.RegistrationValidation;

public class ColumnValidationError
{
    public string ErrorCode { get; set; }

    public int ColumnIndex { get; set; }

    public string ColumnName { get; set; }
}